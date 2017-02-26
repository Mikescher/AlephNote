using System;
using System.Windows;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for UpdateWindow.xaml
	/// </summary>
	public partial class UpdateWindow
	{
		private UpdateWindowViewmodel viewmodel;

		private UpdateWindow()
		{
			InitializeComponent();
			this.DataContext = viewmodel = new UpdateWindowViewmodel();
		}

		public static void Show(Window owner, Version onlineVersion, DateTime onlinePublishDate, string onlineDownloadUrl)
		{
			var dlg = new UpdateWindow { Owner = owner };

			dlg.ShowDialog();
		}
	}
}
