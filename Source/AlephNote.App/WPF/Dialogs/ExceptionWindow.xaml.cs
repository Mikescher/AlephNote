using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for SyncErrorDialog.xaml
	/// </summary>
	public partial class ExceptionDialog
	{
		private ExceptionDialog()
		{
			InitializeComponent();
		}

		public static void Show(Window owner, string title, Exception e, string additionalInfo)
		{
			var dlg = new ExceptionDialog();

			dlg.Title = "Error in AlephNote v" + App.APP_VERSION;
			dlg.ErrorMessage.Text = title;
			dlg.ErrorTrace.Text = (FormatException(e, additionalInfo) ?? FormatStacktrace(additionalInfo));

			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();
		}

		public static void Show(Window owner, string title, string message, Exception e, params Exception[] additionalExceptions)
		{
			string split = Environment.NewLine + "--------------------" + Environment.NewLine;

			var dlg = new ExceptionDialog
			{
				ErrorMessage =
				{
					Text = message
				},
				ErrorTrace   =
				{
					Text = string.Join(split, new List<Exception> {e}.Concat(additionalExceptions).Select(ex => FormatException(ex, "")))
				},
				Title        = title,
			};
			
			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();
		}

		private static string FormatException(Exception e, string suffix)
		{
			const string DELIMITER = " ---> ";

			if (e == null) return null;

			var lines = Regex.Split(e.ToString(), @"\r?\n").ToList();

			if (lines.Any()) lines[0] = lines[0].Replace(DELIMITER, Environment.NewLine + "    ---> ");

			if (!string.IsNullOrWhiteSpace(suffix))
			{
				lines.Add("");
				lines.Add("-----------------------");
				lines.Add("");
				lines.Add(suffix);
			}

			return string.Join(Environment.NewLine, lines);
		}

		private static string FormatStacktrace(string suffix)
		{
			var trace = Environment.StackTrace;
			if (!string.IsNullOrWhiteSpace(suffix))
			{
				trace += "\r\n";
				trace += "\r\n-----------------------";
				trace += "\r\n";
				trace += "\r\n" + suffix;
			}
			return trace;
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void ClickLink(object sender, MouseButtonEventArgs e)
		{
			Process.Start(@"https://github.com/Mikescher/AlephNote/issues");
		}

		private void ButtonExportLogfile_OnClick(object sender, RoutedEventArgs e)
		{
			var sfd = new SaveFileDialog { Filter = "Log files (*.xml)|*.xml", FileName = "Log.xml" };

			if (sfd.ShowDialog(this) == true)
			{
				File.WriteAllText(sfd.FileName, App.Logger.Export());
			}
		}
	}
}
