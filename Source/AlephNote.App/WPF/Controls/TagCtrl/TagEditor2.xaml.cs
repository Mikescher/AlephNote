using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;

namespace AlephNote.WPF.Controls
{
	public partial class TagEditor2
	{
		
		public static readonly DependencyProperty TagSourceProperty =
			DependencyProperty.Register(
				"TagSource",
				typeof(IList<string>),
				typeof(TagEditor2),
				new FrameworkPropertyMetadata(TagsChanged));

		public IList<string> TagSource
		{
			get { return (IList<string>)GetValue(TagSourceProperty); }
			set { SetValue(TagSourceProperty, value); }
		}

		public static readonly DependencyProperty RepositoryProperty =
			DependencyProperty.Register(
				"Repository",
				typeof(NoteRepository),
				typeof(TagEditor2),
				new FrameworkPropertyMetadata(null));

		public NoteRepository Repository
		{
			get { return (NoteRepository)GetValue(RepositoryProperty); }
			set { SetValue(RepositoryProperty, value); }
		}

		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
				"Settings",
				typeof(AppSettings),
				typeof(TagEditor2),
				new FrameworkPropertyMetadata(null));

		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public static readonly DependencyProperty ReadonlyProperty =
			DependencyProperty.Register(
				"Readonly",
				typeof(bool),
				typeof(TagEditor2),
				new FrameworkPropertyMetadata(false));

		public bool Readonly
		{
			get { return (bool)GetValue(ReadonlyProperty); }
			set { SetValue(ReadonlyProperty, value); }
		}
		
		public string FormattedText
		{
			get
			{
				return ((TagSource != null) ? string.Join(" ", TagSource.Select(t => $"[{t}]")) : string.Empty);
			}
		}

		public event TagEditor.TagsSourceChanged Changed;

		public TagEditor2()
		{
			InitializeComponent();

			MainGrid.DataContext = this;
		}

		private static void TagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//
		}
	}
}
