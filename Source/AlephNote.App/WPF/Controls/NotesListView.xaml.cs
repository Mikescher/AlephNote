using AlephNote.Common.Settings.Types;
using AlephNote.PluginInterface;
using AlephNote.Settings;
using AlephNote.WPF.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for NotesListView.xaml
	/// </summary>
	public partial class NotesListView : UserControl, INotifyPropertyChanged
	{
		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnExplicitPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Properties

		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
			"Settings",
			typeof(AppSettings),
			typeof(NotesListView),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesListView)obj).OnSettingsChanged(args); }));
		
		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public static readonly DependencyProperty SelectedNoteProperty =
			DependencyProperty.Register(
			"SelectedNote",
			typeof(INote),
			typeof(NotesListView),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (obj, args) => { ((NotesListView)obj).OnSelectedNoteChanged(args); }));

		public INote SelectedNote
		{
			get { return (INote)GetValue(SelectedNoteProperty); }
			set { SetValue(SelectedNoteProperty, value); }
		}

		public static readonly DependencyProperty AllNotesProperty =
			DependencyProperty.Register(
			"AllNotes",
			typeof(IList<INote>),
			typeof(NotesListView),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj,args) => { ((NotesListView)obj).OnAllNotesChanged(args); }));

		public IList<INote> AllNotes
		{
			get { return (IList<INote>)GetValue(AllNotesProperty); }
			set { SetValue(AllNotesProperty, value); }
		}

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(NotesListView),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesListView)obj).OnSearchTextChanged(args); }));

		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public static readonly DependencyProperty ParentAnchorProperty =
			DependencyProperty.Register(
			"ParentAnchor",
			typeof(FrameworkElement),
			typeof(NotesListView),
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
				if (Settings.NoteSorting != SortingMode.None) source.CustomSort = Settings.GetNoteComparator();

				return source;
			}
		}

		public NotesListView()
		{
			InitializeComponent();
			RootGrid.DataContext = this;
		}

		private void OnSettingsChanged(DependencyPropertyChangedEventArgs args)
		{
			//
		}

		private void OnSelectedNoteChanged(DependencyPropertyChangedEventArgs args)
		{
			//
		}

		private void OnAllNotesChanged(DependencyPropertyChangedEventArgs args)
		{
			OnExplicitPropertyChanged("NotesView");
		}

		private void OnSearchTextChanged(DependencyPropertyChangedEventArgs args)
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
	}
}
