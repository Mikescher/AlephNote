using AlephNote.Plugins;
using AlephNote.Settings;
using AlephNote.WPF.Util;
using ScintillaNET;
using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Controls;
using AlephNote.PluginInterface;
using AlephNote.WPF.Shortcuts;
using AlephNote.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewmodel viewmodel;

		private readonly ScintillaHighlighter _highlighterDefault  = new DefaultHighlighter();
		private readonly ScintillaHighlighter _highlighterMarkdown = new MarkdownHighlighter();

		public AppSettings Settings => viewmodel?.Settings;

		public MainWindowViewmodel VM => viewmodel;

		public MainWindow()
		{
			InitializeComponent();

			PluginManager.Inst.LoadPlugins(AppDomain.CurrentDomain.BaseDirectory, App.Logger);

			bool firstLaunch = false;
			AppSettings settings;
			try
			{
				if (File.Exists(App.PATH_SETTINGS))
				{
					settings = AppSettings.Load(App.PATH_SETTINGS);
				}
				else
				{
					settings = AppSettings.CreateEmpty(App.PATH_SETTINGS);
					settings.Save();

					firstLaunch = true;
				}
			}
			catch (Exception e)
			{
				ExceptionDialog.Show(null, "Could not load settings", "Could not load settings from " + App.PATH_SETTINGS, e);
				settings = AppSettings.CreateEmpty(App.PATH_SETTINGS);
			}

			StartupConfigWindow(settings);

			SetupScintilla(settings);
			UpdateShortcuts(settings);

			viewmodel = new MainWindowViewmodel(settings, this);
			DataContext = viewmodel;

			if (firstLaunch)
			{
				MessageBox.Show(
					this, 
					"It looks like you are starting AlephNote for the first time." + Environment.NewLine +
					"You should start by looking into the settings and configuring a remote where your notes are stored." + Environment.NewLine +
					"1. From the Edit menu, select Settings" + Environment.NewLine +
					"2. Click the '+' symbol at top-right of screen" + Environment.NewLine +
					"3. Choose the provider of your choice (eg SimpleNote)" + Environment.NewLine +
					"4. Enter in login and password data" + Environment.NewLine +
					"5. Press [OK]", 
					"First launch", 
					MessageBoxButton.OK, 
					MessageBoxImage.Information);
			}

			FocusScintillaDelayed(250);
		}

		private void StartupConfigWindow(AppSettings settings)
		{
			if (settings.StartupLocation == ExtendedWindowStartupLocation.CenterScreen)
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
				WindowState = ConvertWindowStateEnum(settings.StartupState);

				Left = settings.StartupPositionX;
				Top = settings.StartupPositionY;

				Width = settings.StartupPositionWidth;
				Height = settings.StartupPositionHeight;
			}
			else if (settings.StartupLocation == ExtendedWindowStartupLocation.Manual)
			{
				WindowStartupLocation = WindowStartupLocation.Manual;
				WindowState = ConvertWindowStateEnum(settings.StartupState);

				Left = settings.StartupPositionX;
				Top = settings.StartupPositionY;

				Width = settings.StartupPositionWidth;
				Height = settings.StartupPositionHeight;
			}
			else if (settings.StartupLocation == ExtendedWindowStartupLocation.ScreenBottomLeft)
			{
				var screen = WpfScreen.GetScreenFrom(this);

				WindowStartupLocation = WindowStartupLocation.Manual;
				WindowState = ConvertWindowStateEnum(settings.StartupState);

				Left = screen.WorkingArea.Left + 5;
				Top = screen.WorkingArea.Bottom - settings.StartupPositionHeight - 5;

				Width = settings.StartupPositionWidth;
				Height = settings.StartupPositionHeight;
			}
			else if (settings.StartupLocation == ExtendedWindowStartupLocation.ScreenLeft)
			{
				var screen = WpfScreen.GetScreenFrom(this);

				WindowStartupLocation = WindowStartupLocation.Manual;
				WindowState = ConvertWindowStateEnum(settings.StartupState);

				Left = screen.WorkingArea.Left + 5;
				Top = screen.WorkingArea.Top + 5;

				Width = settings.StartupPositionWidth;
				Height = screen.WorkingArea.Height - 10;
			}

			if (settings.MinimizeToTray && settings.StartupState == ExtendedWindowState.Minimized)
				Hide();
		}

		private WindowState ConvertWindowStateEnum(ExtendedWindowState s)
		{
			switch (s)
			{
				case ExtendedWindowState.Minimized: return WindowState.Minimized;
				case ExtendedWindowState.Maximized: return WindowState.Maximized;
				case ExtendedWindowState.Normal:    return WindowState.Normal;
			}

			throw new ArgumentException(s + " is not a valid ExtendedWindowState");
		}

		public ScintillaHighlighter GetHighlighter(AppSettings s)
		{
			if (s.MarkdownMode == MarkdownHighlightMode.Always)
				return _highlighterMarkdown;

			if (s.MarkdownMode == MarkdownHighlightMode.WithTag && viewmodel?.SelectedNote?.HasTagCasInsensitive(AppSettings.TAG_MARKDOWN) == true)
				return _highlighterMarkdown;

			return _highlighterDefault;
		}

		public void SetupScintilla(AppSettings s)
		{
			NoteEdit.Lexer = Lexer.Container;

			NoteEdit.WhitespaceSize = 1;
			NoteEdit.ViewWhitespace = s.SciShowWhitespace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
			NoteEdit.SetWhitespaceForeColor(true, Color.Orange);

			UpdateMargins(s);
			NoteEdit.BorderStyle = BorderStyle.FixedSingle;

			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_OFF].DefineRgbaImage(Properties.Resources.ui_off);

			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_ON].DefineRgbaImage(Properties.Resources.ui_on);

			NoteEdit.MultipleSelection = s.SciRectSelection;
			NoteEdit.MouseSelectionRectangularSwitch = s.SciRectSelection;
			NoteEdit.AdditionalSelectionTyping = s.SciRectSelection;
			NoteEdit.VirtualSpaceOptions = s.SciRectSelection ? VirtualSpace.RectangularSelection : VirtualSpace.None;

			NoteEdit.Font = new Font(s.NoteFontFamily, (int)s.NoteFontSize);

			_highlighterDefault.SetUpStyles(NoteEdit, s);

			NoteEdit.WrapMode = s.SciWordWrap ? WrapMode.Whitespace : WrapMode.None;

			NoteEdit.ZoomChanged -= ZoomChanged;
			NoteEdit.ZoomChanged += ZoomChanged;

			NoteEdit.UseTabs = s.SciUseTabs;
			NoteEdit.TabWidth = s.SciTabWidth * 2;

			ResetScintillaScrollAndUndo();

			ForceNewHighlighting(s);
		}

		private void ForceNewHighlighting(AppSettings s)
		{
			GetHighlighter(s).Highlight(NoteEdit, 0, NoteEdit.Text.Length, s); // evtl only re-highlight visible text?
		}

		private void NoteEdit_StyleNeeded(object sender, StyleNeededEventArgs e)
		{
			bool listHighlight =
				(Settings.ListMode == ListHighlightMode.Always) ||
				(Settings.ListMode == ListHighlightMode.WithTag && viewmodel?.SelectedNote?.HasTagCasInsensitive(AppSettings.TAG_LIST) == true);

			var startPos = NoteEdit.GetEndStyled();
			var endPos = e.Position;

			GetHighlighter(Settings).Highlight(NoteEdit, startPos, endPos, Settings);
			if (listHighlight) GetHighlighter(Settings).UpdateListMargin(NoteEdit, startPos, endPos);
		}

		private void NoteEdit_HotspotClick(object sender, HotspotClickEventArgs e)
		{
			if (Settings.LinkMode == LinkHighlightMode.SingleClick)
			{
				var links = _highlighterDefault.FindAllLinks(NoteEdit);
				var link = links.FirstOrDefault(l => l.Item2 <= e.Position && e.Position <= l.Item3);
				if (link != null) Process.Start(link.Item1);
			}
			else if (Settings.LinkMode == LinkHighlightMode.ControlClick && e.Modifiers.HasFlag(Keys.Control))
			{
				var links = _highlighterDefault.FindAllLinks(NoteEdit);
				var link = links.FirstOrDefault(l => l.Item2 <= e.Position && e.Position <= l.Item3);
				if (link != null) Process.Start(link.Item1);
			}
		}

		private void NoteEdit_HotspotDoubleClick(object sender, HotspotClickEventArgs e)
		{
			if (Settings.LinkMode == LinkHighlightMode.DoubleClick)
			{
				var links = _highlighterDefault.FindAllLinks(NoteEdit);
				var link = links.FirstOrDefault(l => l.Item2 <= e.Position && e.Position <= l.Item3);
				if (link != null) Process.Start(link.Item1);
			}
		}

		private void ZoomChanged(object sender, EventArgs args)
		{
			if (Settings.SciZoomable)
			{
				Settings.SciZoom = NoteEdit.Zoom;
				viewmodel.RequestSettingsSave();
			}
			else
			{
				if (NoteEdit.Zoom != 0)
				{
					NoteEdit.Zoom = 0;
					if (Settings.SciZoom != 0)
					{
						Settings.SciZoom = NoteEdit.Zoom;
						viewmodel.RequestSettingsSave();
					}
				}
			}

			UpdateMargins(Settings);
		}

		public void ResetScintillaScrollAndUndo()
		{
			NoteEdit.ScrollWidth = 1;
			NoteEdit.ScrollWidthTracking = true;
			NoteEdit.EmptyUndoBuffer();
		}

		public void UpdateMargins(AppSettings s)
		{
			if (s == null) return;

			bool listHighlight = 
				(s.ListMode == ListHighlightMode.Always) || 
				(s.ListMode == ListHighlightMode.WithTag && viewmodel?.SelectedNote?.HasTagCasInsensitive(AppSettings.TAG_LIST) == true);

			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Width = s.SciLineNumbers ? NoteEdit.TextWidth(ScintillaHighlighter.STYLE_DEFAULT, "5555") : 0;

			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].Width = listHighlight ? (NoteEdit.Lines.FirstOrDefault()?.Height ?? 32) : 0;
			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].Mask = Marker.MaskAll;
			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].Sensitive = true;

			NoteEdit.Margins[2].Width = 0;

			NoteEdit.Margins[3].Width = 0;

			if (listHighlight && viewmodel?.SelectedNote != null) GetHighlighter(s).UpdateListMargin(NoteEdit, null, null);
		}

		public void FocusScintillaDelayed(int d = 50)
		{
			new Thread(() => { Thread.Sleep(d); System.Windows.Application.Current.Dispatcher.Invoke(FocusScintilla); }).Start();
		}

		public void FocusScintilla()
		{
			NoteEditHost.Focus();
			Keyboard.Focus(NoteEditHost);
			NoteEdit.Focus();
		}

		private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (Settings != null && ReferenceEquals(e.OriginalSource, NoteEditHost))
			{
				foreach (var sc in Settings.Shortcuts)
				{
					if (sc.Value.Scope == AlephShortcutScope.NoteEdit)
					{
						var kk = (Key)sc.Value.Key;
						var mm = (ModifierKeys)sc.Value.Modifiers;
						if (kk == e.Key && mm == (e.KeyboardDevice.Modifiers & mm))
						{
							ShortcutManager.Execute(this, sc.Key);
							e.Handled = true;
						}
					}
				}
			}
			else if (e.Key == Key.System && ReferenceEquals(e.OriginalSource, NoteEditHost))
			{
				// Prevent ALT key removing focus of control
				e.Handled = true;
			}
		}

		private void NoteEdit_OnKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar < 32)
			{
				// Prevent control characters from getting inserted into the text buffer
				e.Handled = true;
				return;
			}
		}

		private void NoteEdit_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (Settings != null)
			{
				var ekey = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
				var emod = ModifierKeys.None;
				if (e.Control) emod |= ModifierKeys.Control;
				if (e.Alt) emod |= ModifierKeys.Alt;
				if (e.Shift) emod |= ModifierKeys.Shift;

				foreach (var sc in Settings.Shortcuts)
				{
					if (sc.Value.Scope == AlephShortcutScope.NoteEdit || sc.Value.Scope == AlephShortcutScope.Window)
					{
						var kk = (Key)sc.Value.Key;
						var mm = (ModifierKeys)sc.Value.Modifiers;
						if (kk == ekey && mm == (emod & mm))
						{
							ShortcutManager.Execute(this, sc.Key);
							e.Handled = true;
						}
					}
				}
			}
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			ResetScintillaScrollAndUndo();
		}

		public void ShowDocSearchBar()
		{
			viewmodel.SearchText = string.Empty;
			DocumentSearchBar.Show();
		}

		public void HideDocSearchBar()
		{
			DocumentSearchBar.Hide();
		}

		private void OnHideDoumentSearchBox(object sender, EventArgs e)
		{
			FocusScintilla();
		}

		private void TagEditor_OnChanged(TagEditor source)
		{
			ForceNewHighlighting(Settings);
			UpdateMargins(Settings);
		}

		private void NotesList_Drop(object sender, System.Windows.DragEventArgs e)
		{
			viewmodel.OnNewNoteDrop(e.Data);
		}

		private void NotesList_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key >= Key.A && e.Key <= Key.Z)
			{
				char chr = (char)('A' + (e.Key - Key.A));

				bool found = false;
				if (viewmodel.SelectedNote == null) found = true;
				foreach (var note in Enumerable.Concat(viewmodel.NotesView.OfType<INote>(), viewmodel.NotesView.OfType<INote>()))
				{
					if (found)
					{
						if (note.Title.ToUpper().StartsWith(chr.ToString()))
						{
							viewmodel.SetSelectedNoteWithoutFocus(note);
							return;
						}
					}
					else
					{
						found = (note.GetUniqueName() == viewmodel.SelectedNote.GetUniqueName());
					}
				}
			}
		}

		private void NoteEdit_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (viewmodel?.SelectedNote == null) return;
			
			if (e.Margin == ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS)
			{
				bool listHighlight =
					(Settings.ListMode == ListHighlightMode.Always) ||
					(Settings.ListMode == ListHighlightMode.WithTag && viewmodel?.SelectedNote?.HasTagCasInsensitive(AppSettings.TAG_LIST) == true);

				if (listHighlight)
				{
					var line = NoteEdit.Lines[NoteEdit.LineFromPosition(e.Position)];
					var mark = line.MarkerGet();

					if ((mark & (1 << ScintillaHighlighter.STYLE_MARKER_LIST_ON)) != 0)
					{
						var newText = _highlighterDefault.ChangeListLine(line.Text, ' ');

						NoteEdit.TargetStart = line.Position;
						NoteEdit.TargetEnd = line.EndPosition;
						NoteEdit.ReplaceTarget(newText);
					}
					else if ((mark & (1 << ScintillaHighlighter.STYLE_MARKER_LIST_OFF)) != 0)
					{
						var mrk = _highlighterDefault.FindListerOnMarker(NoteEdit.Lines);
						var newText = _highlighterDefault.ChangeListLine(line.Text, mrk ?? 'X');

						NoteEdit.TargetStart = line.Position;
						NoteEdit.TargetEnd = line.EndPosition;
						NoteEdit.ReplaceTarget(newText);
					}
				}
			}
		}

		public void UpdateShortcuts(AppSettings settings)
		{
			InputBindings.Clear();
			NotesList.InputBindings.Clear();

			foreach (var sc in settings.Shortcuts)
			{
				if (sc.Value.Scope == AlephShortcutScope.Window)
				{
					var sckey = sc.Key;
					var cmd = new RelayCommand(() => ShortcutManager.Execute(this, sckey));
					var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
					InputBindings.Add(new InputBinding(cmd, ges));
				}
				else if (sc.Value.Scope == AlephShortcutScope.NoteList)
				{
					var sckey = sc.Key;
					var cmd = new RelayCommand(() => ShortcutManager.Execute(this, sckey));
					var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
					NotesList.InputBindings.Add(new InputBinding(cmd, ges));
				}
			}
		}
	}
}
