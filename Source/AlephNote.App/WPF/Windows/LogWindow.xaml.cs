using System.Windows;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow
	{
		public LogWindow()
		{
			InitializeComponent();

			CheckBoxDebug.IsChecked = App.Logger.DebugEnabled;

			DataContext = new LogWindowViewmodel();
		}

		private void OnChangedDebugLog(object sender, RoutedEventArgs e)
		{
			App.Logger.DebugEnabled = CheckBoxDebug.IsChecked ?? false;
		}
	}
}
