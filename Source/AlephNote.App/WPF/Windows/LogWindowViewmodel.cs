using System.Collections.Generic;
using AlephNote.Log;
using System.Windows.Data;
using System.Windows.Input;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;
using MSHC.WPF.MVVM;
using RelayCommand = AlephNote.WPF.MVVM.RelayCommand;

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

		public bool IsDebugMode => AlephAppContext.DebugMode;
		
		private bool _showTrace = false;
		public bool ShowTrace { get { return _showTrace; } set { _showTrace = value; OnPropertyChanged(); LogView.Refresh(); } }

		private bool _showDebug = false;
		public bool ShowDebug { get { return _showDebug; } set { _showDebug = value; OnPropertyChanged(); LogView.Refresh(); if (ShowTrace) ShowTrace=false; } }

		private bool Filter(LogEvent p)
		{
			if (!ShowTrace && p.Type == LogEventType.Trace) return false;
			if (!ShowDebug && p.Type == LogEventType.Debug) return false;

			return true;
		}
	}
}
