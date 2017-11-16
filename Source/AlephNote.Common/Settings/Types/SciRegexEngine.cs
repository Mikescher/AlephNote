namespace AlephNote.Common.Settings.Types
{
	public enum SciRegexEngine
	{
		[EnumDescriptor("Default Scintilla Regex engine")]
		Default,

		[EnumDescriptor("C++11 <regex> standard library engine")]
		CPlusPlus,

		[EnumDescriptor("POSIX compatible Regex engine")]
		Posix,
	}
}
