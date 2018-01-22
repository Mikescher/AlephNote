using ScintillaNET;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephNote.Common.Settings.Types;
using System.Text;
using System.Drawing;
using AlephNote.Common.Settings;
using AlephNote.Common.Themes;
using AlephNote.WPF.Extensions;

namespace AlephNote.WPF.Util
{
	public abstract class ScintillaHighlighter
	{
		public static readonly Regex REX_URL = new Regex(@"(?:(?:(?:(?:(?:http|https|ftp|file|irc)://[\w\.\-_äöü]+\.\w\w+)|(?:www\.[\w\.\-_äöü]+\.\w\w+))[/\w\?=\&\-\#\%\.]*)|(?:mailto:(?:[äöü\w]+(?:[._\-][äöü\w]+)*)@(?:[äöü\w\-]+(?:[.-][äöü\w]+)*\.[a-z]{2,})))(?=(\s|$))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public const int STYLE_DEFAULT    = 0;
		public const int STYLE_URL        = 1;
		public const int STYLE_MD_DEFAULT = 2;
		public const int STYLE_MD_BOLD    = 3;
		public const int STYLE_MD_HEADER  = 4;
		public const int STYLE_MD_CODE    = 5;
		public const int STYLE_MD_ITALIC  = 6;
		public const int STYLE_MD_URL     = 7;
		public const int STYLE_MD_LIST    = 8;

		public const int STYLE_MARGIN_LINENUMBERS = 0;
		public const int STYLE_MARGIN_LISTSYMBOLS = 1;

		public const int STYLE_MARKER_LIST_ON  = 2;
		public const int STYLE_MARKER_LIST_OFF = 4;

		public const int INDICATOR_INLINE_SEARCH = 8;
		public const int INDICATOR_GLOBAL_SEARCH = 9;

		public void SetUpStyles(Scintilla sci, AppSettings s, AlephTheme theme)
		{
			sci.Styles[Style.Default].BackColor = theme.Scintilla_Background.ToDCol();

			sci.Styles[STYLE_DEFAULT].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_DEFAULT].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_DEFAULT].Bold      = theme.Scintilla_Default.Bold;
			sci.Styles[STYLE_DEFAULT].Italic    = theme.Scintilla_Default.Italic;
			sci.Styles[STYLE_DEFAULT].Underline = theme.Scintilla_Default.Underline;
			sci.Styles[STYLE_DEFAULT].ForeColor = theme.Scintilla_Default.Foreground.ToDCol();
			sci.Styles[STYLE_DEFAULT].BackColor = theme.Scintilla_Default.Background.ToDCol();

			sci.Styles[STYLE_URL].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_URL].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_URL].Bold      = theme.Scintilla_Link.Bold;
			sci.Styles[STYLE_URL].Italic    = theme.Scintilla_Link.Italic;
			sci.Styles[STYLE_URL].Underline = theme.Scintilla_Link.Underline;
			sci.Styles[STYLE_URL].ForeColor = theme.Scintilla_Link.Foreground.ToDCol();
			sci.Styles[STYLE_URL].BackColor = theme.Scintilla_Link.Foreground.ToDCol();
			sci.Styles[STYLE_URL].Hotspot   = (s.LinkMode != LinkHighlightMode.OnlyHighlight);

			sci.Styles[STYLE_MD_DEFAULT].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_DEFAULT].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_DEFAULT].Bold      = theme.Scintilla_MarkdownDefault.Bold;
			sci.Styles[STYLE_MD_DEFAULT].Italic    = theme.Scintilla_MarkdownDefault.Italic;
			sci.Styles[STYLE_MD_DEFAULT].Underline = theme.Scintilla_MarkdownDefault.Underline;
			sci.Styles[STYLE_MD_DEFAULT].ForeColor = theme.Scintilla_MarkdownDefault.Foreground.ToDCol();
			sci.Styles[STYLE_MD_DEFAULT].BackColor = theme.Scintilla_MarkdownDefault.Foreground.ToDCol();

			sci.Styles[STYLE_MD_BOLD].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_BOLD].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_BOLD].Bold      = theme.Scintilla_MarkdownStrongEmph.Bold;
			sci.Styles[STYLE_MD_BOLD].Italic    = theme.Scintilla_MarkdownStrongEmph.Italic;
			sci.Styles[STYLE_MD_BOLD].Underline = theme.Scintilla_MarkdownStrongEmph.Underline;
			sci.Styles[STYLE_MD_BOLD].ForeColor = theme.Scintilla_MarkdownStrongEmph.Foreground.ToDCol();
			sci.Styles[STYLE_MD_BOLD].BackColor = theme.Scintilla_MarkdownStrongEmph.Foreground.ToDCol();

			sci.Styles[STYLE_MD_ITALIC].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_ITALIC].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_ITALIC].Bold      = theme.Scintilla_MarkdownEmph.Bold;
			sci.Styles[STYLE_MD_ITALIC].Italic    = theme.Scintilla_MarkdownEmph.Italic;
			sci.Styles[STYLE_MD_ITALIC].Underline = theme.Scintilla_MarkdownEmph.Underline;
			sci.Styles[STYLE_MD_ITALIC].ForeColor = theme.Scintilla_MarkdownEmph.Foreground.ToDCol();
			sci.Styles[STYLE_MD_ITALIC].BackColor = theme.Scintilla_MarkdownEmph.Foreground.ToDCol();

			sci.Styles[STYLE_MD_HEADER].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_HEADER].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_HEADER].Bold      = theme.Scintilla_MarkdownHeader.Bold;
			sci.Styles[STYLE_MD_HEADER].Italic    = theme.Scintilla_MarkdownHeader.Italic;
			sci.Styles[STYLE_MD_HEADER].Underline = theme.Scintilla_MarkdownHeader.Underline;
			sci.Styles[STYLE_MD_HEADER].ForeColor = theme.Scintilla_MarkdownHeader.Foreground.ToDCol();
			sci.Styles[STYLE_MD_HEADER].BackColor = theme.Scintilla_MarkdownHeader.Foreground.ToDCol();

			sci.Styles[STYLE_MD_CODE].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_CODE].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_CODE].Bold      = theme.Scintilla_MarkdownCode.Bold;
			sci.Styles[STYLE_MD_CODE].Italic    = theme.Scintilla_MarkdownCode.Italic;
			sci.Styles[STYLE_MD_CODE].Underline = theme.Scintilla_MarkdownCode.Underline;
			sci.Styles[STYLE_MD_CODE].ForeColor = theme.Scintilla_MarkdownCode.Foreground.ToDCol();
			sci.Styles[STYLE_MD_CODE].BackColor = theme.Scintilla_MarkdownCode.Foreground.ToDCol();

			sci.Styles[STYLE_MD_URL].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_URL].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_URL].Bold      = theme.Scintilla_MarkdownURL.Bold;
			sci.Styles[STYLE_MD_URL].Italic    = theme.Scintilla_MarkdownURL.Italic;
			sci.Styles[STYLE_MD_URL].Underline = theme.Scintilla_MarkdownURL.Underline;
			sci.Styles[STYLE_MD_URL].ForeColor = theme.Scintilla_MarkdownURL.Foreground.ToDCol();
			sci.Styles[STYLE_MD_URL].BackColor = theme.Scintilla_MarkdownURL.Foreground.ToDCol();

			sci.Styles[STYLE_MD_LIST].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_LIST].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_LIST].Bold      = theme.Scintilla_MarkdownURL.Bold;
			sci.Styles[STYLE_MD_LIST].Italic    = theme.Scintilla_MarkdownURL.Italic;
			sci.Styles[STYLE_MD_LIST].Underline = theme.Scintilla_MarkdownURL.Underline;
			sci.Styles[STYLE_MD_LIST].ForeColor = theme.Scintilla_MarkdownURL.Foreground.ToDCol();
			sci.Styles[STYLE_MD_LIST].BackColor = theme.Scintilla_MarkdownURL.Foreground.ToDCol();


			sci.Indicators[INDICATOR_INLINE_SEARCH].Style        = IndicatorStyle.StraightBox;
			sci.Indicators[INDICATOR_INLINE_SEARCH].Under        = theme.Scintilla_Search_Local.UnderText;
			sci.Indicators[INDICATOR_INLINE_SEARCH].ForeColor    = theme.Scintilla_Search_Local.Color.ToDCol();
			sci.Indicators[INDICATOR_INLINE_SEARCH].OutlineAlpha = theme.Scintilla_Search_Local.OutlineAlpha;
			sci.Indicators[INDICATOR_INLINE_SEARCH].Alpha        = theme.Scintilla_Search_Local.Alpha;

			sci.Indicators[INDICATOR_GLOBAL_SEARCH].Style        = IndicatorStyle.StraightBox;
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].Under        = theme.Scintilla_Search_Global.UnderText;
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].ForeColor    = theme.Scintilla_Search_Global.Color.ToDCol();
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].OutlineAlpha = theme.Scintilla_Search_Global.OutlineAlpha;
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].Alpha        = theme.Scintilla_Search_Global.Alpha;
		}

		public abstract void Highlight(Scintilla sci, int start, int end, AppSettings s);

		protected void LinkHighlight(Scintilla sci, int start, string text)
		{
			var m = REX_URL.Matches(text);

			var urlAreas = ExtractRanges(m);

			sci.StartStyling(start);

			int relPos = 0;
			var relEnd = text.Length;

			foreach (var area in urlAreas)
			{
				if (area.Item1 > relPos)
				{
					sci.SetStyling(area.Item1 - relPos, STYLE_DEFAULT);
					relPos += (area.Item1 - relPos);
				}

				if (area.Item2 > relPos)
				{
					sci.SetStyling(area.Item2 - relPos, STYLE_URL);
					relPos += (area.Item2 - relPos);
				}
			}

			if (relEnd > relPos)
			{
				sci.SetStyling(relEnd - relPos, STYLE_DEFAULT);
			}
		}

		private List<Tuple<int, int>> ExtractRanges(MatchCollection matches)
		{
			List<Tuple<int, int>> r = new List<Tuple<int, int>>();

			foreach (Match m in matches)
			{
				var start = m.Index;
				var end = m.Index + m.Length;

				bool found = false;
				for (int i = 0; i < r.Count; i++)
				{
					if ((r[i].Item1 <= start && start <= r[i].Item2) || (r[i].Item1 <= end && end <= r[i].Item2))
					{
						var rnew = Tuple.Create(Math.Min(r[i].Item1, start), Math.Max(r[i].Item2, end));
						r.Remove(r[i]);
						r.Add(rnew);
						found = true;
						break;
					}
				}
				if (!found)
				{
					r.Add(Tuple.Create(start, end));
				}
			}

			return r;
		}

		public List<Tuple<string, int, int>> FindAllLinks(Scintilla noteEdit)
		{
			var matched = REX_URL.Matches(noteEdit.Text);

			return matched.OfType<Match>().Select(m => Tuple.Create(m.Groups[0].Value, m.Index, m.Index + m.Length)).ToList();
		}

		public void UpdateListMargin(Scintilla sci, int? start, int? end)
		{
			int startLine = (start == null) ? 0                 : sci.LineFromPosition(start.Value);
			int endLine   = (end   == null) ? sci.Lines.Count-1 : sci.LineFromPosition(end.Value);

			startLine = Math.Max(0, startLine);
			endLine   = Math.Min(sci.Lines.Count-1, endLine);

			for (int idxline = startLine; idxline <= endLine; idxline++)
			{
				var line = sci.Lines[idxline];

				line.MarkerDelete(STYLE_MARKER_LIST_ON);
				line.MarkerDelete(STYLE_MARKER_LIST_OFF);

				var hl = GetListHighlight(line.Text);

				if (hl == true)  line.MarkerAdd(STYLE_MARKER_LIST_ON);
				if (hl == false) line.MarkerAdd(STYLE_MARKER_LIST_OFF);
			}
		}

		private bool? GetListHighlight(string text)
		{
			return GetListHighlight(text, out _);
		}

		private bool? GetListHighlight(string text, out char? mark)
		{
			text = text.TrimStart(' ', '\t');
			if (text.Length > 0 && (text[0] == '*' || text[0] == '-')) text = text.Substring(1);
			text = text.TrimStart(' ', '\t');

			if (text.Length < 4 || string.IsNullOrWhiteSpace(text.Substring(3))) { mark = null; return null; }

			if (text.StartsWith("[ ]")) { mark=' '; return false; }
			if (text.StartsWith("{ }")) { mark=' '; return false; }
			if (text.StartsWith("( )")) { mark=' '; return false; }

			if (text.StartsWith("[x]")) { mark='x'; return true; }
			if (text.StartsWith("{x}")) { mark='x'; return true; }
			if (text.StartsWith("(x)")) { mark='x'; return true; }

			if (text.StartsWith("[X]")) { mark='X'; return true; }
			if (text.StartsWith("{X}")) { mark='X'; return true; }
			if (text.StartsWith("(X)")) { mark='X'; return true; }

			if (text.StartsWith("[+]")) { mark='+'; return true; }
			if (text.StartsWith("{+}")) { mark='+'; return true; }
			if (text.StartsWith("(+)")) { mark='+'; return true; }

			if (text.StartsWith("[#]")) { mark='#'; return true; }
			if (text.StartsWith("{#}")) { mark='#'; return true; }
			if (text.StartsWith("(#)")) { mark='#'; return true; }

			{ mark = null; return null; }
		}

		public string ChangeListLine(string text, char chr)
		{
			if (GetListHighlight(text) == null) return text;

			var enumeration = false;
			for (int i = 0; i < text.Length - 3; i++)
			{
				if (text[i] == ' ' || text[i] == '\t') continue;
				if (!enumeration && (text[i] == '*' || text[i] == '-')) { enumeration = true; continue; }

				var found1 = (text[i + 0] == '[' && text[i + 2] == ']');
				var found2 = (text[i + 0] == '{' && text[i + 2] == '}');
				var found3 = (text[i + 0] == '(' && text[i + 2] == ')');

				if (found1 || found2 || found3)
				{
					var result = new StringBuilder(text);
					result[i + 1] = chr;
					return result.ToString();
				}
			}

			return text;
		}

		public char? FindListerOnMarker(LineCollection lines)
		{
			foreach (var line in lines)
			{
				if (GetListHighlight(line.Text, out char? c) == true) return c;
			}

			return null;
		}
	}
}
