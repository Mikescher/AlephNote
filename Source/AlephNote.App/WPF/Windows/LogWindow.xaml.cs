using System.Windows;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow : Window
	{
		public LogWindow()
		{
			InitializeComponent();

			this.DataContext = new LogWindowViewmodel();
		}
	}
}
