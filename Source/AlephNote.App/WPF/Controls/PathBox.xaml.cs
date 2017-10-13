using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows;

namespace AlephNote.WPF.Controls
{
	public partial class PathBox
	{
		public static readonly DependencyProperty PathTextProperty =
			DependencyProperty.Register(
			"PathText",
			typeof(string),
			typeof(PathBox),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public string PathText
		{
			get => (string)GetValue(PathTextProperty);
			set => SetValue(PathTextProperty, value);
		}

		public PathBox()
		{
			InitializeComponent();
			MainGrid.DataContext = this;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new VistaFolderBrowserDialog();
			if (Directory.Exists(PathText)) dialog.SelectedPath = PathText;
			if (dialog.ShowDialog() ?? false) PathText = dialog.SelectedPath;
		}
	}
}
