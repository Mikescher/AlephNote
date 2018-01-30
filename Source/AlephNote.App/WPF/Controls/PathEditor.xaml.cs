using AlephNote.Common.Settings;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.MVVM;
using AlephNote.WPF.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for PathEditor.xaml
	/// </summary>
	public partial class PathEditor : UserControl
	{
		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
			"Settings",
			typeof(AppSettings),
			typeof(PathEditor),
			new FrameworkPropertyMetadata(null));

		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
        }

        public static readonly DependencyProperty SelectedNoteProperty =
            DependencyProperty.Register(
            "SelectedNote",
            typeof(INote),
            typeof(PathEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public INote SelectedNote
        {
            get { return (INote)GetValue(SelectedNoteProperty); }
            set { SetValue(SelectedNoteProperty, value); }
        }

        public static readonly DependencyProperty SelectedFolderPathProperty =
            DependencyProperty.Register(
            "SelectedFolderPath",
            typeof(DirectoryPath),
            typeof(PathEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public DirectoryPath SelectedFolderPath
        {
            get { return (DirectoryPath)GetValue(SelectedFolderPathProperty); }
            set { SetValue(SelectedFolderPathProperty, value); }
        }

        public ICommand ChangePathCommand { get { return new RelayCommand(ChangePath); } }

		public PathEditor()
		{
			InitializeComponent();

			MainGrid.DataContext = this;
		}

		public void ChangePath()
		{
			if (SelectedNote == null) return;
			if (!Settings.UseHierachicalNoteStructure) return;

			FolderPopupListView.Items.Clear();

			foreach (var folder in MainWindow.Instance.NotesViewControl.ListFolder()) FolderPopupListView.Items.Add(folder);

			FolderPopupListView.SelectedItem = SelectedNote.Path;

			FolderPopup.IsOpen = true;
        }

        private void ButtonMoveFolder_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedNote == null) return;

            var newPath = FolderPopupListView.SelectedItem as DirectoryPath;
            if (newPath == null) return;

            if (SelectedNote.Path.EqualsWithCase(newPath)) return;

            SelectedNote.Path = newPath;
            SelectedFolderPath = newPath;

            FolderPopup.IsOpen = false;
        }
    }
}
