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
	/// <summary>
	/// https://github.com/niieani/TokenizedInputCs
	/// https://stackoverflow.com/a/15314094/1761622
	/// </summary>
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

		public static readonly DependencyProperty DropDownTagsProperty = 
			DependencyProperty.Register(
				"DropDownTags", 
				typeof(List<string>), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(new List<string>()));
		
		public List<string> DropDownTags
		{
			get => (List<string>) GetValue(DropDownTagsProperty);
			set => SetValue(DropDownTagsProperty, value);
		}
		
		public static readonly DependencyProperty EnteredTagsProperty = 
			DependencyProperty.Register(
				"EnteredTags", 
				typeof(IList<string>), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(null, (d,e) => ((TokenizedTagControl)d).OnEnteredTagsChanged(e)));
		
		public IList<string> EnteredTags
		{
			get => (IList<string>) GetValue(EnteredTagsProperty);
			set => SetValue(EnteredTagsProperty, value);
		}

		public static readonly DependencyProperty PlaceholderProperty = 
			DependencyProperty.Register(
				"Placeholder", 
				typeof(string), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata("Click here to enter tags..."));
		
		public string Placeholder
		{
			get => (string)GetValue(PlaceholderProperty); 
			set => SetValue(PlaceholderProperty, value);
		}
		
		public static readonly DependencyProperty IsSelectableProperty = 
			DependencyProperty.Register(
				"IsSelectable", 
				typeof(bool), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(false));
		
		public bool IsSelectable 
		{ 
			get => (bool)GetValue(IsSelectableProperty);
			set => SetValue(IsSelectableProperty, value);
		}
		
		private static readonly DependencyPropertyKey IsEditingPropertyKey = 
			DependencyProperty.RegisterReadOnly(
				"IsEditing", 
				typeof(bool), 
				typeof(TokenizedTagControl), 
				new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;
		
		public bool IsEditing 
		{
			get => (bool)GetValue(IsEditingProperty);
			internal set => SetValue(IsEditingPropertyKey, value);
		}

		public static readonly DependencyProperty IsReadonlyProperty = 
			DependencyProperty.Register(
				"IsReadonly", 
				typeof(bool), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(false));
		
		public bool IsReadonly
		{
			get => (bool)GetValue(IsReadonlyProperty); 
			set => SetValue(IsReadonlyProperty, value);
		}

		private int suppressItemsRefresh = 0;

		static TokenizedTagControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagControl), new FrameworkPropertyMetadata(typeof(TokenizedTagControl)));
		}

		public TokenizedTagControl()
		{
			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();
			if (EnteredTags == null) EnteredTags = new ObservableCollection<string>();

			LostKeyboardFocus += TokenizedTagControl_LostKeyboardFocus;
		}

		void TokenizedTagControl_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (!IsSelectable)
			{
				SelectedItem = null;
				return;
			}

			TokenizedTagItem itemToSelect = null;
			if (Items.Count > 0 && !object.ReferenceEquals((TokenizedTagItem)Items.CurrentItem, null))
			{
				if (SelectedItem != null && ((TokenizedTagItem) SelectedItem).Text != null && !((TokenizedTagItem) SelectedItem).IsEditing)
				{
					itemToSelect = (TokenizedTagItem) SelectedItem;
				}
				else if (!String.IsNullOrWhiteSpace(((TokenizedTagItem)Items.CurrentItem).Text))
				{
					itemToSelect = (TokenizedTagItem) Items.CurrentItem;
				}
			}
			
			if (!(itemToSelect is null))
			{
				e.Handled = true;
				RaiseTagApplied(itemToSelect);
				if (IsSelectable) SelectedItem = itemToSelect;
			}
		}

		private void OnEnteredTagsChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is INotifyCollectionChanged vold) vold.CollectionChanged -= OnEnteredTagsCollectionChanged;
			if (e.NewValue is INotifyCollectionChanged vnew) vnew.CollectionChanged += OnEnteredTagsCollectionChanged;

			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();
			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(((IEnumerable<string>)e.NewValue) ?? new List<string>(), (s,t) => s==t.Text, s => new TokenizedTagItem(s, this));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
		}

		private void OnEnteredTagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (suppressItemsRefresh>0) return;

			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();

			var ni = EnteredTags?.ToList() ?? new List<string>();

			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(ni, (s,t) => s==t.Text, s => new TokenizedTagItem(s, this));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
		}

		private void UpdateEnteredTags()
		{
			try
			{
				suppressItemsRefresh++;
				
				var newvalue = (ItemsSource as IEnumerable<TokenizedTagItem>)?.Where(p => !p.IsEditing)?.Select(p => p.Text)?.ToList() ?? new List<string>();
				EnteredTags.SynchronizeCollection(newvalue);
				OnExplicitPropertyChanged("FormattedText");
			}
			finally
			{
				suppressItemsRefresh--;
			}
		}

		public override void OnApplyTemplate()
		{
			OnApplyTemplate(null);
		}

		public void OnApplyTemplate(TokenizedTagItem appliedTag = null)
		{
			if (GetTemplateChild("PART_CreateTagButton") is Button createBtn)
			{
				createBtn.Click -= CreateBtn_Click;
				createBtn.Click += CreateBtn_Click;
			}

			base.OnApplyTemplate();

			if (appliedTag != null)
			{
				TagApplied?.Invoke(this, new TokenizedTagEventArgs(appliedTag));
			}
		}

		private void CreateBtn_Click(object sender, RoutedEventArgs e)
		{
			if (IsReadonly) return;
			this.SelectedItem = InitializeNewTag();
		}

		public TokenizedTagItem InitializeNewTag(bool suppressEditing = false)
		{
			var newItem = new TokenizedTagItem(string.Empty, this) { IsEditing = !suppressEditing };
			AddTag(newItem);
			IsEditing = !suppressEditing;
			if (suppressEditing) UpdateEnteredTags();
			return newItem;
		}

		public void AddTag(TokenizedTagItem tag)
		{
			if (IsReadonly) return;

			TokenizedTagItem itemToSelect = null;
			if (SelectedItem == null && Items.Count > 0)
			{
				 itemToSelect = (TokenizedTagItem)SelectedItem;
			}

			((IList)ItemsSource).Add(tag);
			Items.Refresh();

			if (!(itemToSelect is null))
			{
				RaiseTagClick(itemToSelect);
				if (IsSelectable) SelectedItem = itemToSelect;
			}

			TagAdded?.Invoke(this, new TokenizedTagEventArgs(tag));

			UpdateEnteredTags();
		}

		public void RemoveTag(TokenizedTagItem tag, bool cancelEvent = false)
		{
			if (IsReadonly) return;

			if (ItemsSource != null)
			{
				((IList)ItemsSource).Remove(tag);
				Items.Refresh();

				if (TagRemoved != null && !cancelEvent) TagRemoved(this, new TokenizedTagEventArgs(tag));

				if (SelectedItem == null && Items.Count > 0)
				{
					TokenizedTagItem itemToSelect = Items.GetItemAt(Items.Count - 1) as TokenizedTagItem;
					if (!(itemToSelect is null))
					{
						RaiseTagClick(itemToSelect);
						if (IsSelectable) SelectedItem = itemToSelect;
					}
				}
			}

			UpdateEnteredTags();
		}

		public void RaiseTagClick(TokenizedTagItem tag)
		{
			TagClick?.Invoke(this, new TokenizedTagEventArgs(tag));
		}

		public void RaiseTagApplied(TokenizedTagItem tag)
		{
			TagApplied?.Invoke(this, new TokenizedTagEventArgs(tag));
			UpdateEnteredTags();
		}

		public void RaiseTagDoubleClick(TokenizedTagItem tag)
		{
			tag.IsEditing = true;
		}
	}

}
