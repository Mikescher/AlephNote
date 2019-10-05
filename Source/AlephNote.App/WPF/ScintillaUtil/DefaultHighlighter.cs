using System.Linq;
using System.Text.RegularExpressions;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.ScintillaUtil
{
	public class DefaultHighlighter : ScintillaHighlighter
	{
		private const int MAX_BACKTRACE = 512;
		private const int MAX_FORWARDTRACE = 512;
		
		public override void Highlight(ScintillaNET.Scintilla sci, int start, int end, AppSettings s)
		{
			bool startsWithNL = sci.GetCharAt(start) == '\n' || (start+1 < sci.TextLength && sci.GetCharAt(start) == '\r' && sci.GetCharAt(start+1) == '\n');
			bool endsWithNL   = sci.GetCharAt(end) == '\n' || (end-1 >= 0 && sci.GetCharAt(end-1) == '\n');

			// move back to start of line
			if (!startsWithNL)
			{
				for (int i = 0; i < MAX_BACKTRACE && start > 0; i++, start--)
				{
					if (start > 0 && sci.GetCharAt(start - 1) == '\n') break;
				}
			}
			
			// move forward to end of line
			if (!endsWithNL)
			{
				for (int i = 0; i < MAX_FORWARDTRACE && end < sci.TextLength; i++, end++)
				{
					if (end >= sci.TextLength || sci.GetCharAt(end) == '\n') break;
				}
			}

			var text = sci.GetTextRange(start, end - start);

			if (s.LinkMode != LinkHighlightMode.Disabled)
			{
				LinkHighlight(sci, start, text);
			}
			else
			{
				sci.StartStyling(start);
				sci.SetStyling(end-start, STYLE_DEFAULT);
			}
		}

		public override string GetClickedLink(string text, int pos)
		{
			var linedata = GetLine(text, pos); // (line, lineStart, lineLen)
			var line      = linedata.Item1;
			var lineStart = linedata.Item2;

			return GetURLMatchingRegex()
				.Matches(line)
				.OfType<Match>()
				.Where(m => lineStart + m.Index <= pos && pos <= lineStart + m.Index+m.Length)
				.Select(m => m.Groups[0].Value)
				.FirstOrDefault();
		}
	}
}
