﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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

		public static void Show(Window owner, string title, Exception e)
		{
			var dlg = new ExceptionDialog
			{
				ErrorMessage = { Text = e?.Message ?? "Error in ALephNote" },
				ErrorTrace   = { Text = FormatException(e) ?? FormatStacktrace() },
				Title        = title,
			};


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
				ErrorMessage = { Text = message },
				ErrorTrace   = { Text = string.Join(split, new List<Exception> {e}.Concat(additionalExceptions).Select(FormatException)) },
				Title        = title,
			};
			
			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.ShowDialog();
		}

		private static string FormatException(Exception e)
		{
			const string DELIMITER = " ---> ";

			if (e == null) return null;

			var lines = Regex.Split(e.ToString(), @"\r?\n");

			if (lines.Any()) lines[0] = lines[0].Replace(DELIMITER, Environment.NewLine + "    ---> ");

			return string.Join(Environment.NewLine, lines);
		}

		private static string FormatStacktrace()
		{
			return Environment.StackTrace;
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
