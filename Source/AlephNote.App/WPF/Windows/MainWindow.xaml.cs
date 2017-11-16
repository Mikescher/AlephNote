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

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewmodel viewmodel;

		public AppSettings Settings => viewmodel.Settings;

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

			viewmodel = new MainWindowViewmodel(settings, this);
			DataContext = viewmodel;

			if (firstLaunch)
			{
				MessageBox.Show(
					this, 
					"It looks like you are starting AlephNote for the first time." + Environment.NewLine + 
					"You should start by looking into the settings and configuring a remote where your notes are stored." + Environment.NewLine + 
					"Or you can use this program in headless mode where the notes exist only local.", 
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

		public void SetupScintilla(AppSettings s)
		{
			NoteEdit.Lexer = Lexer.Container;

			NoteEdit.WhitespaceSize = 1;
			NoteEdit.ViewWhitespace = s.SciShowWhitespace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
			NoteEdit.SetWhitespaceForeColor(true, Color.Orange);

			NoteEdit.Margins[0].Width = s.SciLineNumbers ? 32 : 0;
			NoteEdit.Margins[1].Width = 0;
			NoteEdit.Margins[2].Width = 0;
			NoteEdit.Margins[3].Width = 0;
			NoteEdit.BorderStyle = BorderStyle.FixedSingle;

			NoteEdit.MultipleSelection = s.SciRectSelection;
			NoteEdit.MouseSelectionRectangularSwitch = s.SciRectSelection;
			NoteEdit.AdditionalSelectionTyping = s.SciRectSelection;
			NoteEdit.VirtualSpaceOptions = s.SciRectSelection ? VirtualSpace.RectangularSelection : VirtualSpace.None;

			NoteEdit.Font = new Font(s.NoteFontFamily, (int)s.NoteFontSize);

			DefaultHighlighter.SetUpStyles(NoteEdit, s);

			NoteEdit.WrapMode = s.SciWordWrap ? WrapMode.Whitespace : WrapMode.None;

			NoteEdit.ZoomChanged -= ZoomChanged;
			NoteEdit.ZoomChanged += ZoomChanged;

			NoteEdit.UseTabs = s.SciUseTabs;
			NoteEdit.TabWidth = s.SciTabWidth * 2;

			ResetScintillaScrollAndUndo();

			var showLinks = (s.LinkMode != LinkHighlightMode.Disabled);
			DefaultHighlighter.Highlight(NoteEdit, 0, NoteEdit.Text.Length, showLinks); // evtl only re-highlight visible text?
		}

		private void NoteEdit_StyleNeeded(object sender, StyleNeededEventArgs e)
		{
			var startPos = NoteEdit.GetEndStyled();
			var endPos = e.Position;

			var showLinks = (Settings.LinkMode != LinkHighlightMode.Disabled);

			DefaultHighlighter.Highlight(NoteEdit, startPos, endPos, showLinks);
		}

		private void NoteEdit_HotspotClick(object sender, HotspotClickEventArgs e)
		{
			if (Settings.LinkMode == LinkHighlightMode.SingleClick)
			{
				var links = DefaultHighlighter.FindAllLinks(NoteEdit);
				var link = links.FirstOrDefault(l => l.Item2 <= e.Position && e.Position <= l.Item3);
				if (link != null) Process.Start(link.Item1);
			}
			else if (Settings.LinkMode == LinkHighlightMode.ControlClick && e.Modifiers.HasFlag(Keys.Control))
			{
				var links = DefaultHighlighter.FindAllLinks(NoteEdit);
				var link = links.FirstOrDefault(l => l.Item2 <= e.Position && e.Position <= l.Item3);
				if (link != null) Process.Start(link.Item1);
			}
		}

		private void NoteEdit_HotspotDoubleClick(object sender, HotspotClickEventArgs e)
		{
			if (Settings.LinkMode == LinkHighlightMode.DoubleClick)
			{
				var links = DefaultHighlighter.FindAllLinks(NoteEdit);
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
		}

		public void ResetScintillaScrollAndUndo()
		{
			NoteEdit.ScrollWidth = 1;
			NoteEdit.ScrollWidthTracking = true;
			NoteEdit.EmptyUndoBuffer();
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
			if (e.Key == Key.System && ReferenceEquals(e.OriginalSource, NoteEditHost))
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
			// Manually call our Shortcuts
			// Cause the WindowsFormsHost fucks everything up

			if (e.Control && e.KeyCode == Keys.S)
			{
				viewmodel.SaveAndSyncCommand.Execute(sender);
				e.Handled = true;
			}

			if (e.Control && e.KeyCode == Keys.N)
			{
				viewmodel.CreateNewNoteCommand.Execute(sender);
				e.Handled = true;
			}

			if (e.Control && e.KeyCode == Keys.F)
			{
				viewmodel.DocumentSearchCommand.Execute(sender);
				e.Handled = true;
			}

			if (e.KeyCode == Keys.Escape)
			{
				viewmodel.CloseDocumentSearchCommand.Execute(sender);
				e.Handled = true;
			}
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			ResetScintillaScrollAndUndo();
		}

		public void ShowDocSearchBar()
		{
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
	}
}
