using AlephNote.Common.Settings;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Windows;
using System.Collections.ObjectModel;
using AlephNote.WPF.Util;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using AlephNote.Common.MVVM;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.MVVM;
using AlephNote.WPF.Shortcuts;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for NotesViewHierachical.xaml
	/// </summary>
	public partial class NotesViewHierachical : INotifyPropertyChanged, INotesViewControl, IHierachicalWrapperConfig
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnExplicitPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Properties

		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
			"Settings",
			typeof(AppSettings),
			typeof(NotesViewHierachical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierachical)obj).OnSettingsChanged(args); }));

		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public static readonly DependencyProperty SelectedNoteProperty =
			DependencyProperty.Register(
			"SelectedNote",
			typeof(INote),
			typeof(NotesViewHierachical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierachical)obj).OnSelectedNoteChanged(); }));

		public INote SelectedNote
		{
			get { return (INote)GetValue(SelectedNoteProperty); }
			set { SetValue(SelectedNoteProperty, value); }
		}

		public static readonly DependencyProperty SelectedFolderProperty =
			DependencyProperty.Register(
				"SelectedFolder",
				typeof(HierachicalFolderWrapper),
				typeof(NotesViewHierachical),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierachical)obj).OnSelectedFolderChanged(); }));

		public HierachicalFolderWrapper SelectedFolder
		{
			get { return (HierachicalFolderWrapper)GetValue(SelectedFolderProperty); }
			set { SetValue(SelectedFolderProperty, value); }
		}

		public static readonly DependencyProperty SelectedFolderPathProperty =
			DependencyProperty.Register(
				"SelectedFolderPath",
				typeof(DirectoryPath),
				typeof(NotesViewHierachical),
				new FrameworkPropertyMetadata(DirectoryPath.Root(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierachical)obj).OnSelectedFolderPathChanged(); }));

		public DirectoryPath SelectedFolderPath
		{
			get { return (DirectoryPath)GetValue(SelectedFolderPathProperty); }
			set { SetValue(SelectedFolderPathProperty, value); }
		}

		public static readonly DependencyProperty AllNotesProperty =
			DependencyProperty.Register(
			"AllNotes",
			typeof(ObservableCollection<INote>),
			typeof(NotesViewHierachical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierachical)obj).OnAllNotesChanged(args); }));

		public ObservableCollection<INote> AllNotes
		{
			get { return (ObservableCollection<INote>)GetValue(AllNotesProperty); }
			set { SetValue(AllNotesProperty, value); }
		}

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(NotesViewHierachical),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierachical)obj).OnSearchTextChanged(); }));

		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public static readonly DependencyProperty ParentAnchorProperty =
			DependencyProperty.Register(
			"ParentAnchor",
			typeof(FrameworkElement),
			typeof(NotesViewHierachical),
			new FrameworkPropertyMetadata(null));

		public FrameworkElement ParentAnchor
		{
			get { return (FrameworkElement)GetValue(ParentAnchorProperty); }
			set { SetValue(ParentAnchorProperty, value); }
		}

		private GridLength _notesViewFolderHeight = new GridLength(0);
		public GridLength NotesViewFolderHeight
		{
			get { return _notesViewFolderHeight; }
			set
			{
				if (value != _notesViewFolderHeight)
				{
					_notesViewFolderHeight = value;
					OnPropertyChanged();
					Settings.NotesViewFolderHeight = value.Value;
					GridSplitterChanged?.Invoke(this, null);
				}
			} }

		private HierachicalFolderWrapper _displayItems;
		public HierachicalFolderWrapper DisplayItems { get { return _displayItems; } }

		#endregion

		#region Events

		public event DragEventHandler NotesListDrop;
		public event KeyEventHandler NotesListKeyDown;
		public event EventHandler GridSplitterChanged;

		#endregion

		private DirectoryPath _initFolderPath = null;

		public NotesViewHierachical()
		{
			App.Logger.Trace("NotesViewHierachical", ".ctr()");

			_displayItems = new HierachicalFolderWrapper("ROOT", this, DirectoryPath.Root(), true, false);
			InitializeComponent();
			RootGrid.DataContext = this;
		}

		private void OnSettingsChanged(DependencyPropertyChangedEventArgs args)
		{
			App.Logger.Trace("NotesViewHierachical", "OnSettingsChanged()");

			DisplayItems.ClearPermanents();

			if (AllNotes != null) ResyncDisplayItems(AllNotes);

			if (args.NewValue != null && args.OldValue == null)
			{
				NotesViewFolderHeight = new GridLength(((AppSettings)args.NewValue).NotesViewFolderHeight);
			}

			SelectedFolder?.TriggerAllSubNotesChanged();
		}

		private void OnSelectedNoteChanged()
		{
			App.Logger.Trace("NotesViewHierachical", $"OnSelectedNoteChanged(SelectedNote={SelectedNote?.Title} | SelectedFolder={SelectedFolder?.Header})");

			if (SelectedNote != null && (SelectedFolder == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)))
			{
				App.Logger.Trace("NotesViewHierachical", $"OnSelectedNoteChanged :: AllNotesWrapper.IsSelected = true");
				DisplayItems.AllNotesWrapper.IsSelected = true;
			}
		}

		private void OnSelectedFolderChanged()
		{
			App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderChanged(SelectedNote={SelectedNote?.Title} | SelectedFolder={SelectedFolder?.Header})");

			if (SelectedFolder != null && (SelectedNote == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)) && SelectedFolder.AllSubNotes.Any())
			{
				var n = SelectedFolder.AllSubNotes.FirstOrDefault();
				new Thread(() =>
				{
					Thread.Sleep(50);
					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
					{
						if (SelectedFolder != null && (SelectedNote == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)) && SelectedFolder.AllSubNotes.Any())
						{
							App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderChanged[thread] :: SelectedNote = {n?.Title}");
							SelectedNote = n;
						}
					}));
				}).Start();
			}

			var p = SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();

			if (!p.EqualsIgnoreCase(SelectedFolderPath))
			{
				App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderChanged :: SelectedFolderPath = '{p?.Formatted}'");
				SelectedFolderPath = p;
			}
		}

		private void OnSelectedFolderPathChanged()
		{
			App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderPathChanged(SelectedFolderPath={SelectedFolderPath?.Formatted})");

			if (SelectedFolderPath == null) return;
			if (DisplayItems == null) return;

			var f = DisplayItems.Find(SelectedFolderPath);

			if (f != null)
			{
				if (!f.IsSelected)
				{
					App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderPathChanged :: f.IsSelected = true (f={f?.Header})");
					f.IsSelected = true;
				}
			}
			else
			{
				if (AllNotes == null && !SelectedFolderPath.IsRoot() && !SelectedFolderPath.EqualsIgnoreCase(SelectedFolder?.GetNewNotePath()))
				{
					App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderPathChanged :: SelectedFolderPath = SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root() (SelectedFolder={SelectedFolder?.Header})");
					_initFolderPath = SelectedFolderPath;
					SelectedFolderPath = SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();
				}
				else
				{
					if (!DisplayItems.AllNotesWrapper.IsSelected)
					{
						App.Logger.Trace("NotesViewHierachical", $"OnSelectedFolderPathChanged :: DisplayItems.AllNotesWrapper.IsSelected = true");

						DisplayItems.AllNotesWrapper.IsSelected = true;
					}
				}
			}

		}

		private void OnAllNotesChanged(DependencyPropertyChangedEventArgs args)
		{
			App.Logger.Trace("NotesViewHierachical", $"OnAllNotesChanged()");

			if (args.OldValue != null) ((ObservableCollection<INote>)args.OldValue).CollectionChanged -= OnAllNotesCollectionChanged;
			if (args.NewValue != null) ((ObservableCollection<INote>)args.NewValue).CollectionChanged += OnAllNotesCollectionChanged;

			DisplayItems.ClearPermanents();

			if (args.NewValue != null)
			{
				ResyncDisplayItems((ObservableCollection<INote>)args.NewValue);
			}
			else
			{
				DisplayItems.Clear();
			}

			if (args.OldValue == null && _initFolderPath != null)
			{
				App.Logger.Trace("NotesViewHierachical", $"SelectedFolderPath = _initFolderPath; (_initFolderPath = {_initFolderPath?.Formatted})");
				SelectedFolderPath = _initFolderPath;
				_initFolderPath = null;
			}
		}

		private void OnAllNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			App.Logger.Trace("NotesViewHierachical", $"OnAllNotesCollectionChanged()");

			if (AllNotes != null) ResyncDisplayItems(AllNotes);
		}

		private void ResyncDisplayItems(IList<INote> notes)
		{
			App.Logger.Trace("NotesViewHierachical", $"ResyncDisplayItems()");

			var root = new HierachicalFolderWrapper("ROOT", this, DirectoryPath.Root(), true, false);

			foreach (var note in notes)
			{
				var parent = root;

				foreach (var pathcomp in note.Path.Enumerate())
				{
					parent = parent.GetOrCreateFolder(pathcomp, out _);
				}
				parent.Add(note);
			}

			DisplayItems.CopyPermanentsTo(root);
			root.Sort();
			root.FinalizeCollection(Settings?.DeepFolderView ?? false);

			DisplayItems.Sync(root, new HierachicalFolderWrapper[0]);
		}

		public bool SearchFilter(INote note)
		{
			return ScintillaSearcher.IsInFilter(note, SearchText);
		}

		public IComparer<INote> DisplaySorter()
		{
			return Settings?.GetNoteComparator() ?? AppSettings.COMPARER_NONE;
		}

		private void OnSearchTextChanged()
		{
			App.Logger.Trace("NotesViewHierachical", $"OnSearchTextChanged()");

			if (AllNotes != null) ResyncDisplayItems(AllNotes);

			SelectedFolder?.TriggerAllSubNotesChanged();
		}

		public INote GetTopNote()
		{
			return EnumerateVisibleNotes().FirstOrDefault();
		}

		public void RefreshView()
		{
			if (AllNotes != null) ResyncDisplayItems(AllNotes);
		}

		public bool Contains(INote n)
		{
			return EnumerateVisibleNotes().Contains(n);
		}

		public void AddFolder(DirectoryPath folder)
		{
			App.Logger.Trace("NotesViewHierachical", $"AddFolder({folder?.Formatted})");

			var curr = DisplayItems;

			foreach (var comp in folder.Enumerate())
			{
				curr = curr.GetOrCreateFolder(comp, out var created);
				if (created) curr.Permanent = true;
			}

			SelectedFolderPath = folder;
		}

		public bool ExternalScrollEmulation(int eDelta)
		{
			{
				var hit = VisualTreeHelper.HitTest(HierachicalNotesList, Mouse.GetPosition(HierachicalNotesList));
				if (hit != null)
				{
					var sv = WPFHelper.GetScrollViewer(HierachicalNotesList);
					if (sv == null) return false;

					sv.ScrollToVerticalOffset(sv.VerticalOffset - eDelta/3f);
					return true;
				}
			}

			{
				var hit = VisualTreeHelper.HitTest(FolderTreeView, Mouse.GetPosition(FolderTreeView));
				if (hit != null)
				{
					var sv = WPFHelper.GetScrollViewer(FolderTreeView);
					if (sv == null) return false;

					sv.ScrollToVerticalOffset(sv.VerticalOffset - eDelta/3f);
					return true;
				}
			}

			return false;
		}

		public IEnumerable<INote> EnumerateVisibleNotes()
		{
			return SelectedFolder?.AllSubNotes ?? Enumerable.Empty<INote>();
		}

		public void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> shortcuts)
		{
			App.Logger.Trace("NotesViewHierachical", $"SetShortcuts({shortcuts.Count})");

			HierachicalNotesList.InputBindings.Clear();
			foreach (var sc in shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.NoteList))
			{
				var sckey = sc.Key;
				var cmd = new RelayCommand(() => ShortcutManager.Execute(mw, sckey));
				var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
				HierachicalNotesList.InputBindings.Add(new InputBinding(cmd, ges));
			}

			//---------

			FolderTreeView.InputBindings.Clear();
			foreach (var sc in shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.FolderList))
			{
				var sckey = sc.Key;
				var cmd = new RelayCommand(() => ShortcutManager.Execute(mw, sckey));
				var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
				FolderTreeView.InputBindings.Add(new InputBinding(cmd, ges));
			}
		}

		public IEnumerable<DirectoryPath> ListFolder()
		{
			return DisplayItems.ListPaths().Distinct();
		}

		public DirectoryPath GetNewNotesPath()
		{
			return SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();
		}

		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			App.Logger.Trace("NotesViewHierachical", $"TreeView_SelectedItemChanged({(FolderTreeView.SelectedItem as HierachicalFolderWrapper)?.Header})");

			SelectedFolder = FolderTreeView.SelectedItem as HierachicalFolderWrapper;
		}

		private void HierachicalNotesList_KeyDown(object sender, KeyEventArgs e)
		{
			NotesListKeyDown?.Invoke(sender, e);
		}

		private void HierachicalNotesList_Drop(object sender, DragEventArgs e)
		{
			App.Logger.Trace("NotesViewHierachical", $"HierachicalNotesList_Drop()");

			NotesListDrop?.Invoke(sender, e);
		}

		public void DeleteFolder(DirectoryPath folder)
		{
			App.Logger.Trace("NotesViewHierachical", $"DeleteFolder()");

			DisplayItems.RemoveFind(folder);
			ResyncDisplayItems(AllNotes);
		}

		private void FolderTreeView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem treeViewItem = WPFHelper.VisualTVUpwardSearch(e.OriginalSource as DependencyObject);

			if (treeViewItem != null)
			{
				treeViewItem.Focus();
				e.Handled = true;
			}
		}

		public void FocusNotesList()
		{
			App.Logger.Trace("NotesViewHierachical", $"FocusNotesList()");

			HierachicalNotesList.Focus();
			Keyboard.Focus(HierachicalNotesList);

			HierachicalNotesList.UpdateLayout();
			if (HierachicalNotesList.SelectedItem == null) return;

			var listBoxItem = HierachicalNotesList.ItemContainerGenerator.ContainerFromItem(HierachicalNotesList.SelectedItem) as ListBoxItem;
			if (listBoxItem == null) return;

			listBoxItem.Focus();
		}

		public void FocusFolderList()
		{
			App.Logger.Trace("NotesViewHierachical", $"FocusFolderList()");

			FolderTreeView.Focus();
			Keyboard.Focus(FolderTreeView);
		}
	}
}
