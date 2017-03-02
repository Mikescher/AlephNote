using AlephNote.Plugins;
using AlephNote.Settings;
using ScintillaNET;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewmodel viewmodel;

		public MainWindow()
		{
			InitializeComponent();

			PluginManager.LoadPlugins();

			bool firstLaunch = false;
			AppSettings settings;
			try
			{
				if (File.Exists(App.PATH_SETTINGS))
				{
					settings = AppSettings.Load();
				}
				else
				{
					settings = AppSettings.CreateEmpty();
					settings.Save();

					firstLaunch = true;
				}
			}
			catch (Exception e)
			{
				ExceptionDialog.Show(null, "Could not load settings", "Could not load settings from " + App.PATH_SETTINGS, e);
				settings = AppSettings.CreateEmpty();
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
				WindowState = settings.StartupState;

				Left = settings.StartupPositionX;
				Top = settings.StartupPositionY;

				Width = settings.StartupPositionWidth;
				Height = settings.StartupPositionHeight;
			}
			else if (settings.StartupLocation == ExtendedWindowStartupLocation.Manual)
			{
				WindowStartupLocation = WindowStartupLocation.Manual;
				WindowState = settings.StartupState;

				Left = settings.StartupPositionX;
				Top = settings.StartupPositionY;

				Width = settings.StartupPositionWidth;
				Height = settings.StartupPositionHeight;
			}
			else if (settings.StartupLocation == ExtendedWindowStartupLocation.ScreenBottomLeft)
			{
				var screen = WpfScreen.GetScreenFrom(this);

				WindowStartupLocation = WindowStartupLocation.Manual;
				WindowState = settings.StartupState;

				Left = screen.WorkingArea.Left + 5;
				Top = screen.WorkingArea.Bottom - settings.StartupPositionHeight - 5;

				Width = settings.StartupPositionWidth;
				Height = settings.StartupPositionHeight;
			}
			else if (settings.StartupLocation == ExtendedWindowStartupLocation.ScreenLeft)
			{
				var screen = WpfScreen.GetScreenFrom(this);

				WindowStartupLocation = WindowStartupLocation.Manual;
				WindowState = settings.StartupState;

				Left = screen.WorkingArea.Left + 5;
				Top = screen.WorkingArea.Top + 5;

				Width = settings.StartupPositionWidth;
				Height = screen.WorkingArea.Height - 10;
			}

			if (settings.MinimizeToTray && settings.StartupState == WindowState.Minimized)
				Hide();
		}

		public void SetupScintilla(AppSettings s)
		{
			NoteEdit.Lexer = Lexer.Null;

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

			NoteEdit.Font = new Font(s.NoteFontFamily.Source, (int)s.NoteFontSize);
			NoteEdit.Styles[0].Bold = s.NoteFontModifier == FontModifier.Bold || s.NoteFontModifier == FontModifier.BoldItalic;
			NoteEdit.Styles[0].Italic = s.NoteFontModifier == FontModifier.Italic || s.NoteFontModifier == FontModifier.BoldItalic;
			NoteEdit.Styles[0].Size = (int)s.NoteFontSize;
			NoteEdit.Styles[0].Font = s.NoteFontFamily.Source;

			NoteEdit.WrapMode = s.SciWordWrap ? WrapMode.Whitespace : WrapMode.None;

			NoteEdit.ZoomChanged -= ZoomChanged;
			NoteEdit.ZoomChanged += ZoomChanged;

			NoteEdit.UseTabs = s.SciUseTabs;
			NoteEdit.TabWidth = s.SciTabWidth * 2;

			ResetScintillaScrollAndUndo();
		}

		private void ZoomChanged(object sender, EventArgs args)
		{
			if (viewmodel.Settings.SciZoomable)
			{
				viewmodel.Settings.SciZoom = NoteEdit.Zoom;
				viewmodel.RequestSettingsSave();
			}
			else
			{
				if (NoteEdit.Zoom != 0)
				{
					NoteEdit.Zoom = 0;
					if (viewmodel.Settings.SciZoom != 0)
					{
						viewmodel.Settings.SciZoom = NoteEdit.Zoom;
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
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			ResetScintillaScrollAndUndo();
		}
	}
}
