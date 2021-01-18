using AlephNote.WPF.Util;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Interop;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Shortcuts;
using AlephNote.WPF.Controls;
using MSHC.WPF.MVVM;
using Hardcodet.Wpf.TaskbarNotification;
using AlephNote.WPF.Converter;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using AlephNote.Common.Themes;
using AlephNote.WPF.Extensions;
using AlephNote.Common.Util;
using AlephNote.Common.Util.Search;
using AlephNote.Native;
using AlephNote.WPF.Controls.NotesView;
using AlephNote.WPF.ScintillaUtil;
using MSHC.Lang.Collections;
using Application = System.Windows.Application;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindow : IShortcutHandlerParent
	{
		public static MainWindow Instance { get; private set; }

		private readonly MainWindowViewmodel _viewmodel;

		private readonly ScintillaHighlighter _highlighterDefault  = new DefaultHighlighter();
		private readonly ScintillaHighlighter _highlighterMarkdown = new MarkdownHighlighter();

		private readonly GlobalShortcutManager _scManager;

		public AppSettings Settings => _viewmodel?.Settings;
		public MainWindowViewmodel VM => _viewmodel;

		public INotesViewControl NotesViewControl { get; private set; }

		public bool IsClosed = false;

		public MainWindow(AppSettings settings)
		{
			InitializeComponent();
			Instance = this;
			
			UpdateNotesViewComponent(settings);

			_scManager = new GlobalShortcutManager(this);

			StartupConfigWindow(settings);

			SetupScintilla(settings);

			_viewmodel = new MainWindowViewmodel(settings, this);
			_viewmodel.ManuallyTriggerSelectedNoteChanged();

			DataContext = _viewmodel;

			FocusScintillaDelayed(250);
		}

		public IShortcutHandler GetShortcutHandler() => _viewmodel;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			_scManager.OnSourceInitialized();

			if (PresentationSource.FromVisual(this) is HwndSource source) source.AddHook(WndProc);

			CaptureMouseWheelWhenUnfocusedBehavior.SetIsEnabled(NoteEditHost, Settings.FixScintillaScrollMessages);
		}
		
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == NativeMethods.WM_SHOWME) VM.ShowMainWindow();

			return IntPtr.Zero;
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

			if (s.MarkdownMode == MarkdownHighlightMode.WithTag && _viewmodel?.SelectedNote?.HasTagCaseInsensitive(AppSettings.TAG_MARKDOWN) == true)
				return _highlighterMarkdown;

			return _highlighterDefault;
		}

		public void SetupScintilla(AppSettings s)
		{
			var theme = ThemeManager.Inst.CurrentThemeSet;

			NoteEdit.Lexer = Lexer.Container;

			NoteEdit.CaretForeColor          = theme.Get<ColorRef>("scintilla.caret:foreground").ToDCol();
			NoteEdit.CaretLineBackColor      = theme.Get<ColorRef>("scintilla.caret:background").ToDCol();
			NoteEdit.CaretLineBackColorAlpha = theme.Get<int>("scintilla.caret:background_alpha");
			NoteEdit.CaretLineVisible        = theme.Get<bool>("scintilla.caret:visible");

			NoteEdit.SetSelectionForeColor(theme.Get<bool>("scintilla.selection:override_foreground"), theme.Get<ColorRef>("scintilla.selection:foreground").ToDCol());
			NoteEdit.SetSelectionBackColor(true, theme.Get<ColorRef>("scintilla.selection:background").ToDCol()); // https://sourceforge.net/p/scintilla/bugs/538/

			NoteEdit.WhitespaceSize = theme.Get<int>("scintilla.whitespace:size");
			NoteEdit.ViewWhitespace = s.SciShowWhitespace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
			NoteEdit.ViewEol        = s.SciShowEOL;
			NoteEdit.SetWhitespaceForeColor(!theme.Get<ColorRef>("scintilla.whitespace:color").IsTransparent, theme.Get<ColorRef>("scintilla.whitespace:color").ToDCol());
			NoteEdit.SetWhitespaceBackColor(!theme.Get<ColorRef>("scintilla.whitespace:background").IsTransparent, theme.Get<ColorRef>("scintilla.whitespace:background").ToDCol());

			UpdateMargins(s);
			NoteEdit.BorderStyle = BorderStyle.None;
			
			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_ON ].DefineRgbaImage(theme.GetDBitmapResource("margin_check_on.png" ));
			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_OFF].DefineRgbaImage(theme.GetDBitmapResource("margin_check_off.png"));
			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_MIX].DefineRgbaImage(theme.GetDBitmapResource("margin_check_mix.png"));


			NoteEdit.MultipleSelection = s.SciMultiSelection;
			NoteEdit.MouseSelectionRectangularSwitch = s.SciRectSelection;
			NoteEdit.AdditionalSelectionTyping = s.SciRectSelection;
			NoteEdit.VirtualSpaceOptions = s.SciRectSelection ? VirtualSpace.RectangularSelection : VirtualSpace.None;
			NoteEdit.EndAtLastLine = !s.SciScrollAfterLastLine;

			var fnt = string.IsNullOrWhiteSpace(s.NoteFontFamily) ? FontNameToFontFamily.StrDefaultValue : s.NoteFontFamily;
			NoteEdit.Font = new Font(fnt, (int)s.NoteFontSize);

			_highlighterDefault.SetUpStyles(NoteEdit, s);

			NoteEdit.WrapMode = s.SciWordWrap ? WrapMode.Whitespace : WrapMode.None;

			NoteEdit.HScrollBar = theme.Get<bool>("scintilla.scrollbar_h:visible");
			NoteEdit.VScrollBar = theme.Get<bool>("scintilla.scrollbar_v:visible");

			NoteEdit.ZoomChanged -= ZoomChanged;
			NoteEdit.ZoomChanged += ZoomChanged;

			NoteEdit.UseTabs = s.SciUseTabs;
			NoteEdit.TabWidth = s.SciTabWidth * 2;

			NoteEdit.ReadOnly = s.IsReadOnlyMode || VM?.SelectedNote?.IsLocked==true;

			NoteEdit.ClearCmdKey(Keys.Control | Keys.D); // SCI_SELECTIONDUPLICATE

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
				(Settings.ListMode == ListHighlightMode.WithTag && _viewmodel?.SelectedNote?.HasTagCaseInsensitive(AppSettings.TAG_LIST) == true);

			var startPos = NoteEdit.GetEndStyled();
			var endPos = e.Position;

			GetHighlighter(Settings).Highlight(NoteEdit, startPos, endPos, Settings);
			if (listHighlight) GetHighlighter(Settings).UpdateListMargin(NoteEdit, startPos, endPos);
		}

		private void NoteEdit_HotspotClick(object sender, HotspotClickEventArgs e)
		{
			var settings = VM?.Settings;
			if (settings == null) return;

			if (Settings.LinkMode == LinkHighlightMode.SingleClick)
			{
				var link = GetHighlighter(settings).GetClickedLink(NoteEdit.Text, e.Position);
				if (link != null) OpenLink(link);
			}
			else if (Settings.LinkMode == LinkHighlightMode.ControlClick && e.Modifiers.HasFlag(Keys.Control))
			{
				var link = GetHighlighter(settings).GetClickedLink(NoteEdit.Text, e.Position);
				if (link != null) OpenLink(link);
			}
		}

		private void NoteEdit_HotspotDoubleClick(object sender, HotspotClickEventArgs e)
		{
			var settings = VM?.Settings;
			if (settings == null) return;

			if (Settings.LinkMode == LinkHighlightMode.DoubleClick)
			{
				var link = GetHighlighter(settings).GetClickedLink(NoteEdit.Text, e.Position);
				if (link != null) OpenLink(link);
			}
		}

		private void OpenLink(string link)
		{
			if (link.ToLower().StartsWith("note://"))
			{
				var n = VM.Repository.FindNoteByID(link.Substring("note://".Length));
				if (n != null)
					VM.SelectedNote = n;
				else
					System.Windows.MessageBox.Show("Note not found");
			}
			else if (link.ToLower().StartsWith("file://"))
			{
				Process.Start(link);
			}
			else if (ScintillaHighlighter.IsRawMail(link))
			{
				Process.Start("mailto://" + link);
			}
			else
			{
				Process.Start(link);
			}
		}

		private void ZoomChanged(object sender, EventArgs args)
		{
			if (Settings.SciZoomable)
			{
				Settings.SciZoom = NoteEdit.Zoom;
				_viewmodel.RequestSettingsSave();
			}
			else
			{
				if (NoteEdit.Zoom != 0)
				{
					NoteEdit.Zoom = 0;
					if (Settings.SciZoom != 0)
					{
						Settings.SciZoom = NoteEdit.Zoom;
						_viewmodel.RequestSettingsSave();
					}
				}
			}

			UpdateMargins(Settings);
			UpdateCustomLineNumbers(0);
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

			var theme = ThemeManager.Inst.CurrentThemeSet;

			var listHighlight = 
				(s.ListMode == ListHighlightMode.Always) || 
				(s.ListMode == ListHighlightMode.WithTag && _viewmodel?.SelectedNote?.HasTagCaseInsensitive(AppSettings.TAG_LIST) == true);

			NoteEdit.Margins.ClearAllText();

			if (s.IsCustomLineNumbers() && s.SciHexLineNumber)
			{
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Type = MarginType.RightText;
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Width = NoteEdit.TextWidth(ScintillaHighlighter.STYLE_DEFAULT, "0xAAAA");
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].BackColor = theme.Get<ColorRef>("scintilla.margin.numbers:background").ToDCol();
			}
			else if (s.IsCustomLineNumbers())
			{
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Type = MarginType.RightText;
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Width = NoteEdit.TextWidth(ScintillaHighlighter.STYLE_DEFAULT, "5555");
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].BackColor = theme.Get<ColorRef>("scintilla.margin.numbers:background").ToDCol();
			}
			else
			{
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Type = MarginType.Number;
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].Width = s.SciLineNumbers ? NoteEdit.TextWidth(ScintillaHighlighter.STYLE_DEFAULT, "5555") : 0;
				NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LINENUMBERS].BackColor = theme.Get<ColorRef>("scintilla.margin.numbers:background").ToDCol();
			}

			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].Width = listHighlight ? (NoteEdit.Lines.FirstOrDefault()?.Height ?? 32) : 0;
			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].Mask = Marker.MaskAll;
			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].Sensitive = true;
			NoteEdit.Margins[ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS].BackColor = theme.Get<ColorRef>("scintilla.margin.symbols:background").ToDCol();

			NoteEdit.Margins[2].Width = 0;

			NoteEdit.Margins[3].Width = 0;

			if (listHighlight && _viewmodel?.SelectedNote != null) GetHighlighter(s).UpdateListMargin(NoteEdit, null, null);
		}

		public void UpdateCustomLineNumbers(int startingAtLine)
		{
			if (Settings == null) return;
			if (!Settings.IsCustomLineNumbers()) return;

			// Starting at the specified line index, update each
			// subsequent line margin text with a hex line number.
			for (var i = startingAtLine; i < NoteEdit.Lines.Count; i++)
			{
				NoteEdit.Lines[i].MarginStyle = ScintillaNET.Style.LineNumber;

				if (i % Settings.SciLineNumberSpacing == 0)
				{
					if (Settings.SciHexLineNumber)
						NoteEdit.Lines[i].MarginText = "0x" + i.ToString("X2");
					else
						NoteEdit.Lines[i].MarginText = (i+1).ToString();
				}
				else
				{
					NoteEdit.Lines[i].MarginText = string.Empty;
				}
			}
		}

		public void ScrollScintilla(int? v)
		{
			if (v == null) return;

			NoteEdit.FirstVisibleLine = v.Value;
		}

		public void ScrollScintilla(Tuple<int, int?> v)
		{
			if (v == null) return;

			NoteEdit.FirstVisibleLine = v.Item1;
			if (v.Item2 != null)
			{
				NoteEdit.CurrentPosition = v.Item2.Value;
				NoteEdit.SelectionStart  = NoteEdit.CurrentPosition;
				NoteEdit.SelectionEnd    = NoteEdit.CurrentPosition;
			}
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

		public void FocusGlobalSearch()
		{
			if (Settings.HideSearchBox) return;
			GlobalSearchBar.Focus();
		}
		
		public void FocusTitle()
		{
			if (!TitleTextBox.IsReadOnly) TitleTextBox.Focus();
		}
		
		public void FocusTagEditor()
		{
			if (!TagEditor.Readonly)
			{
				TagEditor.StartEditing();
			}
		}
		
		private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			var evtkey = e.Key;
			var evtmod = e.KeyboardDevice.Modifiers;
			if (evtkey == Key.System) evtkey = e.SystemKey;

			if (Settings?.AutoHideMainMenu == true && (evtkey == Key.LeftAlt || evtkey == Key.RightAlt) && !e.IsRepeat && evtmod == ModifierKeys.Alt)
			{
				VM.MenuIsVisible = !VM.MenuIsVisible;
			}
		}

		private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
		{
			var evtkey = e.Key;
			var evtmod = e.KeyboardDevice.Modifiers;
			if (evtkey == Key.System) evtkey = e.SystemKey;

			if (Settings != null && ReferenceEquals(e.OriginalSource, NoteEditHost))
			{
				var exec = false;
				foreach (var sc in Settings.Shortcuts)
				{
					if (sc.Value.Scope == AlephShortcutScope.NoteEdit)
					{
						var kk = (Key)sc.Value.Key;
						var mm = (ModifierKeys)sc.Value.Modifiers;
						if (kk == evtkey && mm == evtmod)
						{
							ShortcutManager.Execute(this, sc.Key);
							e.Handled = true;
							exec = true;
						}
					}
				}
				if (exec) return;
			}

			if (e.Key == Key.System && ReferenceEquals(e.OriginalSource, NoteEditHost) && Settings?.SciRectSelection==true)
			{
				// Prevent ALT key removing focus of control
				e.Handled = true;
				return;
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
						if (kk == ekey && mm == emod)
						{
							ShortcutManager.Execute(this, sc.Key);
							e.Handled = true;
						}
					}
				}

				if (Settings.VSLineCopy && emod == ModifierKeys.Control && ekey == Key.C)
				{
					_viewmodel.CopyAllowLineCommand.Execute(null);
					e.Handled = true;
				}

				if (Settings.VSLineCopy && emod == ModifierKeys.Control && ekey == Key.X)
				{
					_viewmodel.CutAllowLineCommand.Execute(null);
					e.Handled = true;
				}
			}
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			ResetScintillaScrollAndUndo();

			if (App.IsFirstLaunch)
			{
				var fsw = new FirstStartupWindow(this);
				fsw.ShowDialog();
			}

			if (Settings.RememberScroll) VM.ForceUpdateUIScroll();

			UpdateShortcuts(Settings);
		}

		public void ShowDocSearchBar()
		{
			_viewmodel.SearchText = string.Empty;
			DocumentSearchBar.Show();
		}

		public IDisposable PreventScintillaFocus()
		{
			return VM.PreventScintillaFocusLock.Set();
		}

		public void HideDocSearchBar()
		{
			DocumentSearchBar.Hide();
		}

		public void MainWindow_OnClosed(EventArgs args)
		{
			IsClosed = true;

			_scManager.Close();

			App.Current.Shutdown();
		}

		private void OnHideDoumentSearchBox(object sender, EventArgs e)
		{
			FocusScintilla();
		}

		private void TagEditor_OnChanged(ITagEditor source)
		{
			Application.Current.Dispatcher?.BeginInvoke(new Action(() =>
			{
				ForceNewHighlighting(Settings);
				UpdateMargins(Settings);
				UpdateCustomLineNumbers(0);
			}));
		}

		private void NotesList_Drop(object sender, System.Windows.DragEventArgs e)
		{
			_viewmodel.OnNewNoteDrop(e.Data);
		}

		private void NotesList_KeyDown(object sender, KeyEventArgs e)
		{
			var modShift = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);
			var modCtrl  = e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)  || e.KeyboardDevice.IsKeyDown(Key.RightCtrl);
			var modAlt   = e.KeyboardDevice.IsKeyDown(Key.LeftAlt)   || e.KeyboardDevice.IsKeyDown(Key.RightAlt);

			if (e.Key >= Key.A && e.Key <= Key.Z && !modShift && !modCtrl && !modAlt)
			{
				e.Handled = true; // important, otherwise the normal listbox behaviour is executed after this

				char chr = (char)('A' + (e.Key - Key.A));
			
				bool found = false;
				if (_viewmodel.SelectedNote == null) found = true;

				var visible = NotesViewControl.EnumerateVisibleNotes().ToList();

				foreach (var note in Enumerable.Concat(visible, visible))
				{
					if (found)
					{
						if (note.Title.ToUpper().StartsWith(chr.ToString()))
						{
							_viewmodel.SetSelectedNoteWithoutFocus(note);
							return;
						}
					}
					else
					{
						found = (note.UniqueName== _viewmodel.SelectedNote.UniqueName);
					}
				}
			}
		}

		private void NoteEdit_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (_viewmodel?.SelectedNote == null) return;
			
			if (e.Margin == ScintillaHighlighter.STYLE_MARGIN_LISTSYMBOLS)
			{
				bool listHighlight =
					(Settings.ListMode == ListHighlightMode.Always) ||
					(Settings.ListMode == ListHighlightMode.WithTag && _viewmodel?.SelectedNote?.HasTagCaseInsensitive(AppSettings.TAG_LIST) == true);

				if (listHighlight)
				{
					var line = NoteEdit.Lines[NoteEdit.LineFromPosition(e.Position)];
					var mark = line.MarkerGet();

					if ((mark & (1 << ScintillaHighlighter.STYLE_MARKER_LIST_ON)) != 0)
					{
						var mrk = _highlighterDefault.FindListMarkerChar(NoteEdit.Lines, ListHighlightValue.FALSE);
						var newText = _highlighterDefault.ChangeListLine(line.Text, mrk ?? ' ');

						NoteEdit.TargetStart = line.Position;
						NoteEdit.TargetEnd = line.EndPosition;
						NoteEdit.ReplaceTarget(newText);
					}
					else if ((mark & (1 << ScintillaHighlighter.STYLE_MARKER_LIST_OFF)) != 0)
					{
						if (e.Modifiers.HasFlag(Keys.Control))
						{
							var mrk = _highlighterDefault.FindListMarkerChar(NoteEdit.Lines, ListHighlightValue.INTERMED);
							var newText = _highlighterDefault.ChangeListLine(line.Text, mrk ?? '~');

							NoteEdit.TargetStart = line.Position;
							NoteEdit.TargetEnd = line.EndPosition;
							NoteEdit.ReplaceTarget(newText);
						}
						else
						{
							var mrk = _highlighterDefault.FindListMarkerChar(NoteEdit.Lines, ListHighlightValue.TRUE);
							var newText = _highlighterDefault.ChangeListLine(line.Text, mrk ?? 'X');

							NoteEdit.TargetStart = line.Position;
							NoteEdit.TargetEnd = line.EndPosition;
							NoteEdit.ReplaceTarget(newText);
						}
					}
					else if ((mark & (1 << ScintillaHighlighter.STYLE_MARKER_LIST_MIX)) != 0)
					{
						var mrk = _highlighterDefault.FindListMarkerChar(NoteEdit.Lines, ListHighlightValue.TRUE);
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
			// ================ WINDOW ================
			InputBindings.Clear();
			foreach (var sc in settings.Shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.Window))
			{
				var sckey = sc.Key;
				var cmd = new RelayCommand(() => ShortcutManager.Execute(this, sckey));
				var ges = new KeyGesture((Key)sc.Value.Key, (ModifierKeys)sc.Value.Modifiers);
				InputBindings.Add(new InputBinding(cmd, ges));
			}

			// ================ NOTESLIST ================
			NotesViewControl.SetShortcuts(this, settings.Shortcuts.ToList()); // via InputBindings

			// ================ GLOBAL ================
			_scManager.Clear();
			foreach (var sc in settings.Shortcuts.Where(s => s.Value.Scope == AlephShortcutScope.Global))
			{
				_scManager.Register(sc.Value.Modifiers, sc.Value.Key, sc.Key);
			}
		}
		
		private void TrayIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
		{
			var ti = sender as TaskbarIcon;
			if (ti == null) return;

			var cm = ti.ContextMenu;
			if (cm == null) return;

			foreach (var aami in cm.Items.OfType<AutoActionMenuItem>())
			{
				aami.RecursiveRefresh();
			}
		}

		private void NoteEdit_UpdateUI(object sender, UpdateUIEventArgs e)
		{
			if (e.Change == UpdateChange.VScroll || e.Change == UpdateChange.Selection)
			{
				VM.OnScroll(NoteEdit.FirstVisibleLine, NoteEdit.CurrentPosition);
			}
		}

		private void VertGridSplitterChanged(object sender, EventArgs e)
		{
			VM.GridSplitterChanged();
		}

		public void UpdateNotesViewComponent(AppSettings settings)
		{
			var templ = (ControlTemplate) NotesViewCtrlWrapper.Resources[(settings.UseHierarchicalNoteStructure) ? "TemplateHierarchical" : "TemplateFlat"];

			var ctrl = templ.LoadContent();

			NotesViewCtrlWrapper.Content = ctrl;

			NotesViewControl = (INotesViewControl) ctrl;
		}

		private void NoteEdit_OnMouseWheel(object sender, MouseEventArgs e)
		{
			if (Settings?.FixScintillaScrollMessages != true) return;
			// Windows Forms dows not "scroll under cursor" but "scroll where focus is"
			// To have the same behaviour with the other controls we emulate WPF behaviour 
			// for the scintilla control

			var handled = NotesViewControl.ExternalScrollEmulation(e.Delta);

			if (handled && e is HandledMouseEventArgs e2) e2.Handled = true;
		}
		
		private void CDC_DoChangeAccount(object sender, ConnectionDisplayControl.AccountChangeEventArgs e)
		{
			VM.ChangeAccount(e.AccountID);
		}

		private void ImageLock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			VM.ChangeSettingReadonlyMode();
		}

		private void NoteEdit_OnBeforeTextSet()
		{
			if (NoteEdit != null && NoteEdit.ReadOnly) NoteEdit.ReadOnly = false;
		}

		private void NoteEdit_OnAfterTextSet()
		{
			if (NoteEdit != null)
			{
				NoteEdit.ReadOnly = (Settings?.IsReadOnlyMode==true) || (VM?.SelectedNote?.IsLocked == true);
			}
		}

		public void ShowTagFilter()
		{
			List<CheckableTag> tags = null;

			void Update(string name, bool check)
			{
				VM.SearchText = string.Join(" ", tags.Where(t => t.Checked).Select(p => (p.Additional=="notag") ? SearchStringParser.TAG_SEARCH_EMPTY : SearchStringParser.GetTagSearch(p.Name)));
			}

			tags = VM.Repository.EnumerateAllTags().Distinct().OrderBy(p=>p.ToLower()).Select(p => new CheckableTag(p, Update)).ToList();

			if (tags.Count==0) return; // no tags

			tags.Insert(0, new CheckableTag("(No tags)", Update, "notag"));

			var pst = SearchStringParser.Parse(VM.SearchText);

			foreach (var tm in pst.GetPossibleMatchedTags())
			{
				foreach (var t in tags.Where(p => p.Name.ToLower()==tm)) t.Checked=true;
			}
			if (pst.IsMatchingNoTag())
			{
				foreach (var t in tags.Where(p => p.Additional=="notag")) t.Checked=true;
			}

			TagPopupList.ItemsSource = tags;
			
			TagPopup.IsOpen=true;

			Update(null, true);
		}

		private void NoteEdit_OnInsert(object sender, ModificationEventArgs e)
		{
			if (Settings.IsCustomLineNumbers() && e.LinesAdded != 0) UpdateCustomLineNumbers(NoteEdit.LineFromPosition(e.Position));
		}

		private void NoteEdit_OnDelete(object sender, ModificationEventArgs e)
		{
			if (Settings.IsCustomLineNumbers() && e.LinesAdded != 0) UpdateCustomLineNumbers(NoteEdit.LineFromPosition(e.Position));
		}

		public void SetFocus(FocusTarget dst)
		{
			switch (dst)
			{
				case FocusTarget.NoteTitle:
					FocusTitle();
					break;

				case FocusTarget.NoteTags:
					FocusTagEditor();
					break;

				case FocusTarget.NoteText:
					FocusScintilla();
					break;

				case FocusTarget.NoteList:
					NotesViewControl.FocusNotesList();
					break;

				case FocusTarget.FolderList:
					NotesViewControl.FocusFolderList();
					break;
				
				case FocusTarget.Unchanged:
				default:
					return; // Nothing
			}
		}
	}
}
