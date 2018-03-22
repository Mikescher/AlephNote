using System;
using System.Linq;
using System.Text;

namespace AlephNote.PluginInterface.Util
{
	public static class FilenameHelper
	{
		private const char CONVERT_ESCAPE_CHARACTER = '%';
		private const string ALLOWED_CHARACTER = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~#+-_.,;%& {}()=ÄÖÜäöüµ@";

		private static readonly string[] RESERVED_FILENAMES =
		{
			"CON", "PRN", "AUX", "CLOCK$", "NUL",
			"COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
			"LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
			".GIT", ".NOTES"
		};

		public static string ConvertStringForFilename(string input)
		{
			StringBuilder output = new StringBuilder(input.Length);

			foreach (var c in input)
			{
				if (c == CONVERT_ESCAPE_CHARACTER)
				{
					output.Append(CONVERT_ESCAPE_CHARACTER);
					output.Append(string.Format("{0:X4}", (int)c));
				}
				else if (ALLOWED_CHARACTER.Contains(c))
				{
					output.Append(c);
				}
				else
				{
					output.Append(CONVERT_ESCAPE_CHARACTER);
					output.Append(string.Format("{0:X4}", (int)c));
				}
			}

			var fileName = output.ToString();

			if (fileName.EndsWith("."))
			{
				fileName = fileName.Substring(0, fileName.Length-1) + CONVERT_ESCAPE_CHARACTER + string.Format("{0:X4}", (int)'.');
			}

			if (RESERVED_FILENAMES.Any(r => r.ToLower() == fileName.ToLower())) 
			{
				fileName = CONVERT_ESCAPE_CHARACTER + string.Format("{0:X4}", (int)fileName[0]) + fileName.Substring(1);
			}

			return fileName;
		}

		public static string ConvertStringFromFilenameBack(string input)
		{
			StringBuilder output = new StringBuilder(input.Length);

			for (int i = 0; i < input.Length; i++)
			{
				var c = input[i];

				if (c == CONVERT_ESCAPE_CHARACTER && i + 4 < input.Length)
				{
					string n = "";
					n += input[++i];
					n += input[++i];
					n += input[++i];
					n += input[++i];
					output.Append((char)Convert.ToInt32(n, 16));
				}
				else
				{
					output.Append(c);
				}
			}

			return output.ToString();
		}

		public static string StripStringForFilename(string input, char? repl = null)
		{
			StringBuilder output = new StringBuilder(input.Length);

			foreach (var c in input)
			{
				if (ALLOWED_CHARACTER.Contains(c))
				{
					output.Append(c);
				}
				else
				{
					if (repl != null) output.Append(repl);
				}
			}

			var fileName = output.ToString();

			if (RESERVED_FILENAMES.Any(r => r.ToLower() == fileName.ToLower())) fileName = "_" + fileName;
			if (RESERVED_FILENAMES.Any(r => fileName.ToLower().StartsWith(r.ToLower() + "."))) fileName = "_" + fileName;

			return fileName;
		}
	}
}
