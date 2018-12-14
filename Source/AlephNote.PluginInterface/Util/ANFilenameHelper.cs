using AlephNote.PluginInterface.AppContext;
using MSHC.Util.Helper;

namespace AlephNote.PluginInterface.Util
{
	public static class ANFilenameHelper
	{
		private static FilenameHelper.ValidityMode GetVMode()
		{
			if (AlephAppContext.Settings.AllowAllCharactersInFilename) return FilenameHelper.ValidityMode.AllowAllCharacters;
			if (AlephAppContext.Settings.AllowAllLettersInFilename) return FilenameHelper.ValidityMode.AllowAllLetters;
			return FilenameHelper.ValidityMode.AllowWhitelist;
		}

		public static string StripStringForFilename(string input, char? repl = null) => FilenameHelper.StripStringForFilename(input, GetVMode(), repl);

		public static string ConvertStringForFilename(string input) => FilenameHelper.ConvertStringForFilename(input, GetVMode());

		public static string ConvertStringFromFilenameBack(string input) => FilenameHelper.ConvertStringFromFilenameBack(input);
	}
}
