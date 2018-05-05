using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AlephNote.PluginInterface.Exceptions;
using Microsoft.Win32;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for SyncErrorDialog.xaml
	/// </summary>
	public partial class SyncErrorDialog
	{
		private SyncErrorDialog()
		{
			InitializeComponent();
		}

		public static void Show(Window owner, Exception e)
		{
			var dlg = new SyncErrorDialog();

			dlg.ErrorMessage.Text = e.Message;
			dlg.ErrorTrace.Text = FormatExecption(e);

			dlg.Owner = owner;

			dlg.ShowDialog();
		}

		public static void Show(Window owner, string message, string trace)
		{
			var dlg = new SyncErrorDialog();

			dlg.ErrorMessage.Text = message;
			dlg.ErrorTrace.Text = trace;

			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();
		}

		public static void Show(Window owner, IEnumerable<string> imessages, IEnumerable<Exception> iexceptions)
		{
			string split = Environment.NewLine + "--------------------" + Environment.NewLine;

			var messages = imessages.ToList();
			var exceptions = iexceptions.ToList();

			bool isconnerror = exceptions.All(e => (e as RestException)?.IsConnectionProblem == true);

			var dlg = new SyncErrorDialog();
			
			dlg.CbSupress.Visibility = isconnerror ? Visibility.Visible : Visibility.Collapsed;
			dlg.ErrorMessage.Text = string.Join(Environment.NewLine, messages);
			dlg.ErrorTrace.Text = string.Join(split, exceptions.Select(FormatExecption));

			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();

			if (isconnerror && dlg.CbSupress.IsChecked==true)
			{
				MainWindow.Instance.Settings.SuppressConnectionProblemPopup = true;
				MainWindow.Instance.Settings.Save();
			}
		}

		private static string FormatExecption(Exception e)
		{
			const string DELIMITER = " ---> ";

			var lines = Regex.Split(e.ToString(), @"\r?\n");

			if (lines.Any()) lines[0] = lines[0].Replace(DELIMITER, Environment.NewLine + "    ---> ");

			return string.Join(Environment.NewLine, lines);

		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void ButtonExportLog_OnClick(object sender, RoutedEventArgs e)
		{
			var sfd = new SaveFileDialog { Filter = "Log files (*.xml)|*.xml", FileName = "Log.xml" };

			if (sfd.ShowDialog(this) == true)
			{
				File.WriteAllText(sfd.FileName, App.Logger.Export());
			}
		}
	}
}
