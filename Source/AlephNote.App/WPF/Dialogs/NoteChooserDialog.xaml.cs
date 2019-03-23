using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using AlephNote.Common.Repository;
using AlephNote.PluginInterface;

namespace AlephNote.WPF.Dialogs
{
	public partial class NoteChooserDialog : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
		
		private ObservableCollection<INote> _notes = new ObservableCollection<INote>();
		public ObservableCollection<INote> Notes { get { return _notes; } set { _notes = value; OnPropertyChanged(); } }

		private INote _selectedNote = null;
		public INote SelectedNote { get { return _selectedNote; } set { _selectedNote = value; OnPropertyChanged(); } }

		private NoteChooserDialog()
		{
			InitializeComponent();

			RootGrid.DataContext = this;
		}

		public static bool ShowInputDialog(Window owner, string title, NoteRepository repo, INote initial, out INote result)
		{
			var notes = repo.Notes
				.OrderBy(p => p.Title.ToLower())
				.ThenBy(p => p.CreationDate)
				.ToList();

			var id = new NoteChooserDialog { Title = title, Notes = new ObservableCollection<INote>(notes), SelectedNote = initial };

			if (owner != null) id.Owner = owner;

			var r = id.ShowDialog();

			if (r == true && id.SelectedNote != null) { result = id.SelectedNote; return true; }

			result = null;
			return false;
		}

		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
