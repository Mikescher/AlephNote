using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace AlephNote.Log
{
	public class VoidLogger : BasicWPFLogger
	{
		public override void Trace(string src, string text, string longtext = null) { /* void */ }

		public override void TraceExt(string src, string text, params (string, string)[] longtexts) { /* void */ }

		public override void Debug(string src, string text, string longtext = null) { /* void */ }

		public override void Info(string src, string text, string longtext = null) { /* void */ }

		public override void Warn(string src, string text, string longtext = null) { /* void */ }

		public override void Error(string src, string text, string longtext = null) { /* void */ }

		public override void Error(string src, string text, Exception e) { /* void */ }
		
		public override string Export()
		{
			return "{{void}}";
		}

		public override void Import(XDocument xdoc)
		{
			// nothing to export
		}
		
		public override void Clear()
		{
			// nothing to do
		}
		
		public override ObservableCollection<LogEvent> GetEventSource()
		{
			return null;
		}
	}
}
