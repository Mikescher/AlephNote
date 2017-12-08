using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using AlephNote.Common.Extensions;
using AlephNote.Common.Repository;
using AlephNote.Common.Settings;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for TagEditor.xaml
	/// </summary>
	public partial class TagEditor : INotifyPropertyChanged
	{
		public delegate void TagsSourceChanged(TagEditor source);

		public static readonly DependencyProperty TagSourceProperty =
			DependencyProperty.Register(
			"TagSource",
			typeof(IList<string>),
			typeof(TagEditor),
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
			typeof(TagEditor),
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
			typeof(TagEditor),
			new FrameworkPropertyMetadata(null));

		public AppSettings Settings
		{
			get { return (AppSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public string FormattedText
		{
			get
			{
				return ((TagSource != null) ? string.Join(" ", TagSource.Select(t => $"[{t}]")) : string.Empty) + " " + new TextRange(TagCtrl.Document.ContentStart, TagCtrl.Document.ContentEnd).Text.Trim();
			}
		}

		public event TagsSourceChanged Changed;

		public TagEditor()
		{
			InitializeComponent();

			MainGrid.DataContext = this;

			TagCtrl.PreviewKeyDown += OnKeyDown;
			TagCtrl.TextChanged += OnTextChanged;
		}

		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var para = TagCtrl.CaretPosition.Paragraph;

			if (para != null && e.Changes.Any(c => c.RemovedLength > 0))
			{
				var doctags = para.Inlines
					.OfType<InlineUIContainer>()
					.Select(r => r.Child)
					.Cast<ContentPresenter>()
					.Select(p => p.Content.ToString())
					.ToList();

				if (!TagSource.SequenceEqual(doctags))
				{
					TagSource.SynchronizeCollection(doctags);

					Changed?.Invoke(this);
				}
			}

			var text = new TextRange(TagCtrl.Document.ContentStart, TagCtrl.Document.ContentEnd).Text.Trim();

			if (text.Length >= 2 && Repository != null && Settings != null && Settings.TagAutocomplete)
			{
				var hints = Repository
					.EnumerateAllTags()
					.Concat(new[] { AppSettings.TAG_MARKDOWN, AppSettings.TAG_LIST })
					.OrderBy(p => p)
					.Distinct()
					.Except(TagSource)
					.Where(t => t.ToLower().StartsWith(text.ToLower()))
					.ToList();

				if (hints.Any())
				{
					AutocompleteContent.Items.SynchronizeGenericCollection(hints);
					AutocompleteContent.SelectedIndex = -1;

					AutocompletePopup.Width = MainGrid.ActualWidth;
					AutocompletePopup.IsOpen = true;
				}
				else
				{
					AutocompletePopup.IsOpen = false;
				}
			}
			else
			{
				AutocompletePopup.IsOpen = false;
			}

			
			OnExplicitPropertyChanged("FormattedText");
		}

		private static void TagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var editor = ((TagEditor)d);

			var vold = e.OldValue as INotifyCollectionChanged;
			var vnew = e.NewValue as INotifyCollectionChanged;

			if (vold != null) vold.CollectionChanged -= editor.OnTagsCollectionChanged;
			if (vnew != null) vnew.CollectionChanged += editor.OnTagsCollectionChanged;

			editor.RecreateTags();

			editor.Changed?.Invoke(editor);

			editor.OnExplicitPropertyChanged("FormattedText");
		}

		private void OnTagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RecreateTags();

			Changed?.Invoke(this);

			OnExplicitPropertyChanged("FormattedText");
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Tab)
			{
				var text = TagCtrl.CaretPosition.GetTextInRun(LogicalDirection.Backward);

				if (string.IsNullOrWhiteSpace(text) && e.Key == Key.Enter)
				{
					e.Handled = true;
				}
				else if (string.IsNullOrWhiteSpace(text) && e.Key == Key.Tab)
				{
					e.Handled = false;
				}
				else
				{
					TagSource.Add(text);
					e.Handled = true;

					Changed?.Invoke(this);
				}

				AutocompletePopup.IsOpen = false;
			}
		}

		private void RecreateTags()
		{
			TagCtrl.TextChanged -= OnTextChanged;

			var para = TagCtrl.CaretPosition.Paragraph;

			if (para == null) return;

			para.Inlines.Clear();

			foreach (var tag in TagSource ?? new List<string>())
			{
				var tokenContainer = CreateTokenContainer(tag);
				para.Inlines.Add(tokenContainer);
			}

			TagCtrl.CaretPosition = TagCtrl.CaretPosition.DocumentEnd;

			TagCtrl.TextChanged += OnTextChanged;
		}

		private InlineUIContainer CreateTokenContainer(object token)
		{
			var presenter = new ContentPresenter
			{
				Content = token,
				ContentTemplate = (DataTemplate)FindResource("TagTemplate"),
			};

			// BaselineAlignment is needed to align with Run
			return new InlineUIContainer(presenter) { BaselineAlignment = BaselineAlignment.Center };
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnExplicitPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void AutocompleteContent_Selected(object sender, RoutedEventArgs e)
		{
			if (AutocompleteContent.SelectedIndex >= 0 && AutocompleteContent.SelectedValue != null)
			{
				var tag = (string)AutocompleteContent.SelectedValue;

				TagSource.Add(tag);

				Changed?.Invoke(this);

				AutocompletePopup.IsOpen = false;
			}
		}
	}
}
