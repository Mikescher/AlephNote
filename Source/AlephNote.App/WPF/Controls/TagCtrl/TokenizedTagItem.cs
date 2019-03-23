using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MSHC.WPF;

namespace AlephNote.WPF.Controls
{
	[TemplatePart(Name = "PART_InputBox", Type = typeof(AutoCompleteBox))]
	[TemplatePart(Name = "PART_DeleteTagButton", Type = typeof(Button))]
	[TemplatePart(Name = "PART_TagButton", Type = typeof(Button))]
	[DebuggerDisplay("TTI [Text={Text,nq}, IsEditing={IsEditing,nq}]")]
	public class TokenizedTagItem : Control
	{
		public static readonly DependencyProperty TextProperty = 
			DependencyProperty.Register(
				"Text", 
				typeof(string), 
				typeof(TokenizedTagItem), 
				new PropertyMetadata(null));
		
		public string Text 
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
		
		private static readonly DependencyPropertyKey IsEditingPropertyKey = 
			DependencyProperty.RegisterReadOnly(
				"IsEditing", 
				typeof(bool), 
				typeof(TokenizedTagItem), 
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;
		
		public bool IsEditing
		{ 
			get => (bool)GetValue(IsEditingProperty);
			internal set => SetValue(IsEditingPropertyKey, value);
		}

		private readonly TokenizedTagControl _parent;

		private bool _ignoreLostFocus = false;

		static TokenizedTagItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagItem), new FrameworkPropertyMetadata(typeof(TokenizedTagItem)));
		}

		public TokenizedTagItem(string text, TokenizedTagControl parent)
		{
			_parent = parent;
			this.Text = text;
		}

		public override void OnApplyTemplate()
		{
			if (this.GetTemplateChild("PART_InputBox") is AutoCompleteBox inputBox)
			{
				inputBox.LostKeyboardFocus += InputBox_LostFocus;
				inputBox.Loaded += InputBox_Loaded;
			}

			if (this.GetTemplateChild("PART_TagButton") is Button btn)
			{
				btn.Loaded += (s, e) =>
				{
					var b = (Button)s;
					if (b.Template.FindName("PART_DeleteTagButton", b) is Button btnDelete)
					{
						btnDelete.Click -= BtnDelete_Click;
						btnDelete.Click += BtnDelete_Click;
					}
				};

				btn.Click += (s, e) =>
				{
					if (_parent.IsSelectable && !_parent.IsReadonly) _parent.SelectedItem = this;
				};

				btn.MouseDoubleClick += (s, e) =>
				{
					_parent.RaiseTagDoubleClick(this);
					if (_parent.IsSelectable && !_parent.IsReadonly) _parent.SelectedItem = this;
				};
			}

			base.OnApplyTemplate();
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			var item = WPFHelper.GetParentOfType<TokenizedTagItem>(sender as FrameworkElement);
			if (item != null) _parent.RemoveTag(item);
			e.Handled = true;
		}

		private void InputBox_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is AutoCompleteBox acb)
			{
				if (acb.Template.FindName("Text", acb) is TextBox tb) tb.Focus();

				acb.PreviewKeyDown += (s, e1) =>
				{
					if (_parent.IsReadonly)
					{
						_parent.Focus();
						return;
					}

					if (e1.Key == Key.Enter)
					{
						if (!string.IsNullOrWhiteSpace(this.Text))
						{
							_parent.OnApplyTemplate(this);
							_parent.SelectedItem = _parent.InitializeNewTag();
						}
						else
						{
							_parent.Focus();
						}
					}
					else if (e1.Key == Key.Escape)
					{
						_parent.AbortEditing();
					}
					else if (e1.Key == Key.Back)
					{
						if (string.IsNullOrWhiteSpace(this.Text))
						{
							_ignoreLostFocus = true;
							InputBox_LostFocus(this, new RoutedEventArgs());
							_ignoreLostFocus = false;
							var previousTagIndex = ((IList)_parent.ItemsSource).Count - 1;
							if (previousTagIndex < 0) { _parent.AbortEditing(); return; }
								
							var previousTag = ((TokenizedTagItem) ((IList)_parent.ItemsSource)[previousTagIndex]);
							previousTag.Focus();
							previousTag.IsEditing = true;
						}
					}
				};
			}
		}
		
		private void InputBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(this.Text))
			{
				if (!((AutoCompleteBox) sender).IsDropDownOpen) this.IsEditing = false;
			}
			else
			{
				_parent.RemoveTag(this, true);
			}

			if (!_ignoreLostFocus) _parent.IsEditing = false;
		}
	}
}
