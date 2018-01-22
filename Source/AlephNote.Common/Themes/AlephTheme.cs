using System;
using System.Diagnostics;

namespace AlephNote.Common.Themes
{
	[DebuggerDisplay("{Name} v{Version} ({SourceFilename})")]
	public sealed class AlephTheme
	{
		public string Name { get; set; }
		public Version Version { get; set; }
		public CompatibilityVersionRange Compatibility { get; set; }
		public string SourceFilename { get; set; }


		public ColorRef Window_Background                      { get; set; } = ColorRef.MAGENTA;
		public BrushRef Window_MainMenu_Background             { get; set; } = BrushRef.BLACK;
		public ColorRef Window_MainMenu_Foreground             { get; set; } = ColorRef.WHITE;
		public ColorRef Window_NoteTitle_Foreground            { get; set; } = ColorRef.WHITE;
		public ColorRef Window_ChangeDate_Foreground           { get; set; } = ColorRef.WHITE;


		public ColorRef Scintilla_Background                   { get; set; } = ColorRef.HALF_GRAY;

		public int      Scintilla_WhitespaceSize               { get; set; } = 4;
		public ColorRef Scintilla_WhitespaceColor              { get; set; } = ColorRef.WHITE;
		public ColorRef Scintilla_WhitespaceBackground         { get; set; } = ColorRef.BLACK;

		public ColorRef Scintilla_MarginLineNumbersColor       { get; set; } = ColorRef.BLACK;
		public ColorRef Scintilla_MarginListSymbolsColor       { get; set; } = ColorRef.BLACK;

		public ColorRef Scintilla_CaretForeground              { get; set; } = ColorRef.BLACK;
		public ColorRef Scintilla_CaretBackground              { get; set; } = ColorRef.BLACK;
		public int      Scintilla_CaretBackgroundAlpha         { get; set; } = 256;
		public bool     Scintilla_CaretVisible                 { get; set; } = false;

		public ScintillaStyleSpec Scintilla_Default            { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_Link               { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownDefault    { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownEmph       { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownStrongEmph { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownHeader     { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownCode       { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownList       { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownURL        { get; set; } = ScintillaStyleSpec.Empty();

		public IndicatorSpec      Scintilla_Search_Local       { get; set; } = IndicatorSpec.Empty();
		public IndicatorSpec      Scintilla_Search_Global      { get; set; } = IndicatorSpec.Empty();

		public AlephTheme(string n, Version v, CompatibilityVersionRange c, string fn)
		{
			Name = n;
			Version = v;
			Compatibility = c;
			SourceFilename = fn;
		}
	}

	public struct ScintillaStyleSpec
	{
		public ColorRef Foreground { get; set; }
		public ColorRef Background { get; set; }
		public bool     Underline  { get; set; }
		public bool     Bold       { get; set; }
		public bool     Italic     { get; set; }

		public static ScintillaStyleSpec Empty()
		{
			return new ScintillaStyleSpec
			{
				Foreground = ColorRef.MAGENTA,
				Background = ColorRef.HALF_GRAY,
				Underline  = true,
				Bold       = true,
				Italic     = true,
			};
		}
	}

	public struct IndicatorSpec
	{
		public ColorRef Color        { get; set; }
		public int      Alpha        { get; set; }
		public int      OutlineAlpha { get; set; }
		public bool     UnderText    { get; set; }

		public static IndicatorSpec Empty()
		{
			return new IndicatorSpec
			{
				Color        = ColorRef.WHITE,
				Alpha        = 255,
				OutlineAlpha = 255,
				UnderText    = true,
			};
		}
	}
}
