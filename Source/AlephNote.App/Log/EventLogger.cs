using AlephNote.PluginInterface;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Dialogs;
using AlephNote.WPF.Windows;
using System.Collections.Generic;

namespace AlephNote.Log
{
	public class EventLogger : BasicWPFLogger
	{
		private readonly ObservableCollection<LogEvent> events = new ObservableCollection<LogEvent>();

		private void Log(LogEvent e)
		{
			var curr = Application.Current;

			if (curr == null)
			{
				Console.Error.WriteLine("Could not Log event ... Application.Current == null");
				Console.Out.WriteLine(e.Source);
				Console.Out.WriteLine(e.Type);
				Console.Out.WriteLine(e.Text);
				Console.Out.WriteLine(e.LongText);
				return;
			}

			#if DEBUG
			//Console.Out.WriteLine($"[{e.Type}]===== {e.Source} =====");
			//Console.Out.WriteLine(e.Text);
			//if (!string.IsNullOrWhiteSpace(e.LongText)) Console.Out.WriteLine();
			//if (!string.IsNullOrWhiteSpace(e.LongText)) Console.Out.WriteLine(e.LongText);
			//Console.Out.WriteLine("===============");
			//Console.Out.WriteLine();
			#endif

			var disp = curr.Dispatcher;

			if (disp.CheckAccess())
				events.Add(e);
			else
				disp.BeginInvoke(new Action(() => events.Add(e)));
		}

		public override void Trace(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Trace, src, text, longtext));
		}
		
		public override void TraceExt(string src, string text, params (string, string)[] longtexts)
		{
			var pad = longtexts.Length == 0 ? 0 : longtexts.Max(l => l.Item1.Length);
			Log(new LogEvent(LogEventType.Trace, src, text, string.Join("\n", longtexts.Select(txt => txt.Item1.PadRight(pad, ' ') + " = " + txt.Item2))));
		}

		public override void Debug(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Debug, src, text, longtext));
		}

		public override void Info(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Information, src, text, longtext));
		}

		public override void Warn(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Warning, src, text, longtext));
		}

		public override void Error(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Error, src, text, longtext));
		}

		public override void Error(string src, string text, Exception e)
		{
			Log(new LogEvent(LogEventType.Error, src, text, e.ToString()));
		}

		public override string Export()
		{
			var root = new XElement("log");
			XDocument doc = new XDocument(root);

			root.Add(new XAttribute("version", App.APP_VERSION.ToString()));

			foreach (var e in events) root.Add(e.Serialize());

			return XHelper.ConvertToStringFormatted(doc);
		}

		public override List<LogEvent> ReadExport(XDocument xdoc)
		{
			var result = new List<LogEvent>();
			foreach (var elem in xdoc.Root?.Elements("event") ?? Enumerable.Empty<XElement>())
			{
				result.Add(LogEvent.Deserialize(elem));
			}
			return result;
		}

		public override void Clear()
		{
			events.Clear();
		}
		
		public override ObservableCollection<LogEvent> GetEventSource()
		{
			return events;
		}
	}
}
