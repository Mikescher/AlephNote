using System.Collections.Generic;
using AlephNote.Log;
using System.Windows.Data;
using System.Windows.Input;
using AlephNote.PluginInterface.AppContext;
using MSHC.WPF.MVVM;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Collections.Specialized;
using MSHC.Util.Helper;

namespace AlephNote.WPF.Windows
{
	class LogWindowViewmodel : ObservableObject
	{
		public ICommand ClearCommand { get { return new RelayCommand(App.Logger.Clear); } }

        public ListCollectionView LogView { get; }

        private LogEvent _selectedLog = null;
		public LogEvent SelectedLog { get { return _selectedLog; } set { _selectedLog = value; OnPropertyChanged(); } }

		public bool IsDebugMode => AlephAppContext.DebugMode;
		
		private bool _showTrace = false;
		public bool ShowTrace { get { return _showTrace; } set { _showTrace = value; OnPropertyChanged(); LogView.Refresh(); DoAutoScroll(); } }

		private bool _showDebug = false;
		public bool ShowDebug { get { return _showDebug; } set { _showDebug = value; OnPropertyChanged(); LogView.Refresh(); if (ShowTrace) ShowTrace=false; DoAutoScroll(); } }

		private bool _autoscroll = false;
		public bool Autoscroll { get { return _autoscroll; } set { _autoscroll = value; OnPropertyChanged(); DoAutoScroll(); } }

		private int _selectedFilterIndex = 0;
		public int SelectedFilterIndex { get { return _selectedFilterIndex; } set { _selectedFilterIndex = value; OnPropertyChanged(); LogView.Refresh(); DoAutoScroll(); } }

		public ObservableCollection<string> Filters { get; } = new ObservableCollection<string>();

		private LogWindow _parent;

		public LogWindowViewmodel(LogWindow lw)
		{
			_parent = lw;

			var events = App.Logger?.GetEventSource();
            events.CollectionChanged += OnLogEvent;

			var source = (ListCollectionView)CollectionViewSource.GetDefaultView(events);
			source.Filter = p => Filter((LogEvent)p);
			LogView = source;

			Filters.Add("[All]");
            foreach (var src in events.Select(p => p.Source).Distinct()) if (!Filters.Contains(src)) Filters.Add(src);
		}

		public LogWindowViewmodel(LogWindow lw, IEnumerable<LogEvent> eventsOverride)
		{
			_parent = lw;

			var events = new ObservableCollection<LogEvent>(eventsOverride);

			var source = (ListCollectionView)CollectionViewSource.GetDefaultView(events);
			source.Filter = p => Filter((LogEvent)p);
			LogView = source;

			Filters.Add("[All]");
			foreach (var src in events.Select(p => p.Source).Distinct()) if (!Filters.Contains(src)) Filters.Add(src);
		}

		private void OnLogEvent(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null) foreach (var src in e.NewItems.Cast<LogEvent>().Select(p => p.Source).Distinct()) if (!Filters.Contains(src)) Filters.Add(src);

			DoAutoScroll();
		}

		private void DoAutoScroll()
		{
			if (Autoscroll) 
			{
				// somwhow ScrollIntoView can sometimes result in ConcurrentModificationException, I think/hope that fixes it...
				DispatcherHelper.InvokeDelayed(() =>
				{
					if (Autoscroll && _parent.MainListView.Items.Count > 0) _parent.MainListView.ScrollIntoView(_parent.MainListView.Items[_parent.MainListView.Items.Count - 1]);
				}, 1);
			}
		}

        private bool Filter(LogEvent p)
		{
			if (!ShowTrace && p.Type == LogEventType.Trace) return false;
			if (!ShowDebug && p.Type == LogEventType.Debug) return false;

			if (SelectedFilterIndex != 0 && p.Source != Filters[SelectedFilterIndex]) return false;

			return true;
		}
	}
}
