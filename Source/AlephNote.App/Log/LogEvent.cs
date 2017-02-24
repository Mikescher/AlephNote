using System;

namespace AlephNote.Log
{
	public sealed class LogEvent
	{
		public LogEventType Type { get; private set; }
		public DateTime DateTime { get; private set; }
		public string Source { get; private set; }
		public string Text { get; private set; }
		public string LongText { get; private set; }

		public LogEvent(LogEventType t, string src, string txt, string longtxt)
		{
			Type = t;
			DateTime = DateTime.Now;
			Source = src;
			Text = txt;
			LongText = longtxt;
		}
	}
}
