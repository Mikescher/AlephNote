using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AlephNote.WPF.Dialogs
{
	public partial class GenericInputDialog : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		private string _folderName = "";
		public string FolderName { get { return _folderName; } set { _folderName = value; OnPropertyChanged(); } }

		private string _dialogText = "";
		public string DialogText { get { return _dialogText; } set { _dialogText = value; OnPropertyChanged(); } }

		private GenericInputDialog()
		{
			InitializeComponent();
			RootGrid.DataContext = this;
			TBox.Focus();
			Keyboard.Focus(TBox);
		}

		public static bool ShowInputDialog(Window owner, string text, string title, string initial, out string result)
		{
			var id = new GenericInputDialog();
			id.Title = title;
			id.DialogText = text;
			if (owner != null) id.Owner = owner;
			if (initial != null) id.FolderName = initial;

			var r = id.ShowDialog();

			if (r == true) { result = id.FolderName; return true; }

			result = null;
			return false;
		}

		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
