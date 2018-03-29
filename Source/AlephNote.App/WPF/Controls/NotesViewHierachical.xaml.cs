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
using AlephNote.Common.Util.Search;

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
				typeof(HierachicalWrapper_Folder),
				typeof(NotesViewHierachical),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewHierachical)obj).OnSelectedFolderChanged(); }));

		public HierachicalWrapper_Folder SelectedFolder
		{
			get { return (HierachicalWrapper_Folder)GetValue(SelectedFolderProperty); }
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

		private HierachicalWrapper_Folder _displayItems;
		public HierachicalWrapper_Folder DisplayItems { get { return _displayItems; } }

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

			_displayItems = new HierachicalWrapper_Folder("ROOT", this, DirectoryPath.Root(), true, false);
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
			App.Logger.TraceExt("NotesViewHierachical", 
				"OnSelectedNoteChanged", 
				Tuple.Create("SelectedNote", SelectedNote?.Title),
				Tuple.Create("SelectedFolder", SelectedFolder?.Header));

			if (SelectedNote != null && (SelectedFolder == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)))
			{
				if (DisplayItems.AllNotesViewWrapper != null)
				{
					App.Logger.TraceExt("NotesViewHierachical", 
						"OnSelectedNoteChanged (1)",
						Tuple.Create("AllNotesWrapper.IsSelected", "true"));

					DisplayItems.AllNotesViewWrapper.IsSelected = true;
				}
				else
				{
					var fldr = DisplayItems.Find(SelectedNote);
					if (fldr != null)
					{
						App.Logger.TraceExt("NotesViewHierachical", 
							"OnSelectedNoteChanged (2)",
							Tuple.Create("DisplayItems.Find(SelectedNote).IsSelected","true"));

						fldr.IsSelected = true;
					}
				}
			}

			if (SelectedNote != null)
			{
				HierachicalNotesList.ScrollIntoView(SelectedNote);
			}
		}

		private void OnSelectedFolderChanged()
		{
			App.Logger.TraceExt("NotesViewHierachical", 
				"OnSelectedFolderChanged",
				Tuple.Create("SelectedNote", SelectedNote?.Title),
				Tuple.Create("SelectedFolder", SelectedFolder?.Header));

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
							App.Logger.TraceExt("NotesViewHierachical",
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
				App.Logger.TraceExt("NotesViewHierachical", 
					"OnSelectedFolderChanged (2)",
					Tuple.Create("SelectedFolderPath", p.Formatted));

				SelectedFolderPath = p;
			}
		}

		private void OnSelectedFolderPathChanged()
		{
			App.Logger.TraceExt("NotesViewHierachical", 
				"OnSelectedFolderPathChanged",
				Tuple.Create("SelectedFolderPath", SelectedFolderPath?.Formatted));

			if (SelectedFolderPath == null) return;
			if (DisplayItems == null) return;

			var f = DisplayItems.Find(SelectedFolderPath, true);

			if (f != null)
			{
				if (!f.IsSelected)
				{
					App.Logger.TraceExt("NotesViewHierachical", 
						"OnSelectedFolderPathChanged (1)",
						Tuple.Create("f.IsSelected", "true"));

					f.IsSelected = true;
				}
			}
			else
			{
				if (AllNotes == null && !SelectedFolderPath.IsRoot() && !SelectedFolderPath.EqualsIgnoreCase(SelectedFolder?.GetInternalPath()))
				{
					App.Logger.TraceExt("NotesViewHierachical", "OnSelectedFolderPathChanged (2)",
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
							App.Logger.TraceExt("NotesViewHierachical", 
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
							App.Logger.TraceExt("NotesViewHierachical",
								"OnSelectedFolderPathChanged (4)",
								Tuple.Create("DisplayItems.SubFolder.FirstOrDefault().IsSelected", "true"));

							fod.IsSelected = true;
						}
					}
				}
			}

		}

		private void OnAllNotesChanged(DependencyPropertyChangedEventArgs args)
		{
			App.Logger.Trace("NotesViewHierachical", "OnAllNotesChanged (1)");

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
				App.Logger.TraceExt("NotesViewHierachical", "OnAllNotesChanged (2)",
					Tuple.Create("SelectedFolderPath", "_initFolderPath"),
					Tuple.Create("_initFolderPath", _initFolderPath?.Formatted));

				SelectedFolderPath = _initFolderPath;
				_initFolderPath = null;
			}
		}

		private void OnAllNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			App.Logger.Trace("NotesViewHierachical", "OnAllNotesCollectionChanged");

			if (AllNotes != null) ResyncDisplayItems(AllNotes);
		}

		bool IHierachicalWrapperConfig.ShowAllNotesNode  => Settings?.FolderViewShowAllNotesNode ?? false;
		bool IHierachicalWrapperConfig.ShowEmptyPathNode => Settings?.FolderViewShowEmptyPathNode ?? false;

		private void ResyncDisplayItems(IList<INote> notes)
		{
			App.Logger.Trace("NotesViewHierachical", "ResyncDisplayItems", string.Join("\n", notes.Select(n => $"{n.GetUniqueName()}  {n.Title}")));

			HierachicalWrapper_Folder root = new HierachicalWrapper_Folder("ROOT", this, DirectoryPath.Root(), true, false);
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
			root.Sort();
			root.FinalizeCollection(Settings?.DeepFolderView ?? false);

			DisplayItems.Sync(root, new HierachicalWrapper_Folder[0]);
		}

		public bool SearchFilter(INote note)
		{
			return SearchStringParser.Parse(SearchText).IsMatch(note);
		}

		public IComparer<INote> DisplaySorter()
		{
			return Settings?.GetNoteComparator() ?? AppSettings.COMPARER_NOPIN_NONE;
		}

		private void OnSearchTextChanged()
		{
			using (MainWindow.Instance.PreventScintillaFocus())
			{
				App.Logger.TraceExt("NotesViewHierachical",
					"OnSearchTextChanged",
					Tuple.Create("SearchText", SearchText));

				if (AllNotes != null) ResyncDisplayItems(AllNotes);
				SelectedFolder?.TriggerAllSubNotesChanged();

				if (SelectedFolderPath != null && !SelectedFolder.IsSpecialNode_AllNotes && !SelectedFolder.AllSubNotes.Any() && DisplayItems.AllNotesViewWrapper != null)
				{
					App.Logger.TraceExt("NotesViewHierachical",
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
			App.Logger.TraceExt("NotesViewHierachical", 
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
			App.Logger.Trace("NotesViewHierachical", "SetShortcuts", string.Join("\n", shortcuts.Select(s => s.Key.PadRight(48, ' ') + s.Value.Serialize())));

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
			App.Logger.TraceExt("NotesViewHierachical", 
				"TreeView_SelectedItemChanged",
				Tuple.Create("FolderTreeView.SelectedItem", (FolderTreeView.SelectedItem as HierachicalWrapper_Folder)?.Header));

			SelectedFolder = FolderTreeView.SelectedItem as HierachicalWrapper_Folder;
		}

		private void HierachicalNotesList_KeyDown(object sender, KeyEventArgs e)
		{
			NotesListKeyDown?.Invoke(sender, e);
		}

		private void HierachicalNotesList_Drop(object sender, DragEventArgs e)
		{
			App.Logger.TraceExt("NotesViewHierachical", 
				"HierachicalNotesList_Drop",
				Tuple.Create("e.Data.Data", e?.Data?.GetData("Text", true)?.ToString()),
				Tuple.Create("e.Data.Formats", string.Join("; ", e?.Data?.GetFormats() ?? new string[0])));

			NotesListDrop?.Invoke(sender, e);
		}

		public void DeleteFolder(DirectoryPath folder)
		{
			App.Logger.TraceExt("NotesViewHierachical", 
				"DeleteFolder",
				Tuple.Create("folder", folder?.Formatted));

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

			if (treeViewItem != null)
			{
				// click on item

				var cms = new ContextMenu
				{
					Items =
					{
						new AutoActionMenuItem{ Header="Add subfolder", AlephAction="AddSubFolder", ParentAnchor=ParentAnchor},
						new AutoActionMenuItem{ Header="Rename folder", AlephAction="RenameFolder", ParentAnchor=ParentAnchor},
						new Separator(),
						new AutoActionMenuItem{ Header="Delete folder", AlephAction="DeleteFolder", ParentAnchor=ParentAnchor},
					}
				};
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
			App.Logger.Trace("NotesViewHierachical", "FocusNotesList");

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
			App.Logger.Trace("NotesViewHierachical", "FocusFolderList");

			FolderTreeView.Focus();
			Keyboard.Focus(FolderTreeView);
		}

		private void NotesList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			ListViewItem lvi = WPFHelper.VisualLVUpwardSearch(e.OriginalSource as DependencyObject);

			if (lvi != null)
			{
				// click on item

				var cms = new ContextMenu
				{
					Items =
					{
						new AutoActionMenuItem{ Header="Export",      AlephAction="ExportNote",    ParentAnchor=ParentAnchor},
						new AutoActionMenuItem{ Header="Duplicate",   AlephAction="DuplicateNote", ParentAnchor=ParentAnchor},
						new AutoActionMenuItem{ Header="Pin / Unpin", AlephAction="PinUnpinNote",  ParentAnchor=ParentAnchor},
						new Separator(),
						new AutoActionMenuItem{ Header="Delete",      AlephAction="DeleteNote",    ParentAnchor=ParentAnchor},
					}
				};
				HierachicalNotesList.ContextMenu = null;
				WPFHelper.ExecDelayed(100, () => { HierachicalNotesList.ContextMenu = cms; cms.IsOpen = true; });
			}
			else
			{
				// click on free space

				var cms = new ContextMenu
				{
					Items =
					{
						new AutoActionMenuItem{ Header="New Note",                  AlephAction="NewNote",              ParentAnchor=ParentAnchor},
						new AutoActionMenuItem{ Header="New Note (from clipboard)", AlephAction="NewNoteFromClipboard", ParentAnchor=ParentAnchor},
						new AutoActionMenuItem{ Header="New Note (from text file)", AlephAction="NewNoteFromTextFile",  ParentAnchor=ParentAnchor},
					}
				};
				HierachicalNotesList.ContextMenu = cms;
				cms.IsOpen = true;
			}
		}
	}
}
