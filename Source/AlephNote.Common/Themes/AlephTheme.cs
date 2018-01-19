using System;

namespace AlephNote.Common.Themes
{
	public sealed class AlephTheme
	{
		public string Name { get; set; }
		public Version Version { get; set; }
		public CompatibilityVersionRange Compatibility { get; set; }


		public ColorRef Scintilla_Background          { get; set; } = ColorRef.QUARTER_GRAY;
		public int      Scintilla_WhitespaceSize      { get; set; } = 1;
		public ColorRef Scintilla_WhitespaceColor     { get; set; } = ColorRef.BLACK;

		public ColorRef Scintilla_Markdown_Background { get; set; } = ColorRef.MAGENTA;

		public ScintillaStyleSpec Scintilla_Default            { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_Link               { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownDefault    { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownEmph       { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownStrongEmph { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownHeader     { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownCode       { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownUrl        { get; set; } = ScintillaStyleSpec.Empty();
		public ScintillaStyleSpec Scintilla_MarkdownList       { get; set; } = ScintillaStyleSpec.Empty();

		public IndicatorSpec      Scintilla_Search_Local       { get; set; } = IndicatorSpec.Empty();
		public IndicatorSpec      Scintilla_Search_Global      { get; set; } = IndicatorSpec.Empty();
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
