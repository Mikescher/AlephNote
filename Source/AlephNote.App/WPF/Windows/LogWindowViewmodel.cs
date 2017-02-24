using AlephNote.Log;
using MSHC.WPF.MVVM;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlephNote.WPF.Windows
{
	class LogWindowViewmodel : ObservableObject
	{
		public ICommand ClearCommand { get { return new RelayCommand(Log.Clear); } }

		public ObservableCollection<LogEvent> Log { get { return App.Logger.Events; } }

		private LogEvent _selectedLog = null;
		public LogEvent SelectedLog { get { return _selectedLog; } set { _selectedLog = value; OnPropertyChanged(); } }
	}
}
