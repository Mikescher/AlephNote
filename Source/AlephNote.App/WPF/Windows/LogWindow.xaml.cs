using System.IO;
using System.Windows;
using System.Xml.Linq;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;
using Microsoft.Win32;

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

			CheckBoxDebug.IsChecked = AlephAppContext.DebugMode;

			DataContext = VM = new LogWindowViewmodel();
		}

		private void OnChangedDebugLog(object sender, RoutedEventArgs e)
		{
			AlephAppContext.DebugMode = CheckBoxDebug.IsChecked ?? false;

			VM?.LogView?.Refresh();
		}

		private void ButtonExport_Click(object sender, RoutedEventArgs e)
		{
			var sfd = new SaveFileDialog { Filter = "Log files (*.xml)|*.xml", FileName = "Log.xml" };

			if (sfd.ShowDialog(this) == true)
			{
				File.WriteAllText(sfd.FileName, App.Logger.Export());
			}
		}

		private void ButtonImport_Click(object sender, RoutedEventArgs e)
		{
			var sfd = new OpenFileDialog { Filter = "Log files (*.xml)|*.xml", FileName = "Log.xml" };

			if (sfd.ShowDialog(this) == true)
			{
				var xdoc = XDocument.Load(sfd.FileName);

				App.Logger.Import(xdoc);
			}
		}
	}
}
