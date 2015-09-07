using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace noisebox
{
	class Utils
	{
		/// <summary>
		/// Parses the given string as an int and dumps the result into the given variable.
		/// An error message can be provided, which will be outputted to the console if an error occurs during parsing.
		/// </summary>
		/// <param name="str">The string to parse as an int.</param>
		/// <param name="destVar">The destination variable to dump the result into.</param>
		/// <param name="errorMessage">The error message to use if something goes wrong during parsing.</param>
		/// <returns>Whether we did actually manage to parse the given input.</returns>
		public static bool ParseInt(string str, out int destVar, string errorMessage)
		{
			try
			{
				destVar = int.Parse(str);
			}
			catch
			{
				Console.WriteLine(errorMessage);
				destVar = -1;
				return false;
			}

			return true;
		}
	}
}
