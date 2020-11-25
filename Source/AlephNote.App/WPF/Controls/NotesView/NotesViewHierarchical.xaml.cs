using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AlephNote.Common.Hierarchy;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Shortcuts;
using AlephNote.Common.Util.Search;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Windows;
using MSHC.Lang.Collections;
using MSHC.WPF;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Controls.NotesView
{
	/// <summary>
	/// Interaction logic for NotesViewHierarchical.xaml
	/// </summary>
	public partial class NotesViewHierarchical : INotifyPropertyChanged, INotesViewControl, IHierarchicalWrapperConfig
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
			typeof(NotesViewHierarchical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierarchical)obj).OnSettingsChanged(args); }));

		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public static readonly DependencyProperty RepositoryAccountIDproperty =
			DependencyProperty.Register(
			"RepositoryAccountID",
			typeof(Guid),
			typeof(NotesViewHierarchical),
			new FrameworkPropertyMetadata(Guid.Empty, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierarchical)obj).OnRepoChanged(args); }));

		public Guid RepositoryAccountID
		{
			get { return (Guid)GetValue(RepositoryAccountIDproperty); }
			set { SetValue(RepositoryAccountIDproperty, value); }
		}

		public static readonly DependencyProperty SelectedNoteProperty =
			DependencyProperty.Register(
			"SelectedNote",
			typeof(INote),
			typeof(NotesViewHierarchical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierarchical)obj).OnSelectedNoteChanged(); }));

		public INote SelectedNote
		{
			get { return (INote)GetValue(SelectedNoteProperty); }
			set { SetValue(SelectedNoteProperty, value); }
		}
		
		private static readonly DependencyProperty SelectedNotesListProperty =
			DependencyProperty.Register(
				"SelectedNotesList",
				typeof(ObservableCollection<INote>),
				typeof(NotesViewHierarchical),
				new FrameworkPropertyMetadata(null));
		
		public ObservableCollection<INote> SelectedNotesList
		{
			get { return (ObservableCollection<INote>)GetValue(SelectedNotesListProperty); }
			set { throw new Exception("An attempt ot modify Read-Only property"); }
		}

		public static readonly DependencyProperty SelectedFolderProperty =
			DependencyProperty.Register(
				"SelectedFolder",
				typeof(HierarchicalWrapper_Folder),
				typeof(NotesViewHierarchical),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierarchical)obj).OnSelectedFolderChanged(); }));

		public HierarchicalWrapper_Folder SelectedFolder
		{
			get { return (HierarchicalWrapper_Folder)GetValue(SelectedFolderProperty); }
			set { SetValue(SelectedFolderProperty, value); }
		}

		public static readonly DependencyProperty SelectedFolderPathProperty =
			DependencyProperty.Register(
				"SelectedFolderPath",
				typeof(DirectoryPath),
				typeof(NotesViewHierarchical),
				new FrameworkPropertyMetadata(DirectoryPath.Root(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierarchical)obj).OnSelectedFolderPathChanged(); }));

		public DirectoryPath SelectedFolderPath
		{
			get { return (DirectoryPath)GetValue(SelectedFolderPathProperty); }
			set { SetValue(SelectedFolderPathProperty, value); }
		}

		public static readonly DependencyProperty AllNotesProperty =
			DependencyProperty.Register(
			"AllNotes",
			typeof(ObservableCollection<INote>),
			typeof(NotesViewHierarchical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierarchical)obj).OnAllNotesChanged(args); }));

		public ObservableCollection<INote> AllNotes
		{
			get { return (ObservableCollection<INote>)GetValue(AllNotesProperty); }
			set { SetValue(AllNotesProperty, value); }
		}

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(NotesViewHierarchical),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierarchical)obj).OnSearchTextChanged(); }));

		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public static readonly DependencyProperty ParentAnchorProperty =
			DependencyProperty.Register(
			"ParentAnchor",
			typeof(FrameworkElement),
			typeof(NotesViewHierarchical),
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

		private HierarchicalWrapper_Folder _displayItems;
		public HierarchicalWrapper_Folder DisplayItems { get { return _displayItems; } }

		#endregion

		#region Events

		public event DragEventHandler NotesListDrop;
		public event KeyEventHandler NotesListKeyDown;
		public event EventHandler GridSplitterChanged;

		#endregion

		private bool _isNotesInitialized = false;
		private readonly HierarchyConfigCache _hierarchyCache;

		private readonly Dictionary<string, bool> _filterData = new Dictionary<string, bool>();

		private DirectoryPath _initFolderPath = null;

		public NotesViewHierarchical()
		{
			App.Logger.Trace("NotesViewHierarchical", ".ctr()");

			_hierarchyCache = HierarchyConfigCache.LoadFromFile(AppSettings.PATH_HIERARCHYCACHE);

			SetCurrentValue(SelectedNotesListProperty, new ObservableCollection<INote>());

			_displayItems = new HierarchicalWrapper_Folder("ROOT", this, DirectoryPath.Root(), true, false);
			InitializeComponent();
			RootGrid.DataContext = this;
		}

		private void OnSettingsChanged(DependencyPropertyChangedEventArgs args)
		{
			App.Logger.Trace("NotesViewHierarchical", "OnSettingsChanged()");

			if (args.NewValue != null && args.OldValue == null)
			{
				NotesViewFolderHeight = new GridLength(((AppSettings)args.NewValue).NotesViewFolderHeight);
			}

		}

		private void OnRepoChanged(DependencyPropertyChangedEventArgs args)
		{
			App.Logger.Trace("NotesViewHierarchical", "OnRepoChanged()");

			DisplayItems.ClearPermanents();

			if (AllNotes != null) ResyncDisplayItems(AllNotes);

			SelectedFolder?.TriggerAllSubNotesChanged();
		}

		private void OnSelectedNoteChanged()
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"OnSelectedNoteChanged", 
				Tuple.Create("SelectedNote", SelectedNote?.Title),
				Tuple.Create("SelectedFolder", SelectedFolder?.Header));

			if (SelectedNote != null && (SelectedFolder == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)))
			{
				if (DisplayItems.AllNotesViewWrapper != null)
				{
					App.Logger.TraceExt("NotesViewHierarchical", 
						"OnSelectedNoteChanged (1)",
						Tuple.Create("AllNotesWrapper.IsSelected", "true"));

					DisplayItems.AllNotesViewWrapper.IsSelected = true;
				}
				else
				{
					var fldr = DisplayItems.Find(SelectedNote);
					if (fldr != null)
					{
						App.Logger.TraceExt("NotesViewHierarchical", 
							"OnSelectedNoteChanged (2)",
							Tuple.Create("DisplayItems.Find(SelectedNote).IsSelected","true"));

						fldr.IsSelected = true;
					}
				}
			}

			if (SelectedNote != null)
			{
				HierarchicalNotesList.ScrollIntoView(SelectedNote);
			}

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		private void OnSelectedFolderChanged()
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"OnSelectedFolderChanged",
				Tuple.Create("SelectedNote", SelectedNote?.Title),
				Tuple.Create("SelectedFolder", SelectedFolder?.Header));

			if (SelectedFolder != null && (SelectedNote == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)) && SelectedFolder.AllSubNotes.Any())
			{
				var n = SelectedFolder.AllSubNotes.FirstOrDefault();
				var oldfolder = SelectedFolder.GetInternalPath();
				new Thread(() =>
				{
					Thread.Sleep(50);
					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
					{
						if (SelectedFolder == null) return;
						if (!SelectedFolder.GetInternalPath().EqualsWithCase(oldfolder))
						{
							App.Logger.Debug("NotesViewHierarchical", "Prevent invalidated SelectedFolderChanged event to execute", $"'{SelectedFolder.GetInternalPath()}' <> '{oldfolder}'");
							return;
						}

						if (SelectedFolder != null && (SelectedNote == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)) && SelectedFolder.AllSubNotes.Any())
						{
							App.Logger.TraceExt("NotesViewHierarchical",
								"OnSelectedFolderChanged (1) [thread]",
								Tuple.Create("SelectedNote" ,n?.Title));

							SelectedNote = n;
						}
					}));
				}).Start();
			}

			var p = SelectedFolder?.GetInternalPath() ?? DirectoryPath.Root();

			if (!p.EqualsIgnoreCase(SelectedFolderPath))
			{
				App.Logger.TraceExt("NotesViewHierarchical", 
					"OnSelectedFolderChanged (2)",
					Tuple.Create("SelectedFolderPath", p.Formatted));

				SelectedFolderPath = p;
			}

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		private void OnSelectedFolderPathChanged()
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"OnSelectedFolderPathChanged",
				Tuple.Create("SelectedFolderPath", SelectedFolderPath?.Formatted));

			if (SelectedFolderPath == null) return;
			if (DisplayItems == null) return;

			var f = DisplayItems.Find(SelectedFolderPath, true);

			if (f != null)
			{
				if (!f.IsSelected)
				{
					App.Logger.TraceExt("NotesViewHierarchical", 
						"OnSelectedFolderPathChanged (1)",
						Tuple.Create("f.IsSelected", "true"));

					f.IsSelected = true;
				}
			}
			else
			{
				if (AllNotes == null && !SelectedFolderPath.IsRoot() && !SelectedFolderPath.EqualsIgnoreCase(SelectedFolder?.GetInternalPath()))
				{
					App.Logger.TraceExt("NotesViewHierarchical", "OnSelectedFolderPathChanged (2)",
						Tuple.Create("SelectedFolder", SelectedFolder?.Header));

					_initFolderPath = SelectedFolderPath;
					SelectedFolderPath = SelectedFolder?.GetInternalPath() ?? DirectoryPath.Root();
				}
				else
				{
					if (DisplayItems.AllNotesViewWrapper != null)
					{
						if (!DisplayItems.AllNotesViewWrapper.IsSelected)
						{
							App.Logger.TraceExt("NotesViewHierarchical", 
								"OnSelectedFolderPathChanged (3)",
								Tuple.Create("DisplayItems.AllNotesWrapper.IsSelected", "true"));

							DisplayItems.AllNotesViewWrapper.IsSelected = true;
						}
					}
					else
					{
						var fod = DisplayItems.SubFolder.FirstOrDefault(p=>!p.IsSpecialNode);
						if (fod != null && !fod.IsSelected)
						{
							App.Logger.TraceExt("NotesViewHierarchical",
								"OnSelectedFolderPathChanged (4)",
								Tuple.Create("DisplayItems.SubFolder.FirstOrDefault().IsSelected", "true"));

							fod.IsSelected = true;
						}
					}
				}
			}

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		private void OnAllNotesChanged(DependencyPropertyChangedEventArgs args)
		{
			App.Logger.Trace("NotesViewHierarchical", "OnAllNotesChanged (1)");

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
				App.Logger.TraceExt("NotesViewHierarchical", "OnAllNotesChanged (2)",
					Tuple.Create("SelectedFolderPath", "_initFolderPath"),
					Tuple.Create("_initFolderPath", _initFolderPath?.Formatted));

				SelectedFolderPath = _initFolderPath;
				_initFolderPath = null;
			}

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, DisplayItems, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		private void OnAllNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			App.Logger.Trace("NotesViewHierarchical", "OnAllNotesCollectionChanged");

			if (AllNotes != null) ResyncDisplayItems(AllNotes);
		}

		bool IHierarchicalWrapperConfig.ShowAllNotesNode  => Settings?.FolderViewShowAllNotesNode  ?? false;
		bool IHierarchicalWrapperConfig.ShowEmptyPathNode => Settings?.FolderViewShowEmptyPathNode ?? false;

		private void ResyncDisplayItems(IList<INote> notes)
		{
			App.Logger.Trace("NotesViewHierarchical", "ResyncDisplayItems", string.Join("\n", notes.Select(n => $"{n.UniqueName}  {n.Title}")));
			_isNotesInitialized = false;

			HierarchicalWrapper_Folder root = new HierarchicalWrapper_Folder("ROOT", this, DirectoryPath.Root(), true, false);
			if (Settings?.FolderViewShowRootNode == true)
			{
				var root2 = root.GetOrCreateRootFolder();
				foreach (var note in notes)
				{
					var parent = root2;
					foreach (var pathcomp in note.Path.Enumerate())
					{
						parent = parent.GetOrCreateFolder(pathcomp, out _);
					}
					parent.Add(note);
				}
			}
			else
			{
				foreach (var note in notes)
				{
					var parent = root;
					foreach (var pathcomp in note.Path.Enumerate())
					{
						parent = parent.GetOrCreateFolder(pathcomp, out _);
					}
					parent.Add(note);
				}
			}

			DisplayItems.CopyPermanentsTo(root);
			_hierarchyCache.Get(Settings.ActiveAccount.ID).ApplyTo(Settings, root);
			root.Sort();
			root.FinalizeCollection(Settings?.DeepFolderView ?? false); // Also refreshes/synchronizes SelectedFolder.AllSubNotes

			DisplayItems.Sync(root, new HierarchicalWrapper_Folder[0]);

			_isNotesInitialized = true;
		}

		public bool SearchFilter(INote note)
		{
			if (_filterData.TryGetValue(note.UniqueName, out var cachedResult)) return cachedResult;

			return _filterData[note.UniqueName] = SearchStringParser.Parse(SearchText).IsMatch(note);
		}

		public IComparer<INote> DisplaySorter()
		{
			return Settings?.GetNoteComparator() ?? AppSettings.COMPARER_NOPIN_NONE;
		}

		private void OnSearchTextChanged()
		{
			using (MainWindow.Instance.PreventScintillaFocus())
			{
				App.Logger.TraceExt("NotesViewHierarchical",
					"OnSearchTextChanged",
					Tuple.Create("SearchText", SearchText));

				_filterData.Clear();
				if (AllNotes != null) ResyncDisplayItems(AllNotes);
				SelectedFolder?.TriggerAllSubNotesChanged();

				if (SelectedFolderPath != null && !SelectedFolder.IsSpecialNode_AllNotes && !SelectedFolder.AllSubNotes.Any() && DisplayItems.AllNotesViewWrapper != null)
				{
					App.Logger.TraceExt("NotesViewHierarchical",
						"OnSearchTextChanged (2)",
						Tuple.Create("SelectedFolderPath", SelectedFolderPath?.Formatted));

					SelectedFolder = DisplayItems.AllNotesViewWrapper;
				}
			}
		}

		public INote GetTopNote()
		{
			return EnumerateVisibleNotes().FirstOrDefault();
		}

		public bool IsTopSortedNote(INote n)
		{
			if (Settings?.SortByPinned == true)
				return EnumerateVisibleNotes().FirstOrDefault(p => p.IsPinned == n?.IsPinned) == n;
			else
				return EnumerateVisibleNotes().FirstOrDefault() == n;
		}

		public void RefreshView(bool refreshFilter)
		{
			App.Logger.Trace("NotesViewHierarchical", $"RefreshView({refreshFilter})");

			if (refreshFilter) _filterData.Clear();

			if (AllNotes != null) ResyncDisplayItems(AllNotes);
		}

		public bool Contains(INote n)
		{
			return EnumerateVisibleNotes().Contains(n);
		}

		public void AddFolder(DirectoryPath folder)
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"AddFolder",
				Tuple.Create("folder", folder.Formatted));

			var curr = DisplayItems;

			if (Settings?.FolderViewShowRootNode == true) curr = DisplayItems.GetOrCreateRootFolder();

			foreach (var comp in folder.Enumerate())
			{
				curr = curr.GetOrCreateFolder(comp, out var created);
				if (created) curr.Permanent = true;
			}

			SelectedFolderPath = folder;

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, DisplayItems, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		public void MoveFolder(DirectoryPath folder, int delta)
		{
			App.Logger.TraceExt("NotesViewHierarchical",
				"MoveFolder",
				Tuple.Create("folder", folder.Formatted),
				Tuple.Create("delta", delta.ToString()));

			if (folder.IsRoot()) return;

			var curr = DisplayItems;

			if (Settings?.FolderViewShowRootNode == true) curr = DisplayItems.GetRootFolder();
			if (curr == null)
            {
				App.Logger.Warn("NotesViewHierarchical", "MoveFolder encountered missing (root) path");
				return;
			}

			HierarchicalWrapper_Folder currParent = null;
			foreach (var comp in folder.Enumerate())
			{
				currParent = curr;
				curr = curr.GetFolder(comp);
				if (curr == null)
				{
					App.Logger.Warn("NotesViewHierarchical", "MoveFolder encountered missing path", $"folder := '{folder.Formatted}'\ncomp := '{comp}'");
					return;
				}
			}
			if (curr == null || currParent == null)
			{
				App.Logger.Warn("NotesViewHierarchical", "MoveFolder encountered missing parent");
				return;
			}

			var counter = 1;
            foreach (var f in currParent.SubFolder) { f.CustomOrder = counter * 100; counter++; }
			curr.CustomOrder += delta * 100 + Math.Sign(delta) * 50;
			currParent.Sort(false);
			counter = 1;
			foreach (var f in currParent.SubFolder) { f.CustomOrder = counter * 100; counter++; }

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, DisplayItems, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		public bool ExternalScrollEmulation(int eDelta)
		{
			{
				var hit = VisualTreeHelper.HitTest(HierarchicalNotesList, Mouse.GetPosition(HierarchicalNotesList));
				if (hit != null)
				{
					var sv = WPFHelper.GetScrollViewer(HierarchicalNotesList);
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
			App.Logger.Trace("NotesViewHierarchical", "SetShortcuts", string.Join("\n", shortcuts.Select(s => s.Key.PadRight(48, ' ') + s.Value.Serialize())));

			HierarchicalNotesList.InputBindings.Clear();
			foreach (var sc in shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.NoteList))
			{
				var sckey = sc.Key;
				var cmd = new RelayCommand(() => ShortcutManager.Execute(mw, sckey));
				var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
				HierarchicalNotesList.InputBindings.Add(new InputBinding(cmd, ges));
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
			if (Settings?.FolderViewShowRootNode == false)
				return new []{ DirectoryPath.Root() }.Concat(DisplayItems.ListPaths()).Distinct();
			else
				return DisplayItems.ListPaths().Distinct();
		}

		public DirectoryPath GetNewNotesPath()
		{
			return SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();
		}

		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"TreeView_SelectedItemChanged",
				Tuple.Create("FolderTreeView.SelectedItem", (FolderTreeView.SelectedItem as HierarchicalWrapper_Folder)?.Header));

			SelectedFolder = FolderTreeView.SelectedItem as HierarchicalWrapper_Folder;
		}

		private void HierarchicalNotesList_KeyDown(object sender, KeyEventArgs e)
		{
			NotesListKeyDown?.Invoke(sender, e);
		}

		private void HierarchicalNotesList_Drop(object sender, DragEventArgs e)
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"HierarchicalNotesList_Drop",
				Tuple.Create("e.Data.Data", e?.Data?.GetData("Text", true)?.ToString()),
				Tuple.Create("e.Data.Formats", string.Join("; ", e?.Data?.GetFormats() ?? new string[0])));

			NotesListDrop?.Invoke(sender, e);
		}

		public void DeleteFolder(DirectoryPath folder)
		{
			App.Logger.TraceExt("NotesViewHierarchical", 
				"DeleteFolder",
				Tuple.Create("folder", folder?.Formatted));

			DisplayItems.RemoveFind(folder);
			ResyncDisplayItems(AllNotes);

			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, DisplayItems, SelectedFolderPath, SelectedNote?.UniqueName);
		}

		private void FolderTreeView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			TreeViewItem treeViewItem = WPFHelper.VisualTVUpwardSearch(e.OriginalSource as DependencyObject);

			if (treeViewItem != null)
			{
				treeViewItem.Focus();
				e.Handled = true;
			}

			if (treeViewItem != null)
			{
				// click on item

				var cms = new ContextMenu();

				cms.Items.Add(new AutoActionMenuItem{ Header="Add subfolder", AlephAction="AddSubFolder", ParentAnchor=ParentAnchor});
				cms.Items.Add(new AutoActionMenuItem{ Header="Rename folder", AlephAction="RenameFolder", ParentAnchor=ParentAnchor});
				cms.Items.Add(new Separator());
				if (!Settings.SortHierarchyFoldersByName)
				{
					cms.Items.Add(new AutoActionMenuItem { Header = "Move up",   AlephAction = "MoveFolderUp",   ParentAnchor = ParentAnchor });
					cms.Items.Add(new AutoActionMenuItem { Header = "Move down", AlephAction = "MoveFolderDown", ParentAnchor = ParentAnchor });
					cms.Items.Add(new Separator());
				}
				cms.Items.Add(new AutoActionMenuItem { Header = "Delete folder", AlephAction = "DeleteFolder", ParentAnchor = ParentAnchor });

				FolderTreeView.ContextMenu = null;
				WPFHelper.ExecDelayed(100, () => { FolderTreeView.ContextMenu = cms; cms.IsOpen = true; });
			}
			else
			{
				// click on free space

				var cms = new ContextMenu
				{
					Items =
					{
						new AutoActionMenuItem{ Header="Add subfolder", AlephAction="AddSubFolder", ParentAnchor=ParentAnchor},
					}
				};
				FolderTreeView.ContextMenu = cms;
				cms.IsOpen = true;
			}
		}

		public void FocusNotesList()
		{
			App.Logger.Trace("NotesViewHierarchical", "FocusNotesList");

			HierarchicalNotesList.Focus();
			Keyboard.Focus(HierarchicalNotesList);

			HierarchicalNotesList.UpdateLayout();
			if (HierarchicalNotesList.SelectedItem == null) return;

			var listBoxItem = HierarchicalNotesList.ItemContainerGenerator.ContainerFromItem(HierarchicalNotesList.SelectedItem) as ListBoxItem;
			if (listBoxItem == null) return;

			listBoxItem.Focus();
		}

		public void FocusFolderList()
		{
			App.Logger.Trace("NotesViewHierarchical", "FocusFolderList");

			FolderTreeView.Focus();
			Keyboard.Focus(FolderTreeView);
		}

		private void NotesList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var lvi = WPFHelper.VisualLVUpwardSearch(e.OriginalSource as DependencyObject);

			if (lvi != null)
			{
				// click on item
				
				var cms = NotesViewControlCommon.GetContextMenuNote(ParentAnchor);

				HierarchicalNotesList.ContextMenu = null;
				WPFHelper.ExecDelayed(100, () => { HierarchicalNotesList.ContextMenu = cms; cms.IsOpen = true; });
			}
			else
			{
				// click on free space
				
				var cms = NotesViewControlCommon.GetContextMenuEmpty(ParentAnchor);

				HierarchicalNotesList.ContextMenu = cms;
				cms.IsOpen = true;
			}
		}

		private void HierarchicalNotesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SelectedNotesList == null) SetValue(SelectedNotesListProperty, new ObservableCollection<INote>());
			SelectedNotesList.SynchronizeCollectionSafe(((ListView)sender).SelectedItems.Cast<INote>().ToList());
		}

        public void ForceSaveNow()
        {
			_hierarchyCache.ForceSaveNow();
		}

		public void SaveIfDirty()
		{
			_hierarchyCache.SaveIfDirty();
		}

		private void OnTreeExpanded(object sender, RoutedEventArgs e)
		{
			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, DisplayItems, SelectedFolderPath, SelectedNote?.UniqueName);

			e.Handled = true;
		}

		private void OnTreeCollapsed(object sender, RoutedEventArgs e)
		{
			if (_isNotesInitialized) _hierarchyCache.UpdateAndRequestSave(RepositoryAccountID, DisplayItems, SelectedFolderPath, SelectedNote?.UniqueName);

			e.Handled = true;
		}
	}
}
