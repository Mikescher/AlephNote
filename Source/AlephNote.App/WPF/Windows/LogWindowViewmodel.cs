using AlephNote.Log;
using MSHC.WPF.MVVM;
using System.Collections.ObjectModel;

namespace AlephNote.WPF.Windows
{
	class LogWindowViewmodel : ObservableObject
	{
		public ObservableCollection<LogEvent> Log { get { return App.Logger.Events; } }

		private LogEvent _selectedLog = null;
		public LogEvent SelectedLog { get { return _selectedLog; } set { _selectedLog = value; OnPropertyChanged(); } }
	}
}
