using System.Linq;
using System.Text;

namespace AlephNote
{
	public static class FilenameHelper
	{
		private const char CONVERT_ESCAPE_CHARACTER = '%';
		private const string ALLOWED_CHARACTER = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~#+-_.,;%& {}()=ÄÖÜäöüµ@";

		private static readonly string[] RESERVED_FILENAMES = new[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

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

			if (RESERVED_FILENAMES.Any(r => r.ToLower() == fileName.ToLower())) fileName = "_" + fileName;
			if (RESERVED_FILENAMES.Any(r => fileName.ToLower().StartsWith(r.ToLower() + "."))) fileName = "_" + fileName;

			return fileName;
		}

		public static string StripStringForFilename(string input)
		{
			StringBuilder output = new StringBuilder(input.Length);

			foreach (var c in input)
			{
				if (ALLOWED_CHARACTER.Contains(c)) output.Append(c);
			}

			var fileName = output.ToString();

			if (RESERVED_FILENAMES.Any(r => r.ToLower() == fileName.ToLower())) fileName = "_" + fileName;
			if (RESERVED_FILENAMES.Any(r => fileName.ToLower().StartsWith(r.ToLower() + "."))) fileName = "_" + fileName;

			return fileName;
		}
	}
}
