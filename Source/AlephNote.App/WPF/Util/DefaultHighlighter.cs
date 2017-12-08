using AlephNote.Common.Settings;
using ScintillaNET;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Util
{
	public class DefaultHighlighter : ScintillaHighlighter
	{
		private const int MAX_BACKTRACE = 128;
		
		public override void Highlight(Scintilla sci, int start, int end, AppSettings s)
		{
			// move back to start of line
			for (int i = 0; i < MAX_BACKTRACE && start > 0; i++, start--)
			{
				if (start > 0 && sci.GetCharAt(start - 1) == '\n') break;
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
	}
}
