using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand DebugCreateIpsumNotesCommand  => new RelayCommand<string>(s => { DebugCreateIpsumNotes(int.Parse(s)); });
		public ICommand DebugSerializeSettingsCommand => new RelayCommand(DebugSerializeSettings);
		public ICommand DebugSerializeNoteCommand     => new RelayCommand(DebugSerializeNote);
		public ICommand DebugRefreshViewCommand       => new RelayCommand(()=> { Owner.NotesViewControl.RefreshView(true); });
		public ICommand DebugShowThemeEditorCommand   => new RelayCommand(DebugShowThemeEditor);
		public ICommand DebugShowDefaultThemeCommand  => new RelayCommand(DebugShowDefaultTheme);
		public ICommand DebugDiscoThemeCommand        => new RelayCommand(DebugDiscoTheme);
		public ICommand DebugNoteDiffCommand          => new RelayCommand(DebugNoteDiff);
		
		private void DebugCreateIpsumNotes(int c)
		{
			var path = Owner.NotesViewControl.GetNewNotesPath();

			for (int i = 0; i < c; i++)
			{
				string title = CreateLoremIpsum(4 + App.GlobalRandom.Next(5), 16);
				string text = CreateLoremIpsum((48 + App.GlobalRandom.Next(48)) * (8 + App.GlobalRandom.Next(8)), App.GlobalRandom.Next(8)+8);

				var n = Repository.CreateNewNote(path);

				n.Title = title;
				n.Text = text;

				int tc = App.GlobalRandom.Next(5);
				for (int j = 0; j < tc; j++) n.Tags.Add(CreateLoremIpsum(1,1));
			}
		}

		private void DebugSerializeSettings()
		{
			DebugTextWindow.Show(Owner, Settings.Serialize(), "Settings.Serialize()");
		}

		private void DebugSerializeNote()
		{
			if (SelectedNote == null) return;
			DebugTextWindow.Show(Owner, XHelper.ConvertToStringFormatted(Repository.SerializeNote(SelectedNote)), "XHelper.ConvertToStringFormatted(Repository.SerializeNote(SelectedNote))");
		}

		private void DebugShowThemeEditor()
		{
			var w = new ThemeEditor() { Owner = Owner };
			w.Show();
		}

		private void DebugShowDefaultTheme()
		{
			ThemeManager.Inst.ChangeTheme(ThemeManager.Inst.Cache.GetFallback(), new AlephTheme[0]);
		}

		private void DebugDiscoTheme()
		{
			var tmr = new DispatcherTimer(TimeSpan.FromSeconds(0.05), DispatcherPriority.Normal, (a, e) =>
			{
				ThemeManager.Inst.ChangeTheme(ThemeManager.Inst.Cache.GetFallback(), new AlephTheme[0]);
			}, Application.Current.Dispatcher);

			tmr.Start();
		}

		private INote _noteDiffSelection = null;
		private void DebugNoteDiff()
		{
			if (_noteDiffSelection == null)
			{
				_noteDiffSelection = SelectedNote;
			}
			else
			{
				var d1 = Tuple.Create(SelectedNote.Text, SelectedNote.Title, SelectedNote.Tags.OrderBy(p=>p).ToList(), SelectedNote.Path);
				var d2 = Tuple.Create(_noteDiffSelection.Text, _noteDiffSelection.Title, _noteDiffSelection.Tags.OrderBy(p=>p).ToList(), _noteDiffSelection.Path);
				_noteDiffSelection = null;

				ConflictWindow.Show(Repository, Owner, SelectedNote.UniqueName, d1, d2);
			}
		}

		private string CreateLoremIpsum(int len, int linelen)
		{
			var words = Regex.Split(Properties.Resources.LoremIpsum, @"\r?\n");
			StringBuilder b = new StringBuilder();
			for (int i = 0; i < len; i++)
			{
				if (i>0 && i % linelen == 0) b.Append("\r\n");
				else if (i > 0) b.Append(" ");
 
				b.Append(words[App.GlobalRandom.Next(words.Length)]);
			}
			return b.ToString(0,1).ToUpper() + b.ToString().Substring(1);
		}

	}
}
