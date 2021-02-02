using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Linq;
using AlephNote.PluginInterface;
using AlephNote.WPF.Dialogs;
using AlephNote.WPF.Windows;

namespace AlephNote.Log
{
	public abstract class BasicWPFLogger : AlephLogger
	{
		public override void ShowExceptionDialog(string title, Exception e, string additionalInfo)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => ExceptionDialog.Show(null, title, e, additionalInfo));
				return;
			}
			ExceptionDialog.Show(null, title, e, additionalInfo);
		}

		public override void ShowExceptionDialog(string title, Exception e)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => ExceptionDialog.Show(null, title, e, ""));
				return;
			}
			ExceptionDialog.Show(null, title, e, "");
		}

		public override void ShowExceptionDialog(string title, string message, Exception e, params Exception[] additionalExceptions)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => ExceptionDialog.Show(null, title, message, e, additionalExceptions));
				return;
			}
			ExceptionDialog.Show(null, title, message, e, additionalExceptions);
		}

		public override void ShowSyncErrorDialog(string message, string trace)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => SyncErrorDialog.Show(null, message, trace));
				return;
			}
			SyncErrorDialog.Show(null, message, trace);
		}

		public override void ShowSyncErrorDialog(string message, Exception e)
		{
			if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(() => SyncErrorDialog.Show(null, new[]{message}, new[]{e}));
				return;
			}
			SyncErrorDialog.Show(null, new[]{message}, new[]{e});
		}
		
		public abstract string Export();
		public abstract List<LogEvent> ReadExport(XDocument xdoc);
		public abstract void Clear();
		public abstract ObservableCollection<LogEvent> GetEventSource();
	}
}
