using AlephNote.PluginInterface;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ScintillaNET;

namespace AlephNote.WPF.Util
{
	public static class ScintillaSearcher
	{
		public static bool IsInFilter(INote note, string searchText)
		{
			if (string.IsNullOrWhiteSpace(searchText)) return true;

			if (IsRegex(searchText, out var searchRegex))
			{
				if (searchRegex.IsMatch(note.Title)) return true;
				if (searchRegex.IsMatch(note.Text)) return true;
				if (note.Tags.Any(t => searchRegex.IsMatch(t))) return true;

				return false;
			}
			else if (searchText.Length > 2 && searchText.StartsWith("[") && searchText.EndsWith("]"))
			{
				var searchTag = searchText.Substring(1, searchText.Length - 2);

				if (note.HasTagCasInsensitive(searchTag)) return true;

				return false;
			}
			else
			{
				if (note.Title.ToLower().Contains(searchText.ToLower())) return true;
				if (note.Text.ToLower().Contains(searchText.ToLower())) return true;
				if (note.HasTagCasInsensitive(searchText)) return true;

				return false;
			}
		}

		public static void Highlight(Scintilla sci, INote n, string searchText)
		{
			sci.IndicatorCurrent = ScintillaHighlighter.INDICATOR_GLOBAL_SEARCH;

			sci.IndicatorClearRange(0, sci.TextLength);

			if (string.IsNullOrWhiteSpace(searchText))
			{
				return;
			}

			if (IsRegex(searchText, out var searchRegex))
			{
				var m = searchRegex.Matches(sci.Text);

				foreach (Match match in m)
				{
					sci.IndicatorFillRange(match.Index, match.Length);
				}

				return;
			}
			else if (searchText.Length > 2 && searchText.StartsWith("[") && searchText.EndsWith("]"))
			{
				return;
			}
			else
			{
				string txt = sci.Text.ToLower();
				int lastIdx = 0;
				while (lastIdx>=0)
				{
					lastIdx = txt.IndexOf(searchText, lastIdx);
					if (lastIdx == -1) continue;

					sci.IndicatorFillRange(lastIdx, searchText.Length);
					lastIdx += searchText.Length;
				}

				return;
			}
		}

		private static bool IsRegex(string text, out Regex r)
		{
			try
			{
				if (text.Length >= 3 && text.StartsWith("/") && text.EndsWith("/"))
				{
					r = new Regex(text.Substring(1, text.Length - 2));
					return true;
				}
				else
				{
					r = null;
					return false;
				}

			}
			catch (ArgumentException)
			{
				r = null;
				return false;
			}
		}
	}
}
