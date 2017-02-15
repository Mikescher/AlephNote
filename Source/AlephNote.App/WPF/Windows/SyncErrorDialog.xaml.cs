using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

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

			dlg.Owner = owner;

			dlg.ShowDialog();
		}

		public static void Show(Window owner, IEnumerable<string> messages, IEnumerable<Exception> exceptions)
		{
			string split = Environment.NewLine + "--------------------" + Environment.NewLine;

			var dlg = new SyncErrorDialog();

			dlg.ErrorMessage.Text = string.Join(Environment.NewLine, messages);
			dlg.ErrorTrace.Text = string.Join(split, exceptions.Select(FormatExecption));

			dlg.Owner = owner;

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
	}
}
