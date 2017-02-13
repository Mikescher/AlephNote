using CommonNote.Settings;
using ScintillaNET;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace CommonNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			PluginManager.LoadPlugins();

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
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not load settings from " + App.PATH_SETTINGS + "\r\n\r\n" + e, "Could not load settings");
				settings = AppSettings.CreateEmpty();
			}

			StartupConfigWindow(settings);

			SetupScintilla(settings);

			DataContext = new MainWindowViewmodel(settings, this);
		}

		private void StartupConfigWindow(AppSettings settings)
		{
			WindowStartupLocation = settings.StartupLocation;
			WindowState = settings.StartupState;

			Left = settings.StartupPositionX;
			Top = settings.StartupPositionY;

			Width = settings.StartupPositionWidth;
			Height = settings.StartupPositionHeight;
		}

		public void SetupScintilla(AppSettings s)
		{
			NoteEdit.Lexer = Lexer.Null;

			NoteEdit.WhitespaceSize = 1;
			NoteEdit.ViewWhitespace = s.SciShowWhitespace ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible;
			NoteEdit.SetWhitespaceForeColor(true, Color.Orange);

			NoteEdit.Margins[0].Width = 0;
			NoteEdit.Margins[1].Width = 0;
			NoteEdit.Margins[2].Width = 0;
			NoteEdit.Margins[3].Width = 0;
			NoteEdit.BorderStyle = BorderStyle.FixedSingle;

			NoteEdit.MultipleSelection = s.SciRectSelection;
			NoteEdit.MouseSelectionRectangularSwitch = s.SciRectSelection;
			NoteEdit.AdditionalSelectionTyping = s.SciRectSelection;
			NoteEdit.VirtualSpaceOptions = s.SciRectSelection ? VirtualSpace.RectangularSelection : VirtualSpace.None;

			NoteEdit.Font = new Font(s.NoteFontName, (int)s.NoteFontSize);
			NoteEdit.Styles[0].Bold = s.NoteFontModifier == FontModifier.Bold || s.NoteFontModifier == FontModifier.BoldItalic;
			NoteEdit.Styles[0].Italic = s.NoteFontModifier == FontModifier.Italic || s.NoteFontModifier == FontModifier.BoldItalic;
			NoteEdit.Styles[0].Size = (int)s.NoteFontSize;
			NoteEdit.Styles[0].Font = s.NoteFontName.Name;

			NoteEdit.WrapMode = s.SciWordWrap ? WrapMode.Whitespace : WrapMode.None;

			ResetScintillaScroll();
		}

		public void ResetScintillaScroll()
		{
			NoteEdit.ScrollWidth = 1;
			NoteEdit.ScrollWidthTracking = true;
		}

		public void FocusScintilla()
		{
			NoteEditHost.Focus();
			Keyboard.Focus(NoteEditHost);
			NoteEdit.Focus();
		}
	}
}
