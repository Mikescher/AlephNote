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
using AlephNote.Common.Util;

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

		public void SetUpStyles(Scintilla sci, AppSettings s)
		{
			var theme = ThemeManager.Inst.CurrentTheme;

			sci.Styles[Style.Default].BackColor = theme.Get<ColorRef>("scintilla.background").ToDCol();

			sci.Styles[STYLE_DEFAULT].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_DEFAULT].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_DEFAULT].Bold      = theme.Get<bool>(    "scintilla.default:bold");
			sci.Styles[STYLE_DEFAULT].Italic    = theme.Get<bool>(    "scintilla.default:italic");
			sci.Styles[STYLE_DEFAULT].Underline = theme.Get<bool>(    "scintilla.default:underline");
			sci.Styles[STYLE_DEFAULT].ForeColor = theme.Get<ColorRef>("scintilla.default:foreground").ToDCol();
			sci.Styles[STYLE_DEFAULT].BackColor = theme.Get<ColorRef>("scintilla.default:background").ToDCol();

			sci.Styles[STYLE_URL].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_URL].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_URL].Bold      = theme.Get<bool>(    "scintilla.link:bold");
			sci.Styles[STYLE_URL].Italic    = theme.Get<bool>(    "scintilla.link:italic");
			sci.Styles[STYLE_URL].Underline = theme.Get<bool>(    "scintilla.link:underline");
			sci.Styles[STYLE_URL].ForeColor = theme.Get<ColorRef>("scintilla.link:foreground").ToDCol();
			sci.Styles[STYLE_URL].BackColor = theme.Get<ColorRef>("scintilla.link:background").ToDCol();
			sci.Styles[STYLE_URL].Hotspot   = (s.LinkMode != LinkHighlightMode.OnlyHighlight);

			sci.Styles[STYLE_MD_DEFAULT].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_DEFAULT].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_DEFAULT].Bold      = theme.Get<bool>(    "scintilla.markdown.default:bold");
			sci.Styles[STYLE_MD_DEFAULT].Italic    = theme.Get<bool>(    "scintilla.markdown.default:italic");
			sci.Styles[STYLE_MD_DEFAULT].Underline = theme.Get<bool>(    "scintilla.markdown.default:underline");
			sci.Styles[STYLE_MD_DEFAULT].ForeColor = theme.Get<ColorRef>("scintilla.markdown.default:foreground").ToDCol();
			sci.Styles[STYLE_MD_DEFAULT].BackColor = theme.Get<ColorRef>("scintilla.markdown.default:background").ToDCol();

			sci.Styles[STYLE_MD_BOLD].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_BOLD].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_BOLD].Bold      = theme.Get<bool>(    "scintilla.markdown.strong_emph:bold");
			sci.Styles[STYLE_MD_BOLD].Italic    = theme.Get<bool>(    "scintilla.markdown.strong_emph:italic");
			sci.Styles[STYLE_MD_BOLD].Underline = theme.Get<bool>(    "scintilla.markdown.strong_emph:underline");
			sci.Styles[STYLE_MD_BOLD].ForeColor = theme.Get<ColorRef>("scintilla.markdown.strong_emph:foreground").ToDCol();
			sci.Styles[STYLE_MD_BOLD].BackColor = theme.Get<ColorRef>("scintilla.markdown.strong_emph:background").ToDCol();

			sci.Styles[STYLE_MD_ITALIC].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_ITALIC].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_ITALIC].Bold      = theme.Get<bool>(    "scintilla.markdown.emph:bold");
			sci.Styles[STYLE_MD_ITALIC].Italic    = theme.Get<bool>(    "scintilla.markdown.emph:italic");
			sci.Styles[STYLE_MD_ITALIC].Underline = theme.Get<bool>(    "scintilla.markdown.emph:underline");
			sci.Styles[STYLE_MD_ITALIC].ForeColor = theme.Get<ColorRef>("scintilla.markdown.emph:foreground").ToDCol();
			sci.Styles[STYLE_MD_ITALIC].BackColor = theme.Get<ColorRef>("scintilla.markdown.emph:background").ToDCol();

			sci.Styles[STYLE_MD_HEADER].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_HEADER].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_HEADER].Bold      = theme.Get<bool>(    "scintilla.markdown.header:bold");
			sci.Styles[STYLE_MD_HEADER].Italic    = theme.Get<bool>(    "scintilla.markdown.header:italic");
			sci.Styles[STYLE_MD_HEADER].Underline = theme.Get<bool>(    "scintilla.markdown.header:underline");
			sci.Styles[STYLE_MD_HEADER].ForeColor = theme.Get<ColorRef>("scintilla.markdown.header:foreground").ToDCol();
			sci.Styles[STYLE_MD_HEADER].BackColor = theme.Get<ColorRef>("scintilla.markdown.header:background").ToDCol();

			sci.Styles[STYLE_MD_CODE].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_CODE].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_CODE].Bold      = theme.Get<bool>(    "scintilla.markdown.code:bold");
			sci.Styles[STYLE_MD_CODE].Italic    = theme.Get<bool>(    "scintilla.markdown.code:italic");
			sci.Styles[STYLE_MD_CODE].Underline = theme.Get<bool>(    "scintilla.markdown.code:underline");
			sci.Styles[STYLE_MD_CODE].ForeColor = theme.Get<ColorRef>("scintilla.markdown.code:foreground").ToDCol();
			sci.Styles[STYLE_MD_CODE].BackColor = theme.Get<ColorRef>("scintilla.markdown.code:background").ToDCol();

			sci.Styles[STYLE_MD_URL].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_URL].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_URL].Bold      = theme.Get<bool>(    "scintilla.markdown.url:bold");
			sci.Styles[STYLE_MD_URL].Italic    = theme.Get<bool>(    "scintilla.markdown.url:italic");
			sci.Styles[STYLE_MD_URL].Underline = theme.Get<bool>(    "scintilla.markdown.url:underline");
			sci.Styles[STYLE_MD_URL].ForeColor = theme.Get<ColorRef>("scintilla.markdown.url:foreground").ToDCol();
			sci.Styles[STYLE_MD_URL].BackColor = theme.Get<ColorRef>("scintilla.markdown.url:background").ToDCol();

			sci.Styles[STYLE_MD_LIST].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_LIST].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_MD_LIST].Bold      = theme.Get<bool>(    "scintilla.markdown.list:bold");
			sci.Styles[STYLE_MD_LIST].Italic    = theme.Get<bool>(    "scintilla.markdown.list:italic");
			sci.Styles[STYLE_MD_LIST].Underline = theme.Get<bool>(    "scintilla.markdown.list:underline");
			sci.Styles[STYLE_MD_LIST].ForeColor = theme.Get<ColorRef>("scintilla.markdown.list:foreground").ToDCol();
			sci.Styles[STYLE_MD_LIST].BackColor = theme.Get<ColorRef>("scintilla.markdown.list:background").ToDCol();


			sci.Indicators[INDICATOR_INLINE_SEARCH].Style        = IndicatorStyle.StraightBox;
			sci.Indicators[INDICATOR_INLINE_SEARCH].Under        = theme.Get<bool>(    "scintilla.search.local:under_text");
			sci.Indicators[INDICATOR_INLINE_SEARCH].ForeColor    = theme.Get<ColorRef>("scintilla.search.local:color").ToDCol();
			sci.Indicators[INDICATOR_INLINE_SEARCH].OutlineAlpha = theme.Get<int>(     "scintilla.search.local:outline_alpha");
			sci.Indicators[INDICATOR_INLINE_SEARCH].Alpha        = theme.Get<int>(     "scintilla.search.local:alpha");

			sci.Indicators[INDICATOR_GLOBAL_SEARCH].Style        = IndicatorStyle.StraightBox;
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].Under        = theme.Get<bool>(    "scintilla.search.global:under_text");
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].ForeColor    = theme.Get<ColorRef>("scintilla.search.global:color").ToDCol();
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].OutlineAlpha = theme.Get<int>(     "scintilla.search.global:outline_alpha");
			sci.Indicators[INDICATOR_GLOBAL_SEARCH].Alpha        = theme.Get<int>(     "scintilla.search.global:alpha");

			sci.Styles[Style.LineNumber].Bold               = theme.Get<bool>("scintilla.margin.numbers:bold");
			sci.Styles[Style.LineNumber].Italic             = theme.Get<bool>("scintilla.margin.numbers:italic");
			sci.Styles[Style.LineNumber].Underline          = theme.Get<bool>("scintilla.margin.numbers:underline");
			sci.Styles[Style.LineNumber].ForeColor          = theme.Get<ColorRef>("scintilla.margin.numbers:foreground").ToDCol();
			sci.Styles[Style.LineNumber].BackColor          = theme.Get<ColorRef>("scintilla.margin.numbers:background").ToDCol();
			sci.Margins[STYLE_MARGIN_LINENUMBERS].BackColor = theme.Get<ColorRef>("scintilla.margin.numbers:background").ToDCol();

			sci.Margins[STYLE_MARGIN_LISTSYMBOLS].BackColor = theme.Get<ColorRef>("scintilla.margin.symbols:background").ToDCol();
			sci.SetFoldMarginColor(!theme.Get<ColorRef>("scintilla.margin.symbols:background").IsTransparent, theme.Get<ColorRef>("scintilla.margin.symbols:background").ToDCol());
			sci.SetFoldMarginHighlightColor(!theme.Get<ColorRef>("scintilla.margin.symbols:background").IsTransparent, theme.Get<ColorRef>("scintilla.margin.symbols:background").ToDCol());
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
