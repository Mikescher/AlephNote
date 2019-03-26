using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
	/// Interaction logic for NotesListView.xaml
	/// </summary>
	public partial class NotesViewFlat : INotifyPropertyChanged, INotesViewControl
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
			typeof(NotesViewFlat),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewFlat)obj).OnSettingsChanged(); }));
		
		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public static readonly DependencyProperty SelectedNoteProperty =
			DependencyProperty.Register(
			"SelectedNote",
			typeof(INote),
			typeof(NotesViewFlat),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesViewFlat)obj).OnSelectedNoteChanged(); }));

		public INote SelectedNote
		{
			get { return (INote)GetValue(SelectedNoteProperty); }
			set { SetValue(SelectedNoteProperty, value); }
		}
		
		private static readonly DependencyProperty SelectedNotesListProperty =
			DependencyProperty.Register(
				"SelectedNotesList",
				typeof(ObservableCollection<INote>),
				typeof(NotesViewFlat),
				new FrameworkPropertyMetadata(null));
		
		public ObservableCollection<INote> SelectedNotesList
		{
			get { return (ObservableCollection<INote>)GetValue(SelectedNotesListProperty); }
			set { throw new Exception("An attempt ot modify Read-Only property"); }
		}

		public static readonly DependencyProperty AllNotesProperty =
			DependencyProperty.Register(
			"AllNotes",
			typeof(ObservableCollection<INote>),
			typeof(NotesViewFlat),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj,args) => { ((NotesViewFlat)obj).OnAllNotesChanged(); }));

		public ObservableCollection<INote> AllNotes
		{
			get { return (ObservableCollection<INote>)GetValue(AllNotesProperty); }
			set { SetValue(AllNotesProperty, value); }
		}

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(NotesViewFlat),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewFlat)obj).OnSearchTextChanged(); }));

		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public static readonly DependencyProperty ParentAnchorProperty =
			DependencyProperty.Register(
			"ParentAnchor",
			typeof(FrameworkElement),
			typeof(NotesViewFlat),
			new FrameworkPropertyMetadata(null));

		public FrameworkElement ParentAnchor
		{
			get { return (FrameworkElement)GetValue(ParentAnchorProperty); }
			set { SetValue(ParentAnchorProperty, value); }
		}

		#endregion

		#region Events

		public event DragEventHandler NotesListDrop;
		public event KeyEventHandler NotesListKeyDown;

		#endregion

		public ListCollectionView NotesView
		{
			get
			{
				if (AllNotes == null) return (ListCollectionView)CollectionViewSource.GetDefaultView(new List<INote>());

				var source = (ListCollectionView)CollectionViewSource.GetDefaultView(AllNotes);
				source.Filter = p => SearchFilter((INote)p);
				source.CustomSort = (IComparer)Settings.GetNoteComparator();

				return source;
			}
		}

		public List<INote> NotesViewManual => AllNotes.Where(SearchFilter).OrderBy(x => x, Settings.GetNoteComparator()).ToList();

		public NotesViewFlat()
		{
			App.Logger.Trace("NotesViewFlat", ".ctr()");

			SetCurrentValue(SelectedNotesListProperty, new ObservableCollection<INote>());

			InitializeComponent();
			RootGrid.DataContext = this;
		}

		private void OnSettingsChanged()
		{
			//
		}

		private void OnSelectedNoteChanged()
		{
			if (SelectedNote != null)
			{
				NotesList.ScrollIntoView(SelectedNote);
			}
		}

		private void OnAllNotesChanged()
		{
			App.Logger.Trace("NotesViewFlat", "OnAllNotesChanged()");
			OnExplicitPropertyChanged("NotesView");
		}

		private void OnSearchTextChanged()
		{
			//
		}

		private bool SearchFilter(INote note)
		{
			return SearchStringParser.Parse(SearchText).IsMatch(note);
		}

		private void NotesList_Drop(object sender, DragEventArgs e)
		{
			App.Logger.Trace("NotesViewFlat", "NotesList_Drop()");
			NotesListDrop?.Invoke(sender, e);
		}

		private void NotesList_KeyDown(object sender, KeyEventArgs e)
		{
			NotesListKeyDown?.Invoke(sender, e);
		}

		public INote GetTopNote()
		{
			return NotesList.Items.FirstOrDefault<INote>();
		}

		public bool IsTopSortedNote(INote n)
		{
			if (Settings?.SortByPinned == true)
				return NotesList.Items.OfType<INote>().FirstOrDefault(p => p.IsPinned == n?.IsPinned) == n;
			else
				return NotesList.Items.FirstOrDefault<INote>() == n;
		}

		public void RefreshView()
		{
			App.Logger.Trace("NotesViewFlat", "RefreshView()");
			NotesView.Refresh();
		}

		public bool Contains(INote n)
		{
			return NotesView.Contains(n);
		}

		public void DeleteFolder(DirectoryPath folder)
		{
			// no...
			Debug.Assert(false);
		}

		public void AddFolder(DirectoryPath folder)
		{
			// no...
			Debug.Assert(false);
		}

		public bool ExternalScrollEmulation(int eDelta)
		{
			var hit = VisualTreeHelper.HitTest(NotesList, Mouse.GetPosition(NotesList));
			if (hit != null)
			{
				var sv = WPFHelper.GetScrollViewer(NotesList);
				if (sv == null) return false;
				
				sv.ScrollToVerticalOffset(sv.VerticalOffset - eDelta/3f);
				return true;
			}

			return false;
		}

		public IEnumerable<INote> EnumerateVisibleNotes()
		{
			return NotesViewManual;
		}

		public void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> shortcuts)
		{
			NotesList.InputBindings.Clear();
			foreach (var sc in shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.NoteList))
			{
				var sckey = sc.Key;
				var cmd = new RelayCommand(() => ShortcutManager.Execute(mw, sckey));
				var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
				NotesList.InputBindings.Add(new InputBinding(cmd, ges));
			}
		}

		public IEnumerable<DirectoryPath> ListFolder()
		{
			yield return DirectoryPath.Root();
		}

		public void FocusNotesList()
		{
			App.Logger.Trace("NotesViewFlat", "FocusNotesList()");

			NotesList.Focus();
			Keyboard.Focus(NotesList);

			NotesList.UpdateLayout();
			if (NotesList.SelectedItem == null) return;

			var listBoxItem = NotesList.ItemContainerGenerator.ContainerFromItem(NotesList.SelectedItem) as ListBoxItem;
			if (listBoxItem == null) return;

			listBoxItem.Focus();
		}

		public void FocusFolderList()
		{
			// control does not exist
		}

		public DirectoryPath GetNewNotesPath()
		{
			return DirectoryPath.Root();
		}

		private void NotesList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var lvi = WPFHelper.VisualLVUpwardSearch(e.OriginalSource as DependencyObject);

			if (lvi != null)
			{
				// click on item

				var cms = NotesViewControlCommon.GetContextMenuNote(ParentAnchor);
				
				NotesList.ContextMenu = null;
				WPFHelper.ExecDelayed(50, () => { NotesList.ContextMenu = cms; cms.IsOpen = true; });
			}
			else
			{
				// click on free space

				var cms = NotesViewControlCommon.GetContextMenuEmpty(ParentAnchor);

				NotesList.ContextMenu = cms;
				cms.IsOpen = true;
			}
		}

		private void NotesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SelectedNotesList == null) SetValue(SelectedNotesListProperty, new ObservableCollection<INote>());
			SelectedNotesList.SynchronizeCollectionSafe(((ListView)sender).SelectedItems.Cast<INote>().ToList());
		}
	}
}
