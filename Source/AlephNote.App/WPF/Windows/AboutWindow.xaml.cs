using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow 
	{
		public AboutWindow()
		{
			InitializeComponent();

			this.DataContext = new AboutWindowViewmodel();
		}

		private void Hyperlink_Clicked(object sender, MouseButtonEventArgs e)
		{
			Process.Start(((FrameworkElement) sender).Tag.ToString());
		}
	}
}
