using AlephNote.Common.Settings;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Windows;

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
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierachical)obj).OnSettingsChanged(); }));

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

		public static readonly DependencyProperty AllNotesProperty =
			DependencyProperty.Register(
			"AllNotes",
			typeof(IList<INote>),
			typeof(NotesViewHierachical),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, (obj, args) => { ((NotesViewHierachical)obj).OnAllNotesChanged(); }));

		public IList<INote> AllNotes
		{
			get { return (IList<INote>)GetValue(AllNotesProperty); }
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

		#endregion

		#region Events

		public event DragEventHandler NotesListDrop;
		public event KeyEventHandler NotesListKeyDown;

		#endregion

		public NotesViewHierachical()
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
			//
		}

		private void OnSearchTextChanged()
		{
			//
		}

		public INote GetTopNote()
		{
			throw new NotImplementedException();
		}

		public void RefreshView()
		{
			throw new NotImplementedException();
		}

		public bool Contains(INote n)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<INote> EnumerateVisibleNotes()
		{
			throw new NotImplementedException();
		}

		public void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> list)
		{
			throw new NotImplementedException();
		}
	}
}
