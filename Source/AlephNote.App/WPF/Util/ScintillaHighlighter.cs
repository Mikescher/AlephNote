using AlephNote.Settings;
using ScintillaNET;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Util
{
	public abstract class ScintillaHighlighter
	{
		public static readonly Regex REX_URL = new Regex(@"(?:(?:(?:(?:(?:http|https|ftp|file|irc)://[\w\.\-_äöü]+\.\w\w+)|(?:www\.[\w\.\-_äöü]+\.\w\w+))[/\w\?=&]*)|(?:mailto:(?:[äöüa-z0-9]+(?:[._\-][äöüa-z0-9]+)*)@(?:[äöüa-z0-9]+(?:[.-][äöüa-z0-9]+)*\.[a-z]{2,})))(?=(\s|$))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

		public void SetUpStyles(Scintilla sci, AppSettings s)
		{
			sci.Styles[STYLE_DEFAULT].Bold = s.NoteFontModifier == FontModifier.Bold || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_DEFAULT].Italic = s.NoteFontModifier == FontModifier.Italic || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_DEFAULT].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_DEFAULT].Font = s.NoteFontFamily;

			sci.Styles[STYLE_URL].Bold = s.NoteFontModifier == FontModifier.Bold || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_URL].Italic = s.NoteFontModifier == FontModifier.Italic || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_URL].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_URL].Font = s.NoteFontFamily;
			sci.Styles[STYLE_URL].ForeColor = System.Drawing.Color.Blue;
			sci.Styles[STYLE_URL].Hotspot = (s.LinkMode != LinkHighlightMode.OnlyHighlight);
			sci.Styles[STYLE_URL].Underline = true;

			sci.Styles[STYLE_MD_DEFAULT].Bold = false;
			sci.Styles[STYLE_MD_DEFAULT].Italic = false;
			sci.Styles[STYLE_MD_DEFAULT].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_DEFAULT].Font = s.NoteFontFamily;

			sci.Styles[STYLE_MD_BOLD].Bold = true;
			sci.Styles[STYLE_MD_BOLD].Italic = false;
			sci.Styles[STYLE_MD_BOLD].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_BOLD].Font = s.NoteFontFamily;
			//sci.Styles[STYLE_MD_BOLD].BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

			sci.Styles[STYLE_MD_ITALIC].Bold = false;
			sci.Styles[STYLE_MD_ITALIC].Italic = true;
			sci.Styles[STYLE_MD_ITALIC].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_ITALIC].Font = s.NoteFontFamily;
			//sci.Styles[STYLE_MD_ITALIC].BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

			sci.Styles[STYLE_MD_HEADER].Bold = false;
			sci.Styles[STYLE_MD_HEADER].Italic = false;
			sci.Styles[STYLE_MD_HEADER].ForeColor = System.Drawing.Color.FromArgb(178, 34, 34);
			sci.Styles[STYLE_MD_HEADER].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_HEADER].Font = s.NoteFontFamily;
			//sci.Styles[STYLE_MD_HEADER].BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

			sci.Styles[STYLE_MD_CODE].Bold = false;
			sci.Styles[STYLE_MD_CODE].Italic = false;
			sci.Styles[STYLE_MD_CODE].ForeColor = System.Drawing.Color.FromArgb(0, 128, 0);
			sci.Styles[STYLE_MD_CODE].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_CODE].Font = s.NoteFontFamily;
			//sci.Styles[STYLE_MD_CODE].BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

			sci.Styles[STYLE_MD_URL].Bold = false;
			sci.Styles[STYLE_MD_URL].Italic = false;
			sci.Styles[STYLE_MD_URL].ForeColor = System.Drawing.Color.FromArgb(0, 0, 255);
			sci.Styles[STYLE_MD_URL].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_URL].Font = s.NoteFontFamily;
			//sci.Styles[STYLE_MD_URL].BackColor = System.Drawing.Color.FromArgb(230, 230, 230);

			sci.Styles[STYLE_MD_LIST].Bold = false;
			sci.Styles[STYLE_MD_LIST].Italic = false;
			sci.Styles[STYLE_MD_LIST].ForeColor = System.Drawing.Color.FromArgb(128, 128, 128);
			sci.Styles[STYLE_MD_LIST].Size = (int)s.NoteFontSize;
			sci.Styles[STYLE_MD_LIST].Font = s.NoteFontFamily;
			//sci.Styles[STYLE_MD_LIST].BackColor = System.Drawing.Color.FromArgb(230, 230, 230);
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
				relPos += (relEnd - relPos);
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
			text = text.ToLower();
			text = text.TrimStart(' ', '\t');
			if (text.Length > 0 && (text[0] == '*' || text[0] == '-')) text = text.Substring(1);
			text = text.TrimStart(' ', '\t');

			if (text.Length < 4 || string.IsNullOrWhiteSpace(text.Substring(3))) return null;

			if (text.StartsWith("[ ]")) return false;
			if (text.StartsWith("{ }")) return false;
			if (text.StartsWith("( )")) return false;

			if (text.StartsWith("[x]")) return true;
			if (text.StartsWith("{x}")) return true;
			if (text.StartsWith("(x)")) return true;

			if (text.StartsWith("[+]")) return true;
			if (text.StartsWith("{+}")) return true;
			if (text.StartsWith("(+)")) return true;

			return null;
		}
	}
}
