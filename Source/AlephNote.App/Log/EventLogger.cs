using AlephNote.PluginInterface;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Windows;

namespace AlephNote.Log
{
	public class EventLogger : IAlephLogger
	{
		public readonly ObservableCollection<LogEvent> Events = new ObservableCollection<LogEvent>();

		public bool DebugEnabled = false;

		public Version AppVersion => App.APP_VERSION;

		private void Log(LogEvent e)
		{
			var curr = Application.Current;

			if (curr == null)
			{
#if DEBUG
				Debugger.Break();
#endif

				Console.Error.WriteLine("Could not Log event ... Application.Current == null");
				Console.Out.WriteLine(e.Source);
				Console.Out.WriteLine(e.Type);
				Console.Out.WriteLine(e.Text);
				Console.Out.WriteLine(e.LongText);
				return;
			}

			var disp = curr.Dispatcher;

			if (disp.CheckAccess())
				Events.Add(e);
			else
				disp.BeginInvoke(new Action(() => Events.Add(e)));
		}

		[Conditional("DEBUG")]
		public void Trace(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Trace, src, text, longtext));
		}

		public void Debug(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Debug, src, text, longtext));
		}

		public void Info(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Information, src, text, longtext));
		}

		public void Warn(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Warning, src, text, longtext));
		}

		public void Error(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Error, src, text, longtext));
		}

		public void Error(string src, string text, Exception e)
		{
			Log(new LogEvent(LogEventType.Error, src, text, e.ToString()));
		}

		public void ShowExceptionDialog(string title, Exception e)
		{
			ExceptionDialog.Show(null, title, e);
		}

		public void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions)
		{
			ExceptionDialog.Show(null, title, message, e, additionalExceptions);
		}

		public string Export()
		{
			var root = new XElement("log");
			XDocument doc = new XDocument(root);

			root.Add(new XAttribute("version", App.APP_VERSION.ToString()));

			foreach (var e in Events) root.Add(e.Serialize());

			return XHelper.ConvertToStringFormatted(doc);
		}

		public void Import(XDocument xdoc)
		{
			Events.Clear();
			foreach (var elem in xdoc.Root?.Elements("event") ?? Enumerable.Empty<XElement>())
			{
				Events.Add(LogEvent.Deserialize(elem));
			}
		}
	}
}
