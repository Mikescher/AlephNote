using AlephNote.PluginInterface;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace AlephNote.Log
{
	public class EventLogger : IAlephLogger
	{
		public readonly ObservableCollection<LogEvent> Events = new ObservableCollection<LogEvent>();

#if DEBUG
		public bool DebugEnabled = true;
#else
		public bool DebugEnabled = false;
#endif

		private void Log(LogEvent e)
		{
			var curr = Application.Current;

			if (curr == null)
			{
				Debugger.Break();

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

		public void Debug(string src, string text, string longtext = null)
		{
			if (DebugEnabled) Log(new LogEvent(LogEventType.Debug, src, text, longtext));
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
	}
}
