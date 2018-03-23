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
		void IAlephLogger.Trace(string src, string text, string longtext)
		{
			Trace(src, text, longtext);
		}

		[Conditional("DEBUG")]
		public void Trace(string src, string text, string longtext = null)
		{
			Log(new LogEvent(LogEventType.Trace, src, text, longtext));
		}

		[Conditional("DEBUG")]
		public void TraceExt(string src, string text, params Tuple<string, string>[] longtexts)
		{
			var pad = longtexts.Length == 0 ? 0 : longtexts.Max(l => l.Item1.Length);
			Log(new LogEvent(LogEventType.Trace, src, text, string.Join("\n", longtexts.Select(txt => txt.Item1.PadRight(pad, ' ') + " = " + txt.Item2))));
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

		public void ShowExceptionDialog(string title, Exception e, string additionalInfo)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => ExceptionDialog.Show(null, title, e, additionalInfo));
				return;
			}
			ExceptionDialog.Show(null, title, e, additionalInfo);
		}

		public void ShowExceptionDialog(string title, Exception e)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => ExceptionDialog.Show(null, title, e, ""));
				return;
			}
			ExceptionDialog.Show(null, title, e, "");
		}

		public void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => ExceptionDialog.Show(null, title, message, e, additionalExceptions));
				return;
			}
			ExceptionDialog.Show(null, title, message, e, additionalExceptions);
		}

		public void ShowSyncErrorDialog(string message, string trace)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => SyncErrorDialog.Show(null, message, trace));
				return;
			}
			SyncErrorDialog.Show(null, message, trace);
		}

		public void ShowSyncErrorDialog(string message, Exception e)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => SyncErrorDialog.Show(null, new[]{message}, new[]{e}));
				return;
			}
			SyncErrorDialog.Show(null, new[]{message}, new[]{e});
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
