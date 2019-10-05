using System;
using System.Diagnostics;

namespace AlephNote.PluginInterface
{
	public abstract class AlephLogger
	{
		[Conditional("DEBUG")] public abstract void Trace(string src, string text, string longtext = null);
		[Conditional("DEBUG")] public abstract void TraceExt(string src, string text, params Tuple<string, string>[] longtexts);
		public abstract void Debug(string src, string text, string longtext = null);
		public abstract void Info (string src, string text, string longtext = null);
		public abstract void Warn (string src, string text, string longtext = null);
		public abstract void Error(string src, string text, string longtext = null);
		public abstract void Error(string src, string text, Exception e);

		public abstract void ShowExceptionDialog(string title, Exception e);
		public abstract void ShowExceptionDialog(string title, Exception e, string suffix);
		public abstract void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions);

		public abstract void ShowSyncErrorDialog(string message, string trace);
		public abstract void ShowSyncErrorDialog(string message, Exception e);
	}

	public class AlephDummyLoggger : AlephLogger
	{
		public override void Trace(string src, string text, string longtext = null) { }
		public override void TraceExt(string src, string text, params Tuple<string, string>[] longtexts) { }
		public override void Debug(string src, string text, string longtext = null) { }
		public override void Info(string src, string text, string longtext = null) { }
		public override void Warn(string src, string text, string longtext = null) { }
		public override void Error(string src, string text, string longtext = null) { }
		public override void Error(string src, string text, Exception e) { }

		public override void ShowExceptionDialog(string title, Exception e) { }
		public override void ShowExceptionDialog(string title, Exception e, string suffix) { }
		public override void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions) { }

		public override void ShowSyncErrorDialog(string message, string trace) { }
		public override void ShowSyncErrorDialog(string message, Exception e) { }
	}
}
