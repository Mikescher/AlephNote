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
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.MVVM;
using AlephNote.WPF.Shortcuts;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for NotesViewHierachical.xaml
	/// </summary>
	public partial class NotesViewHierachical : INotifyPropertyChanged, INotesViewControl
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

		private HierachicalFolderWrapper _displayItems = new HierachicalFolderWrapper("ROOT", DirectoryPath.Root(), true);
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
			InitializeComponent();
			RootGrid.DataContext = this;
		}

		private void OnSettingsChanged(DependencyPropertyChangedEventArgs args)
		{
			if (AllNotes != null) ResyncDisplayItems(AllNotes);

			if (args.NewValue != null && args.OldValue == null)
			{
				NotesViewFolderHeight = new GridLength(((AppSettings)args.NewValue).NotesViewFolderHeight);
			}
		}

		private void OnSelectedNoteChanged()
		{
			if (SelectedNote != null && (SelectedFolder == null || !SelectedFolder.AllSubNotes.Contains(SelectedNote)))
			{
				DisplayItems.AllNotesWrapper.IsSelected = true;
			}
		}

		private void OnSelectedFolderChanged()
		{
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
							SelectedNote = n;
						}
					}));
				}).Start();
			}

			var p = SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();

			if (!p.EqualsIgnoreCase(SelectedFolderPath)) SelectedFolderPath = p;
		}

		private void OnSelectedFolderPathChanged()
		{
			var f = DisplayItems.Find(SelectedFolderPath);

			if (f != null)
			{
				if (!f.IsSelected) f.IsSelected = true;
			}
			else
			{
				if (AllNotes == null && !SelectedFolderPath.IsRoot() && !SelectedFolderPath.EqualsIgnoreCase(SelectedFolder?.GetNewNotePath()))
				{
					_initFolderPath = SelectedFolderPath;
					SelectedFolderPath = SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();
				}
				else
				{
					if (!DisplayItems.AllNotesWrapper.IsSelected) DisplayItems.AllNotesWrapper.IsSelected = true;
				}
			}

		}

		private void OnAllNotesChanged(DependencyPropertyChangedEventArgs args)
		{
			if (args.OldValue != null) ((ObservableCollection<INote>)args.OldValue).CollectionChanged -= OnAllNotesCollectionChanged;
			if (args.NewValue != null) ((ObservableCollection<INote>)args.NewValue).CollectionChanged += OnAllNotesCollectionChanged;

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
				SelectedFolderPath = _initFolderPath;
				_initFolderPath = null;
			}
		}

		private void OnAllNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (AllNotes != null) ResyncDisplayItems(AllNotes);
		}

		private void ResyncDisplayItems(IList<INote> notes)
		{
			var root = new HierachicalFolderWrapper("ROOT", DirectoryPath.Root(), true);

			foreach (var note in notes)
			{
				var parent = root;

				foreach (var pathcomp in note.Path.Enumerate())
				{
					parent = parent.GetOrCreateFolder(pathcomp);
				}
				parent.Add(note);
			}

			DisplayItems.Sync(root, new HierachicalFolderWrapper[0]);
		}

		private void OnSearchTextChanged()
		{
			if (AllNotes != null) ResyncDisplayItems(AllNotes);
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

		public IEnumerable<INote> EnumerateVisibleNotes()
		{
			return SelectedFolder?.AllSubNotes ?? Enumerable.Empty<INote>();
		}

		public void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> shortcuts)
		{
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
			foreach (var sc in shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.NoteList))
			{
				var sckey = sc.Key;
				var cmd = new RelayCommand(() => ShortcutManager.Execute(mw, sckey));
				var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
				FolderTreeView.InputBindings.Add(new InputBinding(cmd, ges));
			}
		}
		
		public DirectoryPath GetNewNotesPath()
		{
			return SelectedFolder?.GetNewNotePath() ?? DirectoryPath.Root();
		}

		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			SelectedFolder = FolderTreeView.SelectedItem as HierachicalFolderWrapper;
		}

		private void HierachicalNotesList_KeyDown(object sender, KeyEventArgs e)
		{
			NotesListKeyDown?.Invoke(sender, e);
		}

		private void HierachicalNotesList_Drop(object sender, DragEventArgs e)
		{
			NotesListDrop?.Invoke(sender, e);
		}
	}
}
