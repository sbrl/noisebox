using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*
 * Worley noise generator, by Starbeamrainbowlabs
 * License: Mozilla Public License 2.0
 * License link: https://www.mozilla.org/en-US/MPL/2.0/
 * 
 * Parts of this code were not written by Starbeamrainbowlabs. Credits are below:
	* FastFloor, a faster Math.Floor function: http://www.codeproject.com/Tips/700780/Fast-floor-ceiling-functions
	* FastSqrt, a faster square root function: http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
 * 
 */

namespace WorleyNoise
{
	enum DistanceFunction
	{
		Euclidean,
		Manhattan,
		Chebyshev
	}

	enum ValueFunction
	{
		DistClosest,
		SecondTakeFirst
	}

	class Worley
	{
		protected int seed;

		protected int squareSize = 256;
		protected int pointsPerSquare = 3;

		public DistanceFunction DistFunc = DistanceFunction.Euclidean;

		public Worley()
			: this(-1)
		{
		}
		public Worley(int inSeed)
		{
			if(inSeed == -1)
			{
				Random rng = new Random();
				seed = rng.Next();
			}
			else
			{
				seed = inSeed;
			}
		}

		/// <summary>
		/// Renders a 2D worley noise image on the given reference to a RGBA pixel data byte array.
		/// </summary>
		/// <param name="pixels">A reference to the array of pixels to use for rendering.</param>
		/// <param name="width">The width of the resulting image.</param>
		/// <param name="height">The height of the resulting image.</param>
		public void GenerateImage(ref byte[] pixels, int width, int height)
		{
			int squareWidth = width / squareSize;
			int squareHeight = height / squareSize;
			// Loop over each square in the image
			for (int sy = 0; sy * squareSize < width; sy++)
			{
				for (int sx = 0; sx * squareSize < height; sx++)
				{
					//Console.WriteLine("Rendering square ({0}, {1})", sx, sy);

					//Console.WriteLine("Y: {0}px to {1}px", sy * squareSize, (sy + 1) * squareSize);
					//Console.WriteLine("X: {0}px to {1}px", sx * squareSize, (sx + 1) * squareSize);

					// Grab the nearby points for this square
					Point2D[] nearbyPoints = getNearbyPoints(sx * squareSize, sy * squareSize);

					// Loop over each pixel in the square
					for (int py = sy * squareSize; py < (sy + 1) * squareSize; py++)
					{
						/* Similarly if the current pixel's y coordinate is greater than or equal to the height of the image,
						 * then we have reached the bottom edge of the image. We should stop here. */
						if (py >= height)
						{
							//Console.WriteLine("Breaking at a height of {0}px", py);
							break;
						}

						for (int px = sx * squareSize; px < (sx + 1) * squareSize; px++)
						{
							/* If the current pixel's x coordinate is greater than or equal to the width of the image,
							 * then we have reached the rightmost edge of the image. Break out of the loop here. */
							if (px >= width)
							{
								//Console.WriteLine("Breaking at ({0}, {1})", px, py);
								break;
							}
							// Get the distance to all the nearby points
							float[] distances = calculateDistances(px, py, nearbyPoints);

							// Sort the distances from smallest to largest, whilst keeping the array of nearby points in sync
							sortNumbers(ref distances);

							// Calculate the value for this pixel
							byte value = (byte)(Generate(distances) * 255);

							int curIndex = ((py * width) + px) * 4;
							pixels[curIndex] = (byte)(value);
							pixels[curIndex + 1] = (byte)(value);
							pixels[curIndex + 2] = (byte)(value);
						}

					}
				}
			}
		}

		/// <summary>
		/// Renders a 2D worley noise image slice from a 3D worley simulation on the given reference to a RGBA pixel data byte array.
		/// </summary>
		/// <param name="pixels">A reference to the array of pixels to use for rendering.</param>
		/// <param name="width">The width of the resulting image.</param>
		/// <param name="height">The height of the resulting image.</param>
		/// <param name="z">The depth at which to extract the slice.</param>
		public void GenerateImage(ref byte[] pixels, int width, int height, int z)
		{
			float[] rawValues = new float[width * height];
			// Store the maximum value we find
			float maxValue = 0;

			int squareWidth = width / squareSize;
			int squareHeight = height / squareSize;
			// Loop over each square in the image
			// We *don't* loop over the z axis because we are only rendering a _slice_ and not a cube.
			int currentBlock = 0;
			for (int sy = 0; sy * squareSize < height; sy++)
			{
				for (int sx = 0; sx * squareSize < width; sx++)
				{
					Console.Write("b{0} ", currentBlock);
					//Console.WriteLine("Rendering square ({0}, {1})", sx, sy);

					//Console.WriteLine("Y: {0}px to {1}px", sy * squareSize, (sy + 1) * squareSize);
					//Console.WriteLine("X: {0}px to {1}px", sx * squareSize, (sx + 1) * squareSize);

					// Grab the nearby points for this square
					/* Note that we don't need to multiple z by anything as it is already relative to the pixel.
					 * the sx and sy are in block mode and not pixel mode - so they need multiplying. */
					Point3D[] nearbyPoints = getNearbyPoints(sx * squareSize, sy * squareSize, z);

					// Loop over each pixel in the square
					for (int py = sy * squareSize; py < (sy + 1) * squareSize; py++)
					{
						/* Similarly if the current pixel's y coordinate is greater than or equal to the height of the image,
						 * then we have reached the bottom edge of the image. We should stop here. */
						if (py >= height)
						{
							//Console.WriteLine("Breaking at a height of {0}px", py);
							break;
						}

						for (int px = sx * squareSize; px < (sx + 1) * squareSize; px++)
						{
							/* If the current pixel's x coordinate is greater than or equal to the width of the image,
							 * then we have reached the rightmost edge of the image. Break out of the loop here. */
							if (px >= width)
							{
								//Console.WriteLine("Breaking at ({0}, {1})", px, py);
								break;
							}
							// Get the distance to all the nearby points
							float[] distances = calculateDistances(px, py, z, nearbyPoints);

							// Sort the distances from smallest to largest, whilst keeping the array of nearby points in sync
							sortNumbers(ref distances);

							// Calculate the value for this pixel
							float value = Math.Abs(/*distances[1] - */distances[0]);
							if (value > maxValue)
								maxValue = value;

							rawValues[(py * width) + px] = value;

							//byte value = (byte)(Generate(distances) * 255);

						}
					}

					currentBlock++;
				}
			}

			Console.WriteLine("Maximum value found: {0}", maxValue);

			// Copy the raw values to the actual image
			for (int y = 0; y < width; y++)
			{
				for (int x = 0; x < width; x++)
				{
					byte value = (byte)((rawValues[(y * width) + x] / maxValue) * 255);

					int curPixelIndex = ((y * width) + x) * 4;
					pixels[curPixelIndex] = (byte)(value);
					pixels[curPixelIndex + 1] = (byte)(value);
					pixels[curPixelIndex + 2] = (byte)(value);
				}
			}

				Console.WriteLine();
		}

		/// <summary>
		/// Generates a worley noise value for a given point int the range 0 to 1.
		/// </summary>
		/// <param name="x">The x co-ordinate of the point to generate.</param>
		/// <param name="y">The y co-ordinate of the point to generate.</param>
		/// <returns>The worley noise value for the given point in the range 0 to 1.</returns>
		public float Generate(int x, int y)
		{
			// Get a list of the locations of all the nearby points
			Point2D[] nearbyPoints = getNearbyPoints(x, y);
			// Create an array to store all the distances
			float[] distances = calculateDistances(x, y, nearbyPoints);

			// Sort the distances from smallest to largest, whilst keeping the array of nearby points in sync
			sortNumbers(ref distances);

			return Generate(distances);
		}

		/// <summary>
		/// Generates a worley noise value for a given point int the range 0 to 1.
		/// </summary>
		/// <param name="x">The x co-ordinate of the point to generate.</param>
		/// <param name="y">The y co-ordinate of the point to generate.</param>
		/// <returns>The worley noise value for the given point in the range 0 to 1.</returns>
		public double Generate(int x, int y, int z)
		{
			// Get a list of the locations of all the nearby points
			Point3D[] nearbyPoints = getNearbyPoints(x, y, z);
			// Create an array to store all the distances
			float[] distances = calculateDistances(x, y, z, nearbyPoints);

			// Sort the distances from smallest to largest, whilst keeping the array of nearby points in sync
			sortNumbers(ref distances);

			return Generate(distances);
		}

		/// <summary>
		/// A variant that takes the coordinate in question and a pre-calculated
		/// *sorted* set of distances to each of the nearby points.
		/// </summary>
		/// 
		/// 
		/// <param name="distances">An array of *sorted* distances to use in the calculation.</param>
		/// <returns></returns>
		public float Generate(float[] distances)
		{
			// larger minus smaller
			float value = distances[1] - distances[0];

			value /= squareSize;
			if (value > 1)
				value = 1;

			return value;
		}


		/// <summary>
		/// Calculate the distance from a single point to an array of points.
		/// </summary>
		/// <param name="x">The x coordinate of the starting point.</param>
		/// <param name="y">The y coordinate of the starting point.</param>
		/// <param name="nearbyPoints">An array of points for which to calculate the distance.</param>
		/// <returns>An array of distances.</returns>
		protected float[] calculateDistances(int x, int y, Point2D[] nearbyPoints)
		{
			// Create an array to store all the distances
			float[] distances = new float[nearbyPoints.Length];
			// Work out all the distances
			for (int i = 0; i < nearbyPoints.Length; i++)
			{
				distances[i] = getDistance(x, y, nearbyPoints[i].x, nearbyPoints[i].y);
			}

			return distances;
		}

		/// <summary>
		/// Calculate the distance from a single 3d point to an array of 3d points.
		/// </summary>
		/// <param name="x">The x coordinate of the starting point.</param>
		/// <param name="y">The y coordinate of the starting point.</param>
		/// <param name="nearbyPoints">An array of points for which to calculate the distance.</param>
		/// <returns>An array of distances.</returns>
		protected float[] calculateDistances(int x, int y, int z, Point3D[] nearbyPoints)
		{
			// Create an array to store all the distances
			float[] distances = new float[nearbyPoints.Length];
			// Work out all the distances
			for (int i = 0; i < nearbyPoints.Length; i++)
			{
				distances[i] = getDistance(x, y, z, nearbyPoints[i].x, nearbyPoints[i].y, nearbyPoints[i].z);
			}

			return distances;
		}

		/// <summary>
		/// Gets the position of all the points located somewhat near a given location.
		/// Since this implementation is based on squares, the square in which the point is located is calculated first,
		/// and then that square and all those adjacent to it (including the diagonals) are visited in turn.
		/// </summary>
		/// <param name="x">The x coordinate of the point.</param>
		/// <param name="y">The y coordinate of the point.</param>
		/// <returns>An array of all the nearby points.</returns>
		protected Point2D[] getNearbyPoints(int x, int y)
		{
			// Work out which square we're in
			Point2D centreSquare = calculateBlock(x, y);
			/* Create a new array to hold all the points we find. We will be
			 * checking 9 squares, so we need to create an array big enough to
			 * hold all the points we find. */
			Point2D[] points = new Point2D[pointsPerSquare * 9];
			int nextIndex = 0;

			for (int ix = centreSquare.x - 1; ix <= centreSquare.x + 1; ix++)
			{
				for (int iy = centreSquare.y - 1; iy <= centreSquare.y + 1; iy++)
				{
					// Hash the co-ordinates of the current square
					int squareCode = (ix + "," + iy + "s" + seed).GetHashCode();
					// Create a new random 
					Random squareGenerator = new Random(squareCode);
					for (int i = 0; i < pointsPerSquare; i++)
					{
						points[nextIndex] = new Point2D(
							squareGenerator.Next(0, squareSize) + (ix * squareSize),
							squareGenerator.Next(0, squareSize) + (iy * squareSize)
						);
						nextIndex++;
					}
				}
			}

			return points;
		}

		/// <summary>
		/// Gets the position of all the points located somewhat near a given location.
		/// Since this implementation is based on cubes, the cube in which the point is located is calculated first,
		/// and then that cube and all those adjacent to it (including the diagonals) are visited in turn.
		/// </summary>
		/// <param name="x">The x coordinate of the point.</param>
		/// <param name="y">The y coordinate of the point.</param>
		/// <param name="z">The z coordinate of the point.</param>
		/// <returns>An array of all the nearby points.</returns>
		protected Point3D[] getNearbyPoints(int x, int y, int z)
		{
			// Work out which square we're in
			Point3D centreSquare = calculateBlock(x, y, z);
			/* Create a new array to hold all the points we find. We will be
			 * checking 27 cubes, so we need to create an array big enough to
			 * hold all the points we find. */
			Point3D[] points = new Point3D[pointsPerSquare * 3 * 3 * 3];
			int nextIndex = 0;

			for (int ix = centreSquare.x - 1; ix <= centreSquare.x + 1; ix++)
			{
				for (int iy = centreSquare.y - 1; iy <= centreSquare.y + 1; iy++)
				{
					for (int iz = centreSquare.z - 1; iz <= centreSquare.z + 1; iz++)
					{
						// Hash the co-ordinates of the current square
						int squareCode = (ix + "," + iy + "," + iz + "s" + seed).GetHashCode();
						// Create a new random 
						Random squareGenerator = new Random(squareCode);
						for (int i = 0; i < pointsPerSquare; i++)
						{
							points[nextIndex] = new Point3D(
								squareGenerator.Next(0, squareSize) + (ix * squareSize),
								squareGenerator.Next(0, squareSize) + (iy * squareSize),
								squareGenerator.Next(0, squareSize) + (iz * squareSize)
							);
							nextIndex++;
						}
					}
				}
			}

			return points;
		}

		/// <summary>
		/// Calculates which block a given co-ordinate in located in.
		/// </summary>
		/// <param name="x">The x position of the co-ordinate.</param>
		/// <param name="y">The y position of the co-ordinate.</param>
		/// <returns>A Point2D representing the containing block's co-ordinates.</returns>
		protected Point2D calculateBlock(int x, int y)
		{
			return new Point2D(FastFloor((float)(x) / squareSize), FastFloor((float)(y) / squareSize));
		}

		/// <summary>
		/// Calculates which block a given co-ordinate in located in.
		/// </summary>
		/// <param name="x">The x position of the co-ordinate.</param>
		/// <param name="y">The y position of the co-ordinate.</param>
		/// <param name="z">The z position of the co-ordinate.</param>
		/// <returns>A Point3D representing the containing block's co-ordinates.</returns>
		protected Point3D calculateBlock(int x, int y, int z)
		{
			return new Point3D(
				FastFloor((float)(x / squareSize)),
				FastFloor((float)(y / squareSize)),
				FastFloor((float)(z / squareSize))
			);
		}

		/// <summary>
		/// Calculates the distance between two points.
		/// </summary>
		/// <param name="x1">The x of the first point.</param>
		/// <param name="y1">The y of the first point.</param>
		/// <param name="x2">The x of the second point.</param>
		/// <param name="y2">The y of the second point.</param>
		/// <returns>The distance between the two points.</returns>
		protected float getDistance(int x1, int y1, int x2, int y2)
		{
			float distance_x = x1 - x2;
			float distance_y = y1 - y2;

			switch(DistFunc)
			{
				case DistanceFunction.Euclidean:
					return FastSqrt.Sqrt((float)((distance_x * distance_x) + (distance_y * distance_y)));

				case DistanceFunction.Manhattan:
					return distance_x + distance_y;

				case DistanceFunction.Chebyshev:
					return Math.Max(distance_x, distance_y);
			}

			throw new Exception("Unknown distance function " + DistFunc.ToString());
		}

		/// <summary>
		/// Calculates the distance between two points.
		/// </summary>
		/// <param name="x1">The x of the first point.</param>
		/// <param name="y1">The y of the first point.</param>
		/// <param name="y1">The z of the first point.</param>
		/// <param name="x2">The x of the second point.</param>
		/// <param name="y2">The y of the second point.</param>
		/// <param name="y2">The z of the second point.</param>
		/// <returns>The distance between the two points.</returns>
		protected float getDistance(int x1, int y1, int z1, int x2, int y2, int z2)
		{
			float distance_x = x1 - x2;
			float distance_y = y1 - y2;
			float distance_z = z1 - z2;

			switch (DistFunc)
			{
				case DistanceFunction.Euclidean:
					return FastSqrt.Sqrt((float)(
						(distance_x * distance_x) +
						(distance_y * distance_y) +
						(distance_z * distance_z))
					);

				case DistanceFunction.Manhattan:
					return distance_x + distance_y + distance_z;

				case DistanceFunction.Chebyshev:
					return Math.Max(distance_x, Math.Max(distance_y, distance_z));
			}

			throw new Exception("Unknown distance function " + DistFunc.ToString());
		}

		/// <summary>
		/// Performs an insertion sort on an array of numbers, while keeping an associated array of points in the same order.
		/// </summary>
		/// <param name="array">A reference to the array to sort.</param>
		/// <param name="points">A reference to the array of points to keep in sync with the array of numbers.</param>
		protected void sortNumbers(ref float[] array, ref Point2D[] points)
		{
			for (int i = array.Length - 2; i >= 0; i--)
			{
				int shp = i;
				//                                                      |
				//make sure that we don't fall off the end of the array V
				while (shp < array.Length - 1 && array[shp] > array[shp + 1])
				{
					// Swap the pair
					//swap_places(ref array, shp, shp + 1);
					float tempNumber = array[shp];
					array[shp] = array[shp + 1];
					array[shp + 1] = tempNumber;

					// Keep  the points in sync
					Point2D tempPoint = points[shp];
					points[shp] = points[shp + 1];
					points[shp + 1] = tempPoint;


					shp++;
				}
			}
		}
		/// <summary>
		/// Performs an insertion sort on an array of numbers.
		/// </summary>
		/// <param name="array">A reference to the array to sort.</param>
		protected void sortNumbers(ref float[] array)
		{
			for (int i = array.Length - 2; i >= 0; i--)
			{
				int shp = i;
				//                                                      |
				//make sure that we don't fall off the end of the array V
				while (shp < array.Length - 1 && array[shp] > array[shp + 1])
				{
					// Swap the pair
					//swap_places(ref array, shp, shp + 1);
					float tempNumber = array[shp];
					array[shp] = array[shp + 1];
					array[shp + 1] = tempNumber;

					shp++;
				}
			}
		}

		/// <summary>
		/// A faster floor function.
		/// From http://www.codeproject.com/Tips/700780/Fast-floor-ceiling-functions
		/// </summary>
		/// <param name="n">The number to round down.</param>
		/// <returns>The rounded down number.</returns>
		protected int FastFloor(float n)
		{
			int i = (int)n;
			if (i > n)
				i--;
			return i;
		}

		// From http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
		protected class FastSqrt
		{
			public static float Sqrt(float z)
			{
				if (z == 0) return 0;
				FloatIntUnion u;
				u.tmp = 0;
				u.f = z;
				u.tmp -= 1 << 23; /* Subtract 2^m. */
				u.tmp >>= 1; /* Divide by 2. */
				u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
				return u.f;
			}

			[StructLayout(LayoutKind.Explicit)]
			private struct FloatIntUnion
			{
				[FieldOffset(0)]
				public float f;

				[FieldOffset(0)]
				public int tmp;
			}
		}

		protected class Point2D
		{
			public int x;
			public int y;

			public Point2D(int inX, int inY)
			{
				x = inX;
				y = inY;
			}
		}

		protected class Point3D
		{
			public int x;
			public int y;
			public int z;

			public Point3D(int inX, int inY, int inZ)
			{
				x = inX;
				y = inY;
				z = inZ;
			}
		}

	}
}
