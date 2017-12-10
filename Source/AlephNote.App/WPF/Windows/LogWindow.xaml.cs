using System.Windows;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow
	{
		private readonly LogWindowViewmodel VM;

		public LogWindow()
		{
			InitializeComponent();

			CheckBoxDebug.IsChecked = App.Logger.DebugEnabled;

			DataContext = VM = new LogWindowViewmodel();
		}

		private void OnChangedDebugLog(object sender, RoutedEventArgs e)
		{
			App.Logger.DebugEnabled = CheckBoxDebug.IsChecked ?? false;

			VM?.LogView?.Refresh();
		}
	}
}
