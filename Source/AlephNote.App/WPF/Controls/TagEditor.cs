using MSHC.Lang.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace AlephNote.WPF.Controls
{
	public class TagEditor : RichTextBox
	{
		public static readonly DependencyProperty TokenTemplateProperty =
			DependencyProperty.Register(
			"TokenTemplate", 
			typeof(DataTemplate), 
			typeof(TagEditor));

		public DataTemplate TokenTemplate
		{
			get { return (DataTemplate)GetValue(TokenTemplateProperty); }
			set { SetValue(TokenTemplateProperty, value); }
		}

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

		public TagEditor()
		{
			PreviewKeyDown += OnKeyDown;
			TextChanged += OnTextChanged;
		}

		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var para = CaretPosition.Paragraph;
			if (para == null) return;

			if (e.Changes.Any(c => c.RemovedLength > 0))
			{
				var doctags = para.Inlines
					.OfType<InlineUIContainer>()
					.Select(r => r.Child)
					.Cast<ContentPresenter>()
					.Select(p => p.Content.ToString())
					.ToList();
				
				if (!TagSource.SequenceEqual(doctags))
				{
					TagSource.SynchronizeSequence(doctags);
				}
			}
		}

		private static void TagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var editor = ((TagEditor) d);

			var vold = e.OldValue as INotifyCollectionChanged;
			var vnew = e.NewValue as INotifyCollectionChanged;

			if (vold != null) vold.CollectionChanged -= editor.OnTagsCollectionChanged;
			if (vnew != null) vnew.CollectionChanged += editor.OnTagsCollectionChanged;

			editor.RecreateTags();
		}

		private void OnTagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			RecreateTags();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Tab)
			{
				var text = CaretPosition.GetTextInRun(LogicalDirection.Backward);

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
				}

			}
		}

		private void RecreateTags()
		{
			TextChanged -= OnTextChanged;

			var para = CaretPosition.Paragraph;

			if (para == null) return;

			para.Inlines.Clear();

			foreach (var tag in TagSource ?? new List<string>())
			{
				var tokenContainer = CreateTokenContainer(tag);
				para.Inlines.Add(tokenContainer);
			}

			CaretPosition = CaretPosition.DocumentEnd;

			TextChanged += OnTextChanged;
		}

		private InlineUIContainer CreateTokenContainer(object token)
		{
			var presenter = new ContentPresenter
			{
				Content = token,
				ContentTemplate = TokenTemplate,
			};

			// BaselineAlignment is needed to align with Run
			return new InlineUIContainer(presenter) { BaselineAlignment = BaselineAlignment.Center };
		}
	}
}
