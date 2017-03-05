using System;
using System.Windows;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for UpdateWindow.xaml
	/// </summary>
	public partial class UpdateWindow
	{
		private readonly UpdateWindowViewmodel viewmodel;

		public MainWindow MainWindow;
		public MainWindowViewmodel MainViewmodel;

		private UpdateWindow()
		{
			InitializeComponent();
			this.DataContext = viewmodel = new UpdateWindowViewmodel(this);
		}

		public static void Show(MainWindow owner, MainWindowViewmodel vm, Version onlineVersion, DateTime onlinePublishDate, string onlineDownloadUrl)
		{
			var dlg = new UpdateWindow { MainWindow = owner, MainViewmodel = vm };

			if (owner != null && owner.IsLoaded)
				dlg.Owner = owner;
			else
				dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			dlg.viewmodel.DateOnline = onlinePublishDate;
			dlg.viewmodel.VersionOnline = onlineVersion.ToString(3);

			dlg.viewmodel.URL = onlineDownloadUrl;

			dlg.ShowDialog();
		}
	}
}
