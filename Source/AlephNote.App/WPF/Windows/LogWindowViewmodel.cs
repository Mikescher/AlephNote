using System.Collections.Generic;
using AlephNote.Log;
using AlephNote.WPF.MVVM;
using System.Windows.Data;
using System.Windows.Input;
using AlephNote.Common.MVVM;

namespace AlephNote.WPF.Windows
{
	class LogWindowViewmodel : ObservableObject
	{
		public ICommand ClearCommand { get { return new RelayCommand(App.Logger.Events.Clear); } }

		private ListCollectionView _logView;
		public ListCollectionView LogView
		{
			get
			{
				if (_logView != null) return _logView;

				if (App.Logger?.Events == null) return (ListCollectionView)CollectionViewSource.GetDefaultView(new List<LogEvent>());

				var source = (ListCollectionView)CollectionViewSource.GetDefaultView(App.Logger.Events);
				source.Filter = p => (App.Logger.DebugEnabled || ((LogEvent)p).Type != LogEventType.Debug);

				return _logView = source;
			}
		}

		private LogEvent _selectedLog = null;
		public LogEvent SelectedLog { get { return _selectedLog; } set { _selectedLog = value; OnPropertyChanged(); } }
	}
}
