using CommonNote.Settings;
using ScintillaNET;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
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

			SetupScintilla();

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

			DataContext = new MainWindowViewmodel(settings, this);
		}

		private void SetupScintilla()
		{
			NoteEdit.WhitespaceSize = 1; //TODO via config
			NoteEdit.ViewWhitespace = WhitespaceMode.VisibleAlways;
			NoteEdit.SetWhitespaceForeColor(true, Color.Orange);

			NoteEdit.Margins[0].Width = 0;
			NoteEdit.Margins[1].Width = 0;
			NoteEdit.Margins[2].Width = 0;
			NoteEdit.Margins[3].Width = 0;
			NoteEdit.BorderStyle = BorderStyle.FixedSingle;

			NoteEdit.MultipleSelection = true; //TODO via config
			NoteEdit.VirtualSpaceOptions = VirtualSpace.RectangularSelection;

			NoteEdit.Font = new Font("Segeo UI", 1f); //TODO via config
			NoteEdit.Styles[0].Bold = true;

			NoteEdit.WrapMode = WrapMode.None;  //TODO via config

			ResetScintillaScroll();
		}

		public void ResetScintillaScroll()
		{
			NoteEdit.ScrollWidth = 1;
			NoteEdit.ScrollWidthTracking = true;
		}
	}
}
