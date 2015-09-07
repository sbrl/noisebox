using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;

namespace noisebox
{
	class Program
	{
		static int Main(string[] args)
		{
			NoiseMaker noiseMaker = new NoiseMaker();

			for(int i = 0; i < args.Length; i++)
			{
				if(args[i].StartsWith("-"))
				{
					// This is a directive name
					string optionName = args[i].Trim(new char[]{ '-' }).ToLower();
					switch(optionName)
					{
						case "help":
							Console.WriteLine("Usage: ");
							Console.WriteLine("    noisebox [options] outfilename");
							Console.WriteLine("");
							Console.WriteLine("Options:");
							Console.WriteLine("    -size    1024x768    The size of the resulting image. ");
							Console.WriteLine("    -type    White       The type of noise to generate. Current types:");
							Console.WriteLine("                             White (default)");
							Console.WriteLine("                             Simplex");
							Console.WriteLine("                             OpenSimplex");
							Console.WriteLine("                             Perlin");
							Console.WriteLine("                             Worley");
							Console.WriteLine("    -distfunc  euclidean The distance function to use when rendering worley noise. Possible values:");
							Console.WriteLine("                             Euclidean (default)");
							Console.WriteLine("                             Manhattan (aka Taxicab)");
							Console.WriteLine("                             Chebyshev (aka Chessboard)");
							Console.WriteLine("    -colour  #ffffff     The colour of the resulting image. This is actually applied as multiplier, so a value of #ffffff (white, the default) will not change anything, while #000000 (black) will cause the output image to be completed black. This doesn't have any effect on the Worley algorithm yet.");
							Console.WriteLine("    -frames  1           The number of frames to create. Supporting noise algorithms increment the z value in each frame, creating an animation of sorts. Add a hash symbol (#) to the filename and it will be replaced by the frame number.");
							Console.WriteLine("    -offset  0           The offset in depth for the number of frame to render. Useful if the rendering of a large number of frame was interrupted and you want to resume.");
							Console.WriteLine("    -threads cpus - 1    The number of threads to use when rendering multiple frames. If not set one less than the total number of cpus will be used. At least one thread will always be spawned.");
							Console.WriteLine("    -verbose             Be more verbose in the console output.");
							return 0;

						case "size":
							string sizeText = args[i + 1];

							int delimIndex = sizeText.IndexOf('x');

							if(!sizeText.Contains("x"))
							{
								Console.WriteLine("Invalid size. Sizes should be something like '1024x768'.");
								return 1;
							}

							try
							{
								noiseMaker.Width = int.Parse(sizeText.Substring(0, delimIndex));
								noiseMaker.Height = int.Parse(sizeText.Substring(delimIndex + 1));
							}
							catch
							{
								Console.WriteLine("Invalid size '{0}'. Sizes should look something like this: 1024x768.", sizeText);
								return 1;
							}

							i++;
							break;

						case "type":
							string typeText = args[i + 1];

							try
							{
								noiseMaker.Type = (NoiseType)Enum.Parse(typeof(NoiseType), typeText, true);
							}
							catch
							{
								Console.WriteLine("Invalid type '{0}'. Currently supported types: ", typeText);
								foreach (string type in Enum.GetNames(typeof(NoiseType)))
								{
									Console.WriteLine("    {0}", type);
								}
								return 1;
							}

							i++;
							break;

						case "distfunc":
							string distFuncText = args[i + 1];

							try
							{
								noiseMaker.DistanceFunction = (WorleyNoise.DistanceFunction)Enum.Parse(typeof(WorleyNoise.DistanceFunction), distFuncText, true);
							}
							catch
							{
								Console.WriteLine("Invalid distance function '{0}'. Currently supported types: ", distFuncText);
								foreach (string func in Enum.GetNames(typeof(WorleyNoise.DistanceFunction)))
								{
									Console.WriteLine("    {0}", func);
								}
								return 1;
							}

							i++;
							break;
							
						case "frames":
							try
							{
								noiseMaker.FrameCount = int.Parse(args[i + 1]);
							}
							catch
							{
								Console.WriteLine("Invalid frame count {0}", args[i + 1]);
								return 1;
							}

							i++;
							break;

						case "offset":
							try
							{
								noiseMaker.FrameOffset = int.Parse(args[i + 1]);
							}
							catch
							{
								Console.WriteLine("Invalid offset {0}", args[i + 1]);
								return 1;
							}

							i++;
							break;

						case "threads":
							try
							{
								noiseMaker.ThreadCount = int.Parse(args[i + 1]);
							}
							catch
							{
								Console.WriteLine("Invalid thread count {0}", args[i + 1]);
								return 1;
							}

							i++;
							break;

						case "seed":
							try
							{
								noiseMaker.Seed = int.Parse(args[i + 1]);
							}
							catch
							{
								Console.WriteLine("Invalid seed {0}", args[i + 1]);
								return 1;
							}

							i++;
							break;

						case "colour":
							string rawColour = args[i + 1].Replace("0x", "").Replace("#", "");
							if(rawColour.Length != 6)
							{
								Console.WriteLine("The colourspec {0} isn't the right length. Colours should be in hex, like this: #ff3300");
								return 1;
							}

							try
							{
								byte red = byte.Parse(rawColour.Substring(0, 2), NumberStyles.HexNumber);
								byte green = byte.Parse(rawColour.Substring(2, 2), NumberStyles.HexNumber);
								byte blue = byte.Parse(rawColour.Substring(4), NumberStyles.HexNumber);
								
								noiseMaker.RedMultiplier = (red > 0) ? red / 255f : 0;
								noiseMaker.GreenMultiplier = (green > 0) ? green / 255f : 0;
								noiseMaker.BlueMultiplier = (blue > 0) ? blue / 255f : 0;
							}
							catch
							{
								Console.WriteLine("Invalid colourspec {0}. Colours should be in hex, like this: #ff3300");
								return 1;
							}

							i++;
							break;

						case "v":
						case "verbose":
							noiseMaker.Verbose = true;
							break;

						default:
							Console.WriteLine("Unknown option '{0}'.", optionName);
							return 1;
					}
				}
				else
				{
					if(i == args.Length - 1)
					{
						noiseMaker.OutFilename = args[i];
					}
					else
					{
						Console.WriteLine("Extra argument detected: {0}", args[i]);
					}
				}
			}

			noiseMaker.makeNoise();
			return 0;
		}
	}
}
