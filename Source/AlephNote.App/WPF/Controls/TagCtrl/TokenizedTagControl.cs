using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AlephNote.PluginInterface.Util;

namespace AlephNote.WPF.Controls
{
	[TemplatePart(Name = "PART_CreateTagButton", Type = typeof(Button))]
	public class TokenizedTagControl : ListBox, INotifyPropertyChanged
	{
		public event EventHandler<TokenizedTagEventArgs> TagClick;
		public event EventHandler<TokenizedTagEventArgs> TagAdded;
		public event EventHandler<TokenizedTagEventArgs> TagApplied;
		public event EventHandler<TokenizedTagEventArgs> TagRemoved;

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected virtual void OnExplicitPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		static TokenizedTagControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagControl), new FrameworkPropertyMetadata(typeof(TokenizedTagControl)));
		}

		//private TextBlock _placeholderTextBlock;

		public TokenizedTagControl()
		{
			if (this.ItemsSource == null) this.ItemsSource = new ObservableCollection<TokenizedTagItem>();
			if (this.AllTags == null) this.AllTags = new List<string>();
			if (this.EnteredTags == null) this.EnteredTags = new ObservableCollection<string>();

			this.LostKeyboardFocus += TokenizedTagControl_LostKeyboardFocus;
		}

		void TokenizedTagControl_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (!IsSelectable)
			{
				this.SelectedItem = null;
				return;
			}

			TokenizedTagItem itemToSelect = null;
			if (this.Items.Count > 0 && !object.ReferenceEquals((TokenizedTagItem)this.Items.CurrentItem, null))
			{
				if (this.SelectedItem != null && ((TokenizedTagItem) this.SelectedItem).Text != null && !((TokenizedTagItem) this.SelectedItem).IsEditing)
				{
					itemToSelect = (TokenizedTagItem) this.SelectedItem;
				}
				else if (!String.IsNullOrWhiteSpace(((TokenizedTagItem)this.Items.CurrentItem).Text))
				{
					itemToSelect = (TokenizedTagItem) this.Items.CurrentItem;
				}
			}
			
			// select the previous item
			if (!object.ReferenceEquals(itemToSelect, null))
			{
				e.Handled = true;
				RaiseTagApplied(itemToSelect);
				if (this.IsSelectable)
				{
					this.SelectedItem = itemToSelect;
				}
			}
		}

		public IList<string> EnteredTags
		{
			get => (IList<string>) GetValue(EnteredTagsProperty);
			set => SetValue(EnteredTagsProperty, value);
		}
		public static readonly DependencyProperty EnteredTagsProperty = 
			DependencyProperty.Register(
				"EnteredTags", 
				typeof(IList<string>), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(null, (d,e) => ((TokenizedTagControl)d).OnEnteredTagsChanged(e)));

		private void OnEnteredTagsChanged(DependencyPropertyChangedEventArgs e)
		{
			var vold = e.OldValue as INotifyCollectionChanged;
			var vnew = e.NewValue as INotifyCollectionChanged;

			if (vold != null) vold.CollectionChanged -= OnEnteredTagsCollectionChanged;
			if (vnew != null) vnew.CollectionChanged += OnEnteredTagsCollectionChanged;

			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();
			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(((IEnumerable<string>)e.NewValue) ?? new List<string>(), (s,t) => s==t.Text, s => new TokenizedTagItem(s));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
		}

		private void OnEnteredTagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{			
			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();

			var ni = EnteredTags?.ToList() ?? new List<string>();

			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(ni, (s,t) => s==t.Text, s => new TokenizedTagItem(s));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
		}

		private void UpdateEnteredTags()
		{
			var newvalue = (ItemsSource as IEnumerable<TokenizedTagItem>)?.Where(p => !p.IsEditing)?.Select(p => p.Text)?.ToList() ?? new List<string>();
			EnteredTags.SynchronizeCollection(newvalue);
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
		}

		private List<string> _allTags = new List<string>();
		public static readonly DependencyProperty AllTagsProperty = 
			DependencyProperty.Register(
				"AllTags", 
				typeof(List<string>), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(new List<string>()));

		public List<string> AllTags
		{
			get
			{
				if (!object.ReferenceEquals(this.ItemsSource, null))
				{
					var typedTags = ((IEnumerable<TokenizedTagItem>)ItemsSource).Select(i => i.Text).ToList();
					return _allTags.Except(typedTags).ToList();
				}
				return _allTags;
			}
			set
			{
				SetValue(AllTagsProperty, value);
				_allTags = value;
			}
		}

		public string Placeholder
		{
			get 
			{ 
				return (string)GetValue(PlaceholderProperty); 
			}
			set
			{
				SetValue(PlaceholderProperty, value);
			}
		}
		public static readonly DependencyProperty PlaceholderProperty = 
			DependencyProperty.Register(
				"Placeholder", 
				typeof(string), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata("Click here to enter tags..."));

		private void UpdateAllTagsProperty()
		{
			SetValue(AllTagsProperty, AllTags);
		}

		public static readonly DependencyProperty IsSelectableProperty = 
			DependencyProperty.Register(
				"IsSelectable", 
				typeof(bool), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(false));

		public bool IsSelectable { get { return (bool)GetValue(IsSelectableProperty); } set { SetValue(IsSelectableProperty, value); } }

		public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } internal set { SetValue(IsEditingPropertyKey, value); } }

		private static readonly DependencyPropertyKey IsEditingPropertyKey = 
			DependencyProperty.RegisterReadOnly(
				"IsEditing", 
				typeof(bool), 
				typeof(TokenizedTagControl), 
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

		public override void OnApplyTemplate()
		{
			this.OnApplyTemplate();
		}

		public void OnApplyTemplate(TokenizedTagItem appliedTag = null)
		{
			if (GetTemplateChild("PART_CreateTagButton") is Button createBtn)
			{
				createBtn.Click -= createBtn_Click;
				createBtn.Click += createBtn_Click;
			}

			base.OnApplyTemplate();

			if (appliedTag != null)
			{
				TagApplied?.Invoke(this, new TokenizedTagEventArgs(appliedTag));
			}
		}

		/// <summary>
		/// Executed when create new tag button is clicked.
		/// Adds an TokenizedTagItem to the collection and puts it in edit mode.
		/// </summary>
		void createBtn_Click(object sender, RoutedEventArgs e)
		{
			this.SelectedItem = InitializeNewTag();
		}

		internal TokenizedTagItem InitializeNewTag(bool suppressEditing = false)
		{
			var newItem = new TokenizedTagItem() { IsEditing = !suppressEditing };
			AddTag(newItem);
			UpdateAllTagsProperty();
			this.IsEditing = !suppressEditing;
			if (suppressEditing) UpdateEnteredTags();
			return newItem;
		}

		/// <summary>
		/// Adds a tag to the collection
		/// </summary>
		internal void AddTag(TokenizedTagItem tag)
		{
			TokenizedTagItem itemToSelect = null;
			if (this.SelectedItem == null && this.Items.Count > 0)
			{
				 itemToSelect = (TokenizedTagItem)this.SelectedItem;
			}
			((IList)this.ItemsSource).Add(tag); // assume IList for convenience
			this.Items.Refresh();

			// select the previous item
			if (!object.ReferenceEquals(itemToSelect, null))
			{
				RaiseTagClick(itemToSelect);
				if (this.IsSelectable)
					this.SelectedItem = itemToSelect;
			}

			TagAdded?.Invoke(this, new TokenizedTagEventArgs(tag));

			UpdateEnteredTags();
		}

		/// <summary>
		/// Removes a tag from the collection
		/// </summary>
		internal void RemoveTag(TokenizedTagItem tag, bool cancelEvent = false)
		{
			if (this.ItemsSource != null)
			{
				((IList)this.ItemsSource).Remove(tag); // assume IList for convenience
				this.Items.Refresh();

				if (TagRemoved != null && !cancelEvent)
				{
					TagRemoved(this, new TokenizedTagEventArgs(tag));
				}

				// select the last item
				if (this.SelectedItem == null && this.Items.Count > 0)
				{
					TokenizedTagItem itemToSelect = Items.GetItemAt(Items.Count - 1) as TokenizedTagItem;
					if (!object.ReferenceEquals(itemToSelect, null))
					{
						RaiseTagClick(itemToSelect);
						if (this.IsSelectable)
							this.SelectedItem = itemToSelect;
					}
				}
			}

			UpdateEnteredTags();
		}

		internal void RaiseTagClick(TokenizedTagItem tag)
		{
			TagClick?.Invoke(this, new TokenizedTagEventArgs(tag));
		}

		internal void RaiseTagApplied(TokenizedTagItem tag)
		{
			TagApplied?.Invoke(this, new TokenizedTagEventArgs(tag));

			UpdateEnteredTags();
		}
		/// <summary>
		/// Raises the TagDoubleClick event
		/// </summary>
		internal void RaiseTagDoubleClick(TokenizedTagItem tag)
		{
			UpdateAllTagsProperty();
			tag.IsEditing = true;
		}
	}

}
