using System;
using System.Diagnostics;

namespace AlephNote.PluginInterface
{
	public interface IAlephLogger
	{
		Version AppVersion { get; }

		void Trace(string src, string text, string longtext = null);
		void Debug(string src, string text, string longtext = null);
		void Info (string src, string text, string longtext = null);
		void Warn (string src, string text, string longtext = null);
		void Error(string src, string text, string longtext = null);
		void Error(string src, string text, Exception e);

		void ShowExceptionDialog(string title, Exception e);
		void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions);
	}

	public class AlephDummyLoggger : IAlephLogger
	{
		public Version AppVersion { get; } = new Version(0, 0, 0, 0);

		public void Trace(string src, string text, string longtext = null) { }
		public void Debug(string src, string text, string longtext = null) { }
		public void Info(string src, string text, string longtext = null) { }
		public void Warn(string src, string text, string longtext = null) { }
		public void Error(string src, string text, string longtext = null) { }
		public void Error(string src, string text, Exception e) { }

		public void ShowExceptionDialog(string title, Exception e) { }
		public void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions) { }
	}
}
