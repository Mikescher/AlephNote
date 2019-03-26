using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AlephNote.PluginInterface.Datatypes;
using AlephNote.PluginInterface.Util;
using MSHC.Lang.Collections;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// https://github.com/niieani/TokenizedInputCs
	/// https://stackoverflow.com/a/15314094/1761622
	/// </summary>
	[TemplatePart(Name = "PART_CreateTagButton", Type = typeof(Button))]
	public class TokenizedTagControl : ListBox, INotifyPropertyChanged
	{
		public event EventHandler<TokenizedTagEventArgs> TagListChanged;

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
				typeof(TagList), 
				typeof(TokenizedTagControl), 
				new PropertyMetadata(null, (d,e) => ((TokenizedTagControl)d).OnEnteredTagsChanged(e)));
		
		// Entered Tags is the collection that is watched to the outside world
		// In TagEditor2.xaml we bind TagSource to EnteredTags TwoWay
		// This collection is Updated in UpdateEnteredTags()
		// This is _NOT_ what is actually visible _INSIDE_ the control - that is the ItemsSource collection
		// Can be NULL (?)
		public TagList EnteredTags
		{
			get => (TagList) GetValue(EnteredTagsProperty);
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
				new FrameworkPropertyMetadata(false, (d,e) => ((TokenizedTagControl)d).IsEditingChanged(e)));

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

		private int _suppressItemsRefresh = 0;

		private List<string> _tagsBeforeEdit = null;

		static TokenizedTagControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagControl), new FrameworkPropertyMetadata(typeof(TokenizedTagControl)));
		}

		public TokenizedTagControl()
		{
			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();
			if (EnteredTags == null) EnteredTags = new VoidTagList();

			LostKeyboardFocus += TokenizedTagControl_LostKeyboardFocus;
		}

		private void TokenizedTagControl_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (!IsSelectable)
			{
				SelectedItem = null;
				return;
			}

			TokenizedTagItem itemToSelect = null;
			if (Items.Count > 0 && !object.ReferenceEquals((TokenizedTagItem)Items.CurrentItem, null))
			{
				if (((TokenizedTagItem) SelectedItem)?.Text != null && !((TokenizedTagItem) SelectedItem).IsEditing)
				{
					itemToSelect = (TokenizedTagItem) SelectedItem;
				}
				else if (!string.IsNullOrWhiteSpace(((TokenizedTagItem)Items.CurrentItem).Text))
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

		public void FullResyncWithDataSource()
		{
			if (ItemsSource == null) return;

			if (IsEditing)
			{
				try
				{
					_suppressItemsRefresh++;
				
					var rm = ((IList<TokenizedTagItem>)ItemsSource).FirstOrDefault(p => p.IsEditing);
					if (rm != null) rm.IsEditing = false;
					Items.Refresh();
				}
				finally
				{
					_suppressItemsRefresh--;
				}
			}

			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(((IEnumerable<string>)EnteredTags) ?? new List<string>(), (s,t) => s==t.Text, s => new TokenizedTagItem(s, this));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
		}

		private void OnEnteredTagsChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is TagList vold) vold.OnChanged -= OnEnteredTagsCollectionChanged;
			if (e.NewValue is TagList vnew) vnew.OnChanged += OnEnteredTagsCollectionChanged;

			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();

			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(((IEnumerable<string>)e.NewValue) ?? new List<string>(), (s,t) => s==t.Text, s => new TokenizedTagItem(s, this));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
			
			if (IsEditing) AbortEditing();
		}

		private void OnEnteredTagsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_suppressItemsRefresh>0) return;

			if (ItemsSource == null) ItemsSource = new ObservableCollection<TokenizedTagItem>();

			var ni = EnteredTags?.ToList() ?? new List<string>();

			((IList<TokenizedTagItem>)ItemsSource).SynchronizeCollection(ni, (s,t) => s==t.Text, s => new TokenizedTagItem(s, this));
			OnExplicitPropertyChanged("FormattedText");
			Items.Refresh();
			
			if (IsEditing) AbortEditing();
		}

		public void AbortEditing()
		{
			try
			{
				_suppressItemsRefresh++;
				
				var rm = ((IList<TokenizedTagItem>)ItemsSource).FirstOrDefault(p => p.IsEditing);
				if (rm != null) ((IList<TokenizedTagItem>)ItemsSource).Remove(rm);
				Items.Refresh();
				IsEditing = false;
			}
			finally
			{
				_suppressItemsRefresh--;
			}
		}

		private void UpdateEnteredTags()
		{
			try
			{
				_suppressItemsRefresh++;
				
				var newvalue = (ItemsSource as IEnumerable<TokenizedTagItem>)?.Where(p => !p.IsEditing).Select(p => p.Text).ToList() ?? new List<string>();
				EnteredTags?.SynchronizeCollection(newvalue);
				OnExplicitPropertyChanged("FormattedText");
			}
			finally
			{
				_suppressItemsRefresh--;
			}
		}

		public override void OnApplyTemplate()
		{
			OnApplyTemplate(null);
		}

		public void OnApplyTemplate(TokenizedTagItem appliedTag)
		{
			if (GetTemplateChild("PART_CreateTagButton") is Button createBtn)
			{
				createBtn.Click -= CreateBtn_Click;
				createBtn.Click += CreateBtn_Click;
			}

			base.OnApplyTemplate();
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

		private void AddTag(TokenizedTagItem tag)
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
				if (IsSelectable) SelectedItem = itemToSelect;
			}

			UpdateEnteredTags();
		}

		private bool _lockRemoveTag = false;
		public void RemoveTag(TokenizedTagItem tag, bool cancelEvent = false)
		{
			if (IsReadonly) return;
			if (_lockRemoveTag) return;

			if (ItemsSource != null)
			{
				try
				{
					_lockRemoveTag = true;
					((IList)ItemsSource).Remove(tag);
				}
				finally
				{
					_lockRemoveTag = false;
				}
				
				Items.Refresh();

				if (SelectedItem == null && Items.Count > 0)
				{
					TokenizedTagItem itemToSelect = Items.GetItemAt(Items.Count - 1) as TokenizedTagItem;
					if (!(itemToSelect is null))
					{
						if (IsSelectable) SelectedItem = itemToSelect;
					}
				}
			}

			UpdateEnteredTags();
		}

		public void RaiseTagApplied(TokenizedTagItem tag)
		{
			UpdateEnteredTags();
		}

		public void RaiseTagDoubleClick(TokenizedTagItem tag)
		{
			if (IsReadonly) return;
			tag.IsEditing = true;
		}
		
		private void IsEditingChanged(DependencyPropertyChangedEventArgs e)
		{
			if (IsEditing)
			{
				_tagsBeforeEdit = EnteredTags?.ToList() ?? new List<string>();
			}
			else
			{
				if (_tagsBeforeEdit == null) return;

				var tbe = _tagsBeforeEdit;
				_tagsBeforeEdit = null;

				Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
				{
					if (!tbe.UnorderedCollectionEquals(EnteredTags?.ToList() ?? new List<string>())) TagListChanged?.Invoke(this, new TokenizedTagEventArgs(null));
				}));
			}
			
		}

	}

}
