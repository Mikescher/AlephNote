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
				source.Filter = p => Filter((LogEvent)p);

				return _logView = source;
			}
		}

		private LogEvent _selectedLog = null;
		public LogEvent SelectedLog { get { return _selectedLog; } set { _selectedLog = value; OnPropertyChanged(); } }

		public bool IsDebugMode => App.DebugMode;

		private bool _showTrace = false;
		public bool ShowTrace { get { return _showTrace; } set { _showTrace = value; OnPropertyChanged(); LogView.Refresh(); } }

		private bool Filter(LogEvent p)
		{
			if (!ShowTrace && p.Type == LogEventType.Trace) return false;

			if (App.Logger.DebugEnabled) return true;
			return p.Type > LogEventType.Debug;
		}
	}
}
