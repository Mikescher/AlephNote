using AlephNote.PluginInterface;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ScintillaNET;
using AlephNote.Common.Util.Search;

namespace AlephNote.WPF.Util
{
	public static class ScintillaSearcher
	{
		public static void Highlight(Scintilla sci, INote n, string searchText)
		{
			sci.IndicatorCurrent = ScintillaHighlighter.INDICATOR_GLOBAL_SEARCH;

			sci.IndicatorClearRange(0, sci.TextLength);

			if (string.IsNullOrWhiteSpace(searchText))
			{
				return;
			}

			if (SearchStringParser.IsRegex(searchText, out var searchRegex))
			{
				var m = searchRegex.Matches(sci.Text);

				foreach (Match match in m)
				{
					sci.IndicatorFillRange(match.Index, match.Length);
				}

				return;
			}
			else
			{
				string txt = sci.Text.ToLower();
				int lastIdx = 0;
				while (lastIdx>=0)
				{
					lastIdx = txt.IndexOf(searchText, lastIdx, StringComparison.Ordinal);
					if (lastIdx == -1) continue;

					sci.IndicatorFillRange(lastIdx, searchText.Length);
					lastIdx += searchText.Length;
				}

				return;
			}
		}
	}
}
