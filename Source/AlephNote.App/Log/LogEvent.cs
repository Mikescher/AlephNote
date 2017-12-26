using System;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

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

		private LogEvent(LogEventType t, DateTime dt, string src, string txt, string longtxt)
		{
			Type = t;
			DateTime = dt;
			Source = src;
			Text = txt;
			LongText = longtxt;
		}

		public XElement Serialize()
		{
			return new XElement("event",
				new XAttribute("Type", Type),
				new XAttribute("DateTime", DateTime),
				new XAttribute("Source", Source),
				new XAttribute("Text", Text),
				new XAttribute("LongText", Convert.ToBase64String(Encoding.UTF8.GetBytes(LongText ?? ""))));
		}

		public static LogEvent Deserialize(XElement elem)
		{
			return new LogEvent(
				XHelper.GetAttributeEnum<LogEventType>(elem, "Type"),
				XHelper.GetAttributeDateTime(elem, "DateTime"),
				XHelper.GetAttributeString(elem, "Source"),
				XHelper.GetAttributeString(elem, "Text"),
				Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetAttributeString(elem, "LongText"))));
		}
	}
}
