using ScintillaNET;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Threading;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Util;

namespace AlephNote.WPF.Controls
{
	/// <summary>
	/// Interaction logic for SearchBox.xaml
	/// </summary>
	public partial class SearchBox : UserControl
	{
		public static readonly DependencyProperty SearchTextProperty =
			DependencyProperty.Register(
			"SearchText",
			typeof(string),
			typeof(SearchBox),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o,a) => { ((SearchBox)o).OnSearchTexChanged(); }));
		
		public static readonly DependencyProperty CaseSensitiveProperty =
			DependencyProperty.Register(
			"CaseSensitive",
			typeof(bool),
			typeof(SearchBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty WholeWordProperty =
			DependencyProperty.Register(
			"WholeWord",
			typeof(bool),
			typeof(SearchBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty RegexProperty =
			DependencyProperty.Register(
			"Regex",
			typeof(bool),
			typeof(SearchBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty TargetProperty =
			DependencyProperty.Register(
			"Target",
			typeof(Scintilla),
			typeof(SearchBox));

		public static readonly DependencyProperty SettingsProperty =
			DependencyProperty.Register(
			"Settings",
			typeof(AppSettings),
			typeof(SearchBox));

		public event EventHandler HideBox;

		private const int NUM = ScintillaHighlighter.INDICATOR_INLINE_SEARCH;

		public string      SearchText    { get { return (string)     GetValue(SearchTextProperty);    } set { SetValue(SearchTextProperty,    value); } }
		public bool        CaseSensitive { get { return (bool)       GetValue(CaseSensitiveProperty); } set { SetValue(CaseSensitiveProperty, value); } }
		public bool        WholeWord     { get { return (bool)       GetValue(WholeWordProperty);     } set { SetValue(WholeWordProperty,     value); } }
		public bool        Regex         { get { return (bool)       GetValue(RegexProperty);         } set { SetValue(RegexProperty,         value); } }
		public Scintilla   Target        { get { return (Scintilla)  GetValue(TargetProperty);        } set { SetValue(TargetProperty,        value); } }
		public AppSettings Settings      { get { return (AppSettings)GetValue(SettingsProperty);      } set { SetValue(SettingsProperty,      value); } }

		public SearchBox()
		{
			InitializeComponent();
			MainGrid.DataContext = this;
			Visibility = Visibility.Collapsed;
		}

		private void ExecSearch()
		{
			var scintilla = Target;
			var text = SearchText;
			if (scintilla == null) return;
			if (text == "") { ClearSearch(); return; }

			scintilla.IndicatorCurrent = NUM;
			scintilla.IndicatorClearRange(0, scintilla.TextLength);

			scintilla.TargetStart = 0;
			scintilla.TargetEnd = scintilla.TextLength;
			scintilla.SearchFlags = SearchFlags.None;
			if (CaseSensitive) scintilla.SearchFlags |= SearchFlags.MatchCase;
			if (WholeWord) scintilla.SearchFlags |= SearchFlags.WholeWord;
			if (Regex) scintilla.SearchFlags |= ConvertRegexFlags(Settings.DocSearchRegexEngine);

			bool first = true;
			while (scintilla.SearchInTarget(text) != -1)
			{
				scintilla.IndicatorFillRange(scintilla.TargetStart, scintilla.TargetEnd - scintilla.TargetStart);

				scintilla.TargetStart = scintilla.TargetEnd;
				scintilla.TargetEnd = scintilla.TextLength;

				if (first)
				{
					scintilla.ScrollRange(scintilla.TargetStart, scintilla.TargetEnd);
					first = false;
				}
			}
		}

		private SearchFlags ConvertRegexFlags(SciRegexEngine f)
		{
			switch (f)
			{
				case SciRegexEngine.Default: return SearchFlags.Regex;
				case SciRegexEngine.CPlusPlus: return SearchFlags.Regex | SearchFlags.Cxx11Regex;
				case SciRegexEngine.Posix: return SearchFlags.Regex | SearchFlags.Posix;
			}

			throw new Exception("Invalid SciRegexEngine: " + f);
		}

		private void ClearSearch()
		{
			var scintilla = Target;
			if (scintilla == null) return;
			
			scintilla.IndicatorCurrent = NUM;
			scintilla.IndicatorClearRange(0, scintilla.TextLength);
		}

		public void Hide()
		{
			if (Visibility == Visibility.Collapsed) return;

			Visibility = Visibility.Collapsed;
			ClearSearch();
			HideBox?.Invoke(this, new EventArgs());
		}

		public void Show()
		{
			if (!Settings.DocSearchEnabled) return;

			if (Visibility == Visibility.Visible) { FocusToTextbox();  return; }

			SearchText = "";
			if (Settings.DocSearchCaseSensitive) CaseSensitive = true;
			if (Settings.DocSearchWholeWord) WholeWord = true;
			if (Settings.DocSearchRegex) Regex = true;

			Visibility = Visibility.Visible;
			FocusToTextbox();
		}

		private void OnCloseBox(object sender, RoutedEventArgs e)
		{
			Hide();
		}

		private void OnClickCaseSensitive(object sender, RoutedEventArgs e)
		{
			CaseSensitive = !CaseSensitive;
			ExecSearch();
		}

		private void OnClickWholeWord(object sender, RoutedEventArgs e)
		{
			WholeWord = !WholeWord;
			ExecSearch();
		}

		private void OnClickCaseRegex(object sender, RoutedEventArgs e)
		{
			Regex = !Regex;
			ExecSearch();
		}

		private void OnSearch(object sender, RoutedEventArgs e)
		{
			ExecSearch();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				ExecSearch();
				e.Handled = true;
			}
			else if (e.Key == Key.Escape)
			{
				Hide();
				e.Handled = true;
			}
		}

		private void OnSearchTexChanged()
		{
			if (Settings.DocSearchLiveSearch) ExecSearch();
		}

		public void FocusToTextbox()
		{
			Focus();
			Keyboard.Focus(MainTextBox);
			MainTextBox.Focus();

			new Thread(() => 
			{
				Thread.Sleep(50);
				Application.Current.Dispatcher.Invoke(() =>
				{
					Focus();
					Keyboard.Focus(MainTextBox);
					MainTextBox.Focus();
				});
				Thread.Sleep(100);
				Application.Current.Dispatcher.Invoke(() =>
				{
					Focus();
					Keyboard.Focus(MainTextBox);
					MainTextBox.Focus();
				});
			}).Start();
		}
	}
}
