using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Transitions.Utilities
{
	public static class ConfigParser
	{
		static Regex parser = new Regex(@"(?:\s|^)([^""\s]+| ""(?:[^""]|"""")*"")?");
		public static async Task<List<string[]>> ParseAsync(string fileLocation)
		{
			List<string[]> ConfigLines = new List<string[]>();
			using (var fs = File.OpenRead(Path.Combine(CommonUtilities.RootDirectory, "transitions.txt")))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
					{
						string line = await sr.ReadLineAsync();

						string[] bits = parser.Split(line);
						ConfigLines.Add(bits.Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b.Trim(' ', '"')).ToArray());
					}
				}
			}
			return ConfigLines;
		}
		public static List<string[]> Parse(string fileLocation)
		{
			List<string[]> ConfigLines = new List<string[]>();
			using (var fs = File.OpenRead(Path.Combine(CommonUtilities.RootDirectory, "transitions.txt")))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine();

						string[] bits = parser.Split(line);
						ConfigLines.Add(bits.Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b.Trim(' ', '"')).ToArray());
					}
				}
			}
			return ConfigLines;
		}
	}
}
