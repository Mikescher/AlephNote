using System.Collections;
using AlephNote.Common.Settings.Types;
using AlephNote.PluginInterface;
using AlephNote.WPF.Util;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AlephNote.Common.Settings;
using AlephNote.WPF.Extensions;
using AlephNote.WPF.MVVM;
using System.Linq;
using AlephNote.WPF.Shortcuts;
using AlephNote.WPF.Windows;
using System.Collections.ObjectModel;
using AlephNote.PluginInterface.Util;

namespace AlephNote.WPF.Controls
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
				if (Settings.NoteSorting != SortingMode.None) source.CustomSort = (IComparer)Settings.GetNoteComparator();

				return source;
			}
		}

		public NotesViewFlat()
		{
			InitializeComponent();
			RootGrid.DataContext = this;
		}

		private void OnSettingsChanged()
		{
			//
		}

		private void OnSelectedNoteChanged()
		{
			//
		}

		private void OnAllNotesChanged()
		{
			OnExplicitPropertyChanged("NotesView");
		}

		private void OnSearchTextChanged()
		{
			//
		}

		private bool SearchFilter(INote note)
		{
			return ScintillaSearcher.IsInFilter(note, SearchText);
		}

		private void NotesList_Drop(object sender, DragEventArgs e)
		{
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

		public void RefreshView()
		{
			NotesView.Refresh();
		}

		public bool Contains(INote n)
		{
			return NotesView.Contains(n);
		}

		public void DeleteFolder(DirectoryPath folder)
		{
			// no...
		}

		public void AddFolder(DirectoryPath folder)
		{
			// no...
		}

		public IEnumerable<INote> EnumerateVisibleNotes()
		{
			return NotesView.OfType<INote>();
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
		
		public DirectoryPath GetNewNotesPath()
		{
			return DirectoryPath.Root();
		}
	}
}
