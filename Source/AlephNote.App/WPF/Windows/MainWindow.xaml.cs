using AlephNote.WPF.Util;
using ScintillaNET;
using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Diagnostics;
using System.Windows.Controls;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Impl;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Controls;
using AlephNote.WPF.Shortcuts;
using AlephNote.WPF.MVVM;
using Hardcodet.Wpf.TaskbarNotification;
using AlephNote.WPF.Converter;
using Color = System.Drawing.Color;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using AlephNote.Common.Themes;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindow
	{
		public static MainWindow Instance { get; private set; }

		private readonly MainWindowViewmodel viewmodel;

		private readonly ScintillaHighlighter _highlighterDefault  = new DefaultHighlighter();
		private readonly ScintillaHighlighter _highlighterMarkdown = new MarkdownHighlighter();

		private readonly GlobalShortcutManager _scManager;
		private readonly bool _firstLaunch;

		private AlephTheme _currentTheme;

		public AppSettings Settings => viewmodel?.Settings;
		public MainWindowViewmodel VM => viewmodel;

		public INotesViewControl NotesViewControl { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			Instance = this;

			PluginManager.Inst.LoadPlugins(AppDomain.CurrentDomain.BaseDirectory);
			App.Themes.Init(AppDomain.CurrentDomain.BaseDirectory);

			_firstLaunch = false;
			AppSettings settings;
			try
			{
				if (File.Exists(App.PATH_SETTINGS))
				{
					settings = AppSettings.Load(App.PATH_SETTINGS);
					if (App.IsUpdateMigration)
					{
						settings.Migrate(App.UpdateMigrationFrom, App.UpdateMigrationTo);
						settings.Save();
					}
				}
				else
				{
					settings = AppSettings.CreateEmpty(App.PATH_SETTINGS);
					settings.Save();

					_firstLaunch = true;
				}
			}
			catch (Exception e)
			{
				ExceptionDialog.Show(null, "Could not load settings", "Could not load settings from " + App.PATH_SETTINGS, e);
				settings = AppSettings.CreateEmpty(App.PATH_SETTINGS);
			}

			_currentTheme = App.Themes.GetByFilename(settings.Theme, out var cte);
			if (_currentTheme == null)
			{
				App.Logger.ShowExceptionDialog($"Could not load theme {settings.Theme}", cte);
				_currentTheme = App.Themes.GetDefault();
			}
			else if (!_currentTheme.Compatibility.Includes(App.APP_VERSION))
			{
				App.Logger.ShowExceptionDialog($"Could not load theme {settings.Theme}\r\nThe theme does not support the current version of AlephNote ({App.APP_VERSION})", null);
				_currentTheme = App.Themes.GetDefault();
			}

			UpdateNotesViewComponent(settings);

			_scManager = new GlobalShortcutManager(this);

			StartupConfigWindow(settings);

			SetupScintilla(settings);

			viewmodel = new MainWindowViewmodel(settings, this);
			DataContext = viewmodel;

			FocusScintillaDelayed(250);
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			_scManager.OnSourceInitialized();
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
			//NoteEdit.SetWhitespaceForeColor(true, Color.FromArgb(255, 165, 0));
			NoteEdit.SetWhitespaceForeColor(true, Color.FromArgb(255, 195, 85));

			UpdateMargins(s);
			NoteEdit.BorderStyle = BorderStyle.FixedSingle;

			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_OFF].DefineRgbaImage(Properties.Resources.ui_off);

			NoteEdit.Markers[ScintillaHighlighter.STYLE_MARKER_LIST_ON].DefineRgbaImage(Properties.Resources.ui_on);

			NoteEdit.MultipleSelection = s.SciRectSelection;
			NoteEdit.MouseSelectionRectangularSwitch = s.SciRectSelection;
			NoteEdit.AdditionalSelectionTyping = s.SciRectSelection;
			NoteEdit.VirtualSpaceOptions = s.SciRectSelection ? VirtualSpace.RectangularSelection : VirtualSpace.None;
			NoteEdit.EndAtLastLine = !s.SciScrollAfterLastLine;

			var fnt = string.IsNullOrWhiteSpace(s.NoteFontFamily) ? FontNameToFontFamily.StrDefaultValue : s.NoteFontFamily;
			NoteEdit.Font = new Font(fnt, (int)s.NoteFontSize);

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

		public void ScrollScintilla(int? v)
		{
			if (v == null) return;

			NoteEdit.FirstVisibleLine = v.Value;
		}

		public void FocusScintillaDelayed(int d = 50)
		{
			new Thread(() => { Thread.Sleep(d); System.Windows.Application.Current.Dispatcher.Invoke(FocusScintilla); }).Start();
		}

		public void ExecuteDelayed(int d, Action a)
		{
			new Thread(() => { Thread.Sleep(d); System.Windows.Application.Current.Dispatcher.BeginInvoke(a); }).Start();
		}

		public void FocusScintilla()
		{
			NoteEditHost.Focus();
			Keyboard.Focus(NoteEditHost);
			NoteEdit.Focus();
		}

		public void FocusGlobalSearch()
		{
			GlobalSearchBar.Focus();
			Keyboard.Focus(GlobalSearchBar);
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
							return;
						}
					}
				}
			}

			if (e.Key == Key.System && ReferenceEquals(e.OriginalSource, NoteEditHost))
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

			if (_firstLaunch)
			{
				var fsw = new FirstStartupWindow(this);
				fsw.ShowDialog();
			}

			if (Settings.RememberScroll) VM.ForceUpdateUIScroll();

			UpdateShortcuts(Settings);
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

		public void MainWindow_OnClosed(EventArgs args)
		{
			_scManager.Close();
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
			var modShift = e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift);
			var modCtrl  = e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)  || e.KeyboardDevice.IsKeyDown(Key.RightCtrl);
			var modAlt   = e.KeyboardDevice.IsKeyDown(Key.LeftAlt)   || e.KeyboardDevice.IsKeyDown(Key.RightAlt);

			if (e.Key >= Key.A && e.Key <= Key.Z && !modShift && !modCtrl && !modAlt)
			{
				e.Handled = true; // important, otherwise the normal listbox behaviour is executed after this

				char chr = (char)('A' + (e.Key - Key.A));
			
				bool found = false;
				if (viewmodel.SelectedNote == null) found = true;

				var visible = NotesViewControl.EnumerateVisibleNotes().ToList();

				foreach (var note in Enumerable.Concat(visible, visible))
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
			NotesViewControl.SetShortcuts(this, settings.Shortcuts.ToList());

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
			if (e.Change == UpdateChange.VScroll)
			{
				VM.OnScroll(NoteEdit.FirstVisibleLine);
			}
		}

		private void VertGridSplitterChanged(object sender, EventArgs e)
		{
			VM.GridSplitterChanged();
		}

		public void UpdateNotesViewComponent(AppSettings settings)
		{
			var templ = (ControlTemplate) NotesViewCtrlWrapper.Resources[(settings.UseHierachicalNoteStructure) ? "TemplateHierachical" : "TemplateFlat"];

			var ctrl = templ.LoadContent();

			NotesViewCtrlWrapper.Content = ctrl;

			NotesViewControl = (INotesViewControl) ctrl;
		}

		public void ShowMoveFolderPopup()
		{
			if (VM.SelectedNote == null) return;

			FolderPopupListView.Items.Clear();

			foreach (var folder in NotesViewControl.ListFolder()) FolderPopupListView.Items.Add(folder);

			FolderPopupListView.SelectedItem = VM.SelectedNote.Path;

			FolderPopup.IsOpen = true;
		}

		private void ButtonMoveFolder_OnClick(object sender, RoutedEventArgs e)
		{
			if (VM.SelectedNote == null) return;

			var newPath = FolderPopupListView.SelectedItem as DirectoryPath;
			if (newPath == null) return;

			if (VM.SelectedNote.Path.EqualsWithCase(newPath)) return;

			VM.SelectedNote.Path = newPath;
			VM.SelectedFolderPath = newPath;

			FolderPopup.IsOpen = false;
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
	}
}
