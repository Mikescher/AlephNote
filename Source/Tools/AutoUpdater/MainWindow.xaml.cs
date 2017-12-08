using System;
using System.IO;
using System.Threading;
using System.Windows;
using WPFCustomMessageBox;

namespace AlephNote.AutoUpdater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly string targetPath;
		private readonly string sourcePath;
		
		public MainWindow()
		{
			InitializeComponent();

			var cmdparams = Environment.GetCommandLineArgs();

			if (cmdparams.Length <= 1) Fail("Invalid call arguments");

			sourcePath = AppDomain.CurrentDomain.BaseDirectory;
			targetPath = cmdparams[1];

			if (!Directory.Exists(targetPath)) Fail("Target path not found");
			if (!Directory.Exists(sourcePath)) Fail("Source path not found");

			new Thread(Run).Start();
		}

		private void Fail(string msg)
		{
			MessageBox.Show("AutoUpdater failed - " + msg, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
			Environment.Exit(-1);
		}

		private void Run()
		{
			try
			{
				Update();
			}
			catch (Exception e)
			{
				Fail("Internal Error\r\n" + e.Message);
			}
		}

		private void Update()
		{
			var d = Application.Current.Dispatcher;

			Action<int, int, string> lambdaSetFull = (v, m, t) =>
			{
				d.Invoke(() =>
				{
					Progress.Maximum = m;
					Progress.Value = v;
					InfoBox.Text = t;
				});
			};

			Action<string> lambdaFail = (t) => d.Invoke(() => Fail(t));

			Action<string, bool> lambdaShowMessage = (m, c) =>
			{
				d.Invoke(() =>
				{
					if (c)
					{
						var r = CustomMessageBox.ShowOKCancel(m, "AlephNote AutoUpdater", "Continue", "Abort");
						if (r != MessageBoxResult.OK) Environment.Exit(-1);
					}
					else
					{
						MessageBox.Show(m, "AlephNote AutoUpdater", MessageBoxButton.OK);
					}
				});
			};

			var updater = new ANUpdater(lambdaSetFull, lambdaFail, lambdaShowMessage, sourcePath, targetPath);

			try
			{
				updater.Run();
			}
			catch (Exception e)
			{
				MessageBox.Show("AutoUpdater threw internal error:" + Environment.NewLine + e, "INTERNAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(-1);
			}

			d.Invoke(Close);
		}
	}
}
