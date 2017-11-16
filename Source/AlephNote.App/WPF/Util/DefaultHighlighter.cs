using AlephNote.Settings;
using ScintillaNET;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Util
{
	public static class DefaultHighlighter
	{
		private static readonly Regex REX_URL = new Regex(@"(?:(?:(?:http|https|ftp|file|mailto|irc)://[\w\.]+\.\w+)|(?:www\.[\w\.]+\.\w+))(?=(\s|$))", RegexOptions.Compiled);

		private const int MAX_BACKTRACE = 128;

		public const int STYLE_DEFAULT = 0;
		public const int STYLE_URL     = 1;

		public static void SetUpStyles(Scintilla sci, AppSettings s)
		{
			sci.Styles[STYLE_DEFAULT].Bold   = s.NoteFontModifier == FontModifier.Bold || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_DEFAULT].Italic = s.NoteFontModifier == FontModifier.Italic || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_DEFAULT].Size   = (int)s.NoteFontSize;
			sci.Styles[STYLE_DEFAULT].Font   = s.NoteFontFamily;

			sci.Styles[STYLE_URL].Bold      = s.NoteFontModifier == FontModifier.Bold || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_URL].Italic    = s.NoteFontModifier == FontModifier.Italic || s.NoteFontModifier == FontModifier.BoldItalic;
			sci.Styles[STYLE_URL].Size      = (int)s.NoteFontSize;
			sci.Styles[STYLE_URL].Font      = s.NoteFontFamily;
			sci.Styles[STYLE_URL].ForeColor = System.Drawing.Color.Blue;
			sci.Styles[STYLE_URL].Hotspot   = (s.LinkMode != LinkHighlightMode.OnlyHighlight);
			sci.Styles[STYLE_URL].Underline = true;
		}

		public static void Highlight(Scintilla sci, int start, int end, bool highlightLinks)
		{
			// move back to start of line
			for (int i = 0; i < MAX_BACKTRACE && start > 0; i++, start--)
			{
				if (start > 0 && sci.GetCharAt(start - 1) == '\n') break;
			}

			var text = sci.GetTextRange(start, end - start);

			if (highlightLinks)
			{
				LinkHighlight(sci, start, text);
			}
			else
			{
				sci.StartStyling(start);
				sci.SetStyling(end-start, STYLE_DEFAULT);
			}
		}

		private static void LinkHighlight(Scintilla sci, int start, string text)
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

		private static List<Tuple<int, int>> ExtractRanges(MatchCollection matches)
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

		public static List<Tuple<string, int, int>> FindAllLinks(Scintilla noteEdit)
		{
			var matched = REX_URL.Matches(noteEdit.Text);

			return matched.OfType<Match>().Select(m => Tuple.Create(m.Groups[0].Value, m.Index, m.Index+m.Length)).ToList();
		}
	}
}
