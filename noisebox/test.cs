using System;
using WorleyNoise;

public class Program
{
	public static void Main()
	{
		Worley engine = new Worley(1000);
		engine.Generate(324, 132);
	}
}