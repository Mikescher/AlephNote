using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AlephNote.Common.Settings.Types;
using MSHC.Util.Threads;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for GlobalSearchTextBox.xaml
	/// </summary>
	public partial class GlobalSearchTextBox : UserControl
	{
		public static readonly DependencyProperty SearchTextInternalProperty =
			DependencyProperty.Register(
				"SearchTextInternal",
				typeof(string),
				typeof(GlobalSearchTextBox),
				new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o,e) => ((GlobalSearchTextBox)o).SearchTextInternalChanged()));
		
		public string SearchTextInternal
		{
			get { return (string)GetValue(SearchTextInternalProperty); }
			set { SetValue(SearchTextInternalProperty, value); }
		}

		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(GlobalSearchTextBox),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o,e) => ((GlobalSearchTextBox)o).SearchTextChanged()));
		
		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}
		
		public static readonly DependencyProperty SearchDelayModeProperty =
			DependencyProperty.Register(
				"SearchDelayMode",
				typeof(SearchDelayMode),
				typeof(GlobalSearchTextBox));

		public SearchDelayMode SearchDelayMode
		{
			get { return (SearchDelayMode)GetValue(SearchDelayModeProperty); }
			set { SetValue(SearchDelayModeProperty, value); }
		}
		
		public static readonly DependencyProperty NodeCountProperty =
			DependencyProperty.Register(
				"NodeCount",
				typeof(int),
				typeof(GlobalSearchTextBox),
				new FrameworkPropertyMetadata(0));

		public int NodeCount
		{
			get { return (int)GetValue(NodeCountProperty); }
			set { SetValue(NodeCountProperty, value); }
		}
		
		private readonly DelayedCombiningInvoker _invSearch;

		public GlobalSearchTextBox()
		{
			_invSearch = DelayedCombiningInvoker.CreateHighspeed(() => Application.Current.Dispatcher.BeginInvoke(new Action(ApplyInternalSearchText)), 500, 60_000);

			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		private SearchDelayMode GetActualSearchDelayMode()
		{
			if (SearchDelayMode == SearchDelayMode.Auto)
			{
				return NodeCount>200 ? SearchDelayMode.Delayed : SearchDelayMode.Direct;
			}
			return SearchDelayMode;
		}

		public new void Focus()
		{
			SearchControl.Focus();
			Keyboard.Focus(SearchControl);
		}

		private void Button_Clear_Click(object sender, RoutedEventArgs e)
		{
			SearchText = string.Empty;
		}

		private void SearchTextInternalChanged()
		{
			var sdm = GetActualSearchDelayMode();
			if (sdm == SearchDelayMode.Direct) ApplyInternalSearchText();
			if (sdm == SearchDelayMode.Delayed) _invSearch.Request();
		}
		
		private void SearchTextChanged()
		{
			_invSearch.CancelPendingRequests();
			SearchTextInternal = SearchText;
		}

		private void ApplyInternalSearchText()
		{
			if (SearchTextInternal != SearchText) SearchText = SearchTextInternal;
		}

		private void SearchControl_OnKeyDown(object sender, KeyEventArgs e)
		{
			var sdm = GetActualSearchDelayMode();
			if (e.Key == Key.Enter && (sdm == SearchDelayMode.Delayed || sdm == SearchDelayMode.Manual))
			{
				ApplyInternalSearchText();
			}
		}
	}
}
