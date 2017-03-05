using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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

		public static void Show(Window owner, string title, Exception e)
		{
			var dlg = new ExceptionDialog();

			dlg.ErrorMessage.Text = e.Message;
			dlg.ErrorTrace.Text = FormatExecption(e);
			dlg.Title = title;

			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();
		}

		public static void Show(Window owner, string title, string message, Exception e, params Exception[] additionalExceptions)
		{
			string SPLIT = Environment.NewLine + "--------------------" + Environment.NewLine;

			var dlg = new ExceptionDialog();

			dlg.ErrorMessage.Text = message;
			dlg.ErrorTrace.Text = string.Join(SPLIT, new List<Exception> {e}.Concat(additionalExceptions).Select(FormatExecption));
			dlg.Title = title;

			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();
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

		private void ClickLink(object sender, MouseButtonEventArgs e)
		{
			Process.Start(@"https://github.com/Mikescher/CommonNote/issues");
		}
	}
}
