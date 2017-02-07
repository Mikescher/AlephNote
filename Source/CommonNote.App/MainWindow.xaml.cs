using System.Windows;

namespace CommonNote
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			PluginManager.LoadPlugins();
		}
	}
}
