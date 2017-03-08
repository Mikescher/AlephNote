using System;

namespace AlephNote.PluginInterface
{
	public interface IAlephLogger
	{
		void Debug(string src, string text, string longtext = null);
		void Info (string src, string text, string longtext = null);
		void Warn (string src, string text, string longtext = null);
		void Error(string src, string text, string longtext = null);
		void Error(string src, string text, Exception e);

		void ShowExceptionDialog(string title, Exception e);
		void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions);
	}
}
