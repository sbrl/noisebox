using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace noisebox
{
	enum NoiseType
	{
		White,
		Simplex,
		OpenSimplex,
		Perlin,
		Worley
	}

	class NoiseMaker
	{
		public int Width = 1024;
		public int Height = 1024;
		
		public bool Verbose = false;
		public string OutFilename = "noise.png";
		public int FrameCount = 1;
		public int FrameOffset = 0;

		public int ThreadCount = -1;

		public float RedMultiplier = 1;
		public float GreenMultiplier = 1;
		public float BlueMultiplier = 1;

		public NoiseType Type = NoiseType.White;
		public WorleyNoise.DistanceFunction DistanceFunction = WorleyNoise.DistanceFunction.Euclidean;

		public long Seed = -1;

		protected Random rng;
		protected Stopwatch timer;

		protected object frameIndexSync = new object();
		protected int frameIndex = 0;

		public void makeNoise()
		{
			timer = Stopwatch.StartNew();
			if (Seed == -1)
			{
				rng = new Random();
				Seed = rng.Next();
			}
			else
			{
				rng = new Random((int)Seed);
			}
			

			switch (Type)
			{
				case NoiseType.Simplex:
					simplexNoseSetup();
					break;
			}
			if (Verbose)
				Console.WriteLine("[ {0,6}ms ] Initial setup complete", timer.ElapsedMilliseconds);

			// Set the initial frameIndex to the offset
			frameIndex = FrameOffset;

			// Calculate the number of threads that we should use, but only if it hasn't been set already
			if(ThreadCount == -1)
			{
				ThreadCount = Environment.ProcessorCount - 1;
				// Make sure that there will always be at least one thread
				if (ThreadCount < 1) ThreadCount = 1;
			}

			if (ThreadCount > FrameCount)
				ThreadCount = FrameCount;
			

			/*
			for (int i = frameOffset; i < frameOffset + frameCount; i++)
			{
				renderFrame(i);
			}
			 */

			Thread[] threadPool = new Thread[ThreadCount];

			for (int i = 0; i < ThreadCount; i++)
			{
				threadPool[i] = new Thread(new ParameterizedThreadStart(renderFrames));
				threadPool[i].Start(i);
			}
		}

		/// <summary>
		/// Method that calls getNextFrameIndex() repeatedly and renders each frameIndex returned.
		/// Stops when getNextFrameIndex() returns -1.
		/// </summary>
		/// <param name="threadID">The ID of the thread. Used when writing messages to the console.</param>
		protected void renderFrames(object threadIDObj)
		{
			int threadID = (int)threadIDObj;
			// Create a variable to hold the next frame index that we need to work on
			int nextFrameIndex = getNextFrameIndex();
			do
			{
				// Render the next frame that we've been given
				renderFrame(nextFrameIndex, threadID);

				// Get the next frame index
				nextFrameIndex = getNextFrameIndex();
			} while (nextFrameIndex != -1);
		}

		/// <summary>
		/// Gets the next frame index that needs rendering. Returns -1 if no frames are left to render.
		/// </summary>
		/// <returns>The index of the frame that needs rendering next.</returns>
		protected int getNextFrameIndex()
		{
			// Lock the frameIndexSync in order to ensure that only one thread is running this code at a time
			lock(frameIndexSync)
			{
				// limit: frameOffset + frameCount;
				// Increment the frame counter
				frameIndex++;
				// Work out if we have reached the end
				if(frameIndex >= FrameOffset + FrameCount)
				{
					return -1;
				}
				return frameIndex;
			}
		}

		protected void renderFrame(int i, int threadID)
		{
			Console.Write("[ {0,6}ms ] [ Thread {1} ] Frame {2} ", timer.ElapsedMilliseconds, threadID, i);
			// If the renderer doesn't support extra block progress indicators, write a newline character.
			if (Type != NoiseType.Worley)
				Console.WriteLine();


			byte[] pixels = new byte[Width * Height * 4];
			// Set the alpha to opaque
			for (int p = 3; p < pixels.Length; p += 4)
				pixels[p] = 255;

			if (Verbose)
			{
				Console.WriteLine("[ {0,6}ms ] [ Thread {1} ] Created new pixeldata array of size {2}x{3}.", timer.ElapsedMilliseconds, threadID, Width, Height);
				Console.WriteLine("[ {0,6}ms ] [ Thread {1} ] Generating noise of type {2}.", timer.ElapsedMilliseconds, threadID, Type.ToString());
			}
			switch (Type)
			{
				case NoiseType.White:
					whiteNoise(ref pixels, i);
					break;

				case NoiseType.Simplex:
					simplexNoise(ref pixels, i);
					break;

				case NoiseType.OpenSimplex:
					openSimplexNoise(ref pixels, i);
					break;

				case NoiseType.Perlin:
					perlinNoise(ref pixels, i);
					break;

				case NoiseType.Worley:
					worleyNoise(ref pixels, i);
					break;
			}

			if (Verbose)
				Console.WriteLine("[ {0,6}ms ] [ Thread {1} ] Populated pixel data with noise.", timer.ElapsedMilliseconds, threadID);

			GCHandle gchPixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			Bitmap image = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, gchPixels.AddrOfPinnedObject());

			if (Verbose)
				Console.WriteLine("[ {0,6}ms ] [ Thread {1} ] Converted pixel data to image.", timer.ElapsedMilliseconds, threadID);

			int padCount = OutFilename.Split('#').Length - 1;
			string filename = OutFilename;
			if (padCount > 0)
			{
				filename = filename.Replace(new String('#', padCount), i.ToString().PadLeft(padCount, '0'));
			}

			// Grab the file extension (without the dot)
			string fileExt = filename.Substring(filename.LastIndexOf(".") + 1).ToLower();

			ImageFormat imageFormat;
			switch (fileExt)
			{
				case "png":
					imageFormat = ImageFormat.Png;
					break;
				case "jpeg":
				case "jpg":
					imageFormat = ImageFormat.Jpeg;
					break;
				case "gif":
					imageFormat = ImageFormat.Gif;
					break;
				case "tif":
				case "tiff":
					imageFormat = ImageFormat.Tiff;
					break;
				case "bmp":
					imageFormat = ImageFormat.Bmp;
					break;

				default:
					Console.WriteLine("Unknown file type {0}.", fileExt);
					return;
			}

			image.Save(filename, imageFormat);
			// Free the memory associated with the image's pixels
			gchPixels.Free();

			if (Verbose)
				Console.WriteLine("[ {0,6}ms ] [ Thread {1} ] Written image to {2}", timer.ElapsedMilliseconds, threadID, filename);
		}

		/// <summary>
		/// Generates white noise.
		/// </summary>
		/// <param name="pixels">A reference to the pixel array to generate the noise on.</param>
		protected void whiteNoise(ref byte[] pixels, int z)
		{
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					int curIndex = (((y * Width) + x) * 4) % pixels.Length;
					byte value = (byte)rng.Next(0, 255);

					pixels[curIndex] = (byte)(value * BlueMultiplier);
					pixels[curIndex + 1] = (byte)(value * GreenMultiplier);
					pixels[curIndex + 2] = (byte)(value * RedMultiplier);
				}
			}
		}

		protected void simplexNoseSetup()
		{
			byte[] simplexSeed = new byte[512];
			rng.NextBytes(simplexSeed);
			SimplexNoise.Noise.perm = simplexSeed;
			if(Verbose)
				Console.WriteLine("[ {0,6}ms ] Generated simplex seed.", timer.ElapsedMilliseconds);
		}

		/// <summary>
		/// Generates simplex noise using Heikki Törmälä's SimplexNoise class.
		/// </summary>
		/// <param name="pixels">A reference to the pixel array to generate the noise on.</param>
		protected void simplexNoise(ref byte[] pixels, int z)
		{
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					int curIndex = ((y * Width) + x) * 4;
					float value = ((SimplexNoise.Noise.Generate(x / 80f, y / 80f, z / 80f) + 1) / 2) * 255;

					pixels[curIndex] = (byte)(value * BlueMultiplier);
					pixels[curIndex + 1] = (byte)(value * GreenMultiplier);
					pixels[curIndex + 2] = (byte)(value * RedMultiplier);
				}
			}
		}

		protected void openSimplexNoise(ref byte[] pixels, int z)
		{
			OpenSimplexNoise.OpenSimplex openSimplex = new OpenSimplexNoise.OpenSimplex();
			for(int y = 0; y < Height; y++)
			{
				for(int x = 0; x < Width; x++)
				{
					int curIndex = ((y * Width) + x) * 4;
					double value = ((openSimplex.Evaluate(x / 100f, y / 100f, z / 100f) + 1) / 2) * 255;

					pixels[curIndex] = (byte)(value * BlueMultiplier);
					pixels[curIndex + 1] = (byte)(value * GreenMultiplier);
					pixels[curIndex + 2] = (byte)(value * RedMultiplier);
				}
			}
		}

		protected void perlinNoise(ref byte[] pixels, int z)
		{
			PerlinNoise.Perlin perlin = new PerlinNoise.Perlin((int)Seed);
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					int curIndex = ((y * Width) + x) * 4;
					double value = ((perlin.Noise(x / 100f, y / 100f, z / 100f) + 1) / 2) * 255;

					pixels[curIndex] = (byte)(value * BlueMultiplier);
					pixels[curIndex + 1] = (byte)(value * GreenMultiplier);
					pixels[curIndex + 2] = (byte)(value * RedMultiplier);
				}
			}
		}

		protected void worleyNoise(ref byte[] pixels, int z)
		{
			// Create a new owrley noise generator
			WorleyNoise.Worley worley = new WorleyNoise.Worley((int)Seed);
			// set the distance function
			worley.DistFunc = DistanceFunction;
			// Render the image
			worley.GenerateImage(ref pixels, Width, Height, z);
			/*
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int curIndex = ((y * width) + x) * 4;
					double value = worley.Generate(x, y) * 255;

					//Console.WriteLine(value);

					pixels[curIndex] = (byte)(value * blue_multiplier);
					pixels[curIndex + 1] = (byte)(value * green_multiplier);
					pixels[curIndex + 2] = (byte)(value * red_multiplier);
				}
			}
			*/
		}
	}
}
