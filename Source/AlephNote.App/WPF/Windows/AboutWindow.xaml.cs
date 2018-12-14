using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MSHC;

namespace AlephNote.WPF.Windows
{
	public partial class AboutWindow 
	{
		public AboutWindow()
		{
			InitializeComponent();

			DataContext = new AboutWindowViewmodel();
		}

		private void Hyperlink_Clicked(object sender, MouseButtonEventArgs e)
		{
			Process.Start(((FrameworkElement) sender).Tag.ToString());
		}
	}
}
