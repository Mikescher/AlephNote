using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Util;

namespace AlephNote.WPF.Controls
{
	public partial class TagEditor2 : ITagEditor
	{
		public delegate void TagsSourceChanged(ITagEditor source);

		public static readonly DependencyProperty TagSourceProperty =
			DependencyProperty.Register(
				"TagSource",
				typeof(IList<string>),
				typeof(TagEditor2),
				new FrameworkPropertyMetadata((d,e) => ((TagEditor2)d).TagsChanged(e)));

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
				new FrameworkPropertyMetadata((d,e) => ((TagEditor2)d).RepositoryChanged(e)));

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
		
		public event TagsSourceChanged Changed;

		public TagEditor2()
		{
			InitializeComponent();

			MainGrid.DataContext = this;
		}

		private void TagsChanged(DependencyPropertyChangedEventArgs e)
		{
			UpdateHintTags();
		}

		private void RepositoryChanged(DependencyPropertyChangedEventArgs e)
		{
			UpdateHintTags();
		}

		private void TokenizedTagControl_OnChange(object sender, TokenizedTagEventArgs e)
		{
			Changed?.Invoke(this);

			UpdateHintTags();
		}

		private void UpdateHintTags()
		{
			if (Repository == null) return;

			var enteredTags = TagCtrl?.EnteredTags;

			if (enteredTags == null) return;

			var hints = Repository
				.EnumerateAllTags()
				.Concat(new[] { AppSettings.TAG_MARKDOWN, AppSettings.TAG_LIST })
				.OrderBy(p => p)
				.Distinct()
				.Except(TagCtrl.EnteredTags)
				.ToList();

			if (TagCtrl?.DropDownTags == null) return;

			TagCtrl.DropDownTags.SynchronizeCollection(hints);
		}
	}
}
