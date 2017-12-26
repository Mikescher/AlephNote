using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AlephNote.WPF.Dialogs
{
	public partial class FolderNameDialog : INotifyPropertyChanged
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

		public FolderNameDialog()
		{
			InitializeComponent();
			RootGrid.DataContext = this;
			TBox.Focus();
			Keyboard.Focus(TBox);
		}

		private void Button_OK_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Button_Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
