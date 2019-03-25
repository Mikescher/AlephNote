using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.Common.Util.Search;
using AlephNote.PluginInterface.Datatypes;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Util;
using MSHC.Lang.Collections;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Controls
{
	public partial class TagEditor2 : ITagEditor
	{
		public delegate void TagsSourceChanged(ITagEditor source);

		public static readonly DependencyProperty TagSourceProperty =
			DependencyProperty.Register(
				"TagSource",
				typeof(TagList),
				typeof(TagEditor2),
				new FrameworkPropertyMetadata((d,e) => ((TagEditor2)d).TagsChanged(e)));

		public TagList TagSource
		{
			get { return (TagList)GetValue(TagSourceProperty); }
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
				new FrameworkPropertyMetadata((d,e) => ((TagEditor2)d).SettingsChanged(e)));

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
		
		public ICommand ShowTagChooserCommand { get { return new RelayCommand(ShowTagChooser); } }

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

		private void SettingsChanged(DependencyPropertyChangedEventArgs e)
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
			
			if (TagCtrl?.DropDownTags == null) return;

			if (Settings != null && Settings.TagAutocomplete)
			{
				var hints = Repository
					.EnumerateAllTags()
					.Concat(new[] { AppSettings.TAG_MARKDOWN, AppSettings.TAG_LIST })
					.OrderBy(p => p)
					.Distinct()
					.Except(TagCtrl.EnteredTags)
					.ToList();


				TagCtrl.DropDownTags.SynchronizeCollection(hints);
			}
			else
			{
				TagCtrl.DropDownTags.Clear();
			}
		}

		private void ShowTagChooser()
		{
			void Update(string name, bool check)
			{
				if (check)
				{
					if (TagSource.Any(t => t.ToLower() == name.ToLower())) return;
					TagSource.Add(name);
				}
				else
				{
					var rm = TagSource.FirstOrDefault(t => t.ToLower() == name.ToLower());
					if (rm != null) TagSource.Remove(rm);
				}
			}

			List<CheckableTag> tags = Repository.EnumerateAllTags().Distinct().OrderBy(p=>p.ToLower()).Select(p => new CheckableTag(p, Update)).ToList();
			if (tags.Count==0) return; // no tags

			foreach (var tm in TagSource)
			{
				foreach (var t in tags.Where(p => p.Name.ToLower()==tm)) t.Checked=true;
			}

			TagChoosePopupList.ItemsSource = tags;
			
			TagChoosePopup.IsOpen=true;
		}
	}
}
