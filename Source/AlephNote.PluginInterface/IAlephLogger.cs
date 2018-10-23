using System;

namespace AlephNote.PluginInterface
{
	public interface IAlephLogger
	{
		void Trace(string src, string text, string longtext = null);
		void Debug(string src, string text, string longtext = null);
		void Info (string src, string text, string longtext = null);
		void Warn (string src, string text, string longtext = null);
		void Error(string src, string text, string longtext = null);
		void Error(string src, string text, Exception e);

		void ShowExceptionDialog(string title, Exception e);
		void ShowExceptionDialog(string title, Exception e, string suffix);
		void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions);

		void ShowSyncErrorDialog(string message, string trace);
		void ShowSyncErrorDialog(string message, Exception e);
	}

	public class AlephDummyLoggger : IAlephLogger
	{
		public void Trace(string src, string text, string longtext = null) { }
		public void Debug(string src, string text, string longtext = null) { }
		public void Info(string src, string text, string longtext = null) { }
		public void Warn(string src, string text, string longtext = null) { }
		public void Error(string src, string text, string longtext = null) { }
		public void Error(string src, string text, Exception e) { }

		public void ShowExceptionDialog(string title, Exception e) { }
		public void ShowExceptionDialog(string title, Exception e, string suffix) { }
		public void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions) { }

		public void ShowSyncErrorDialog(string message, string trace) { }
		public void ShowSyncErrorDialog(string message, Exception e) { }
	}
}
