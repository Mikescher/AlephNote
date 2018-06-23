using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.MVVM;
using Microsoft.Win32;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand CreateNewNoteCommand { get { return new RelayCommand(CreateNote);} }
		public ICommand CreateNewNoteFromClipboardCommand { get { return new RelayCommand(CreateNoteFromClipboard);} }
		public ICommand CreateNewNoteFromTextfileCommand { get { return new RelayCommand(CreateNewNoteFromTextfile);} }
		public ICommand ExportCommand { get { return new RelayCommand(ExportNote); } }
		public ICommand DeleteCommand { get { return new RelayCommand(DeleteNote); } }
		public ICommand DocumentSearchCommand { get { return new RelayCommand(ShowDocSearchBar); } }
		public ICommand DocumentContinueSearchCommand { get { return new RelayCommand(ContinueSearch); } }
		public ICommand CloseDocumentSearchCommand { get { return new RelayCommand(HideDocSearchBar); } }
		public ICommand InsertSnippetCommand { get { return new RelayCommand<string>(InsertSnippet); } }
		public ICommand ChangePathCommand { get { return new RelayCommand(() => Owner.PathEditor.ChangePath()); } }
		public ICommand DuplicateNoteCommand { get { return new RelayCommand(DuplicateNote); } }
		public ICommand PinUnpinNoteCommand { get { return new RelayCommand(PinUnpinNote); } }
		public ICommand LockUnlockNoteCommand { get { return new RelayCommand(LockUnlockNote); } }
		
		private void ExportNote()
		{
			if (SelectedNote == null) return;

			SaveFileDialog sfd = new SaveFileDialog();

			if (SelectedNote.HasTagCaseInsensitive(AppSettings.TAG_MARKDOWN))
			{
				sfd.Filter = "Markdown files (*.md)|*.md";
				sfd.FileName = SelectedNote.Title + ".md";
			}
			else
			{
				sfd.Filter = "Text files (*.txt)|*.txt";
				sfd.FileName = SelectedNote.Title + ".txt";
			}

			if (sfd.ShowDialog() == true)
			{
				try
				{
					File.WriteAllText(sfd.FileName, SelectedNote.Text, Encoding.UTF8);
				}
				catch (Exception e)
				{
					App.Logger.Error("Main", "Could not write to file", e);
					ExceptionDialog.Show(Owner, "Could not write to file", e, string.Empty);
				}
			}
		}

		private void DeleteNote()
		{
			try
			{
				if (SelectedNote == null) return;

				if (MessageBox.Show(Owner, "Do you really want to delete this note?", "Delete note?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

				Repository.DeleteNote(SelectedNote, true);

				SelectedNote = Repository.Notes.FirstOrDefault();
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Could not delete note", e);
				ExceptionDialog.Show(Owner, "Could not delete note", e, string.Empty);
			}
		}
		
		private void DuplicateNote()
		{
			if (SelectedNote == null) return;

			if (Owner.Visibility == Visibility.Hidden) ShowMainWindow();

			var title = SelectedNote.Title;
			var path = SelectedNote.Path;
			var text = SelectedNote.Text;
			var tags = SelectedNote.Tags.ToList();

			var ntitle = title + " (copy)";
			int i = 2;
			while (Repository.Notes.Any(n => n.Title.ToLower() == ntitle.ToLower())) ntitle = title + " (copy-" + (i++) + ")";
			title = ntitle;

			SelectedNote = Repository.CreateNewNote(path);

			SelectedNote.Title = title;
			SelectedNote.Text = text;
			SelectedNote.Tags.SynchronizeCollection(tags);
		}

		private void PinUnpinNote()
		{
			if (SelectedNote == null) return;

			SelectedNote.IsPinned = !SelectedNote.IsPinned;
		}

		private void LockUnlockNote()
		{
			if (SelectedNote == null) return;

			SelectedNote.IsLocked = !SelectedNote.IsLocked;
		}
		
		private void CreateNote()
		{
			try
			{
				var path = Owner.NotesViewControl.GetNewNotesPath();
				if (Owner.Visibility == Visibility.Hidden) ShowMainWindow();
				SelectedNote = Repository.CreateNewNote(path);
			}
			catch (Exception e)
			{
				ExceptionDialog.Show(Owner, "Cannot create note", e, string.Empty);
			}
		}
		
		private void CreateNoteFromClipboard()
		{
			var notepath = Owner.NotesViewControl.GetNewNotesPath();

			if (Clipboard.ContainsFileDropList())
			{
				if (Owner.Visibility == Visibility.Hidden) ShowMainWindow();

				foreach (var path in Clipboard.GetFileDropList())
				{
					var filename    = Path.GetFileNameWithoutExtension(path) ?? "New note from unknown file";
					var filecontent = File.ReadAllText(path);

					SelectedNote       = Repository.CreateNewNote(notepath);
					SelectedNote.Title = filename;
					SelectedNote.Text  = filecontent;
				}
			}
			else if (Clipboard.ContainsText())
			{
				if (Owner.Visibility == Visibility.Hidden) ShowMainWindow();

				var notetitle   = "New note from clipboard";
				var notecontent = Clipboard.GetText();
				if (!string.IsNullOrWhiteSpace(notecontent))
				{
					SelectedNote       = Repository.CreateNewNote(notepath);
					SelectedNote.Title = notetitle;
					SelectedNote.Text  = notecontent;
				}
			}
		}

		private void CreateNewNoteFromTextfile()
		{
			var notepath = Owner.NotesViewControl.GetNewNotesPath();

			var ofd = new OpenFileDialog
			{
				Multiselect = true,
				ShowReadOnly = true,
				DefaultExt = ".txt",
				Title = "Import new notes from text files",
				CheckFileExists = true,
				Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
			};

			if (ofd.ShowDialog() != true) return;

			try
			{ 
				foreach (var path in ofd.FileNames)
				{
					var filename    = Path.GetFileNameWithoutExtension(path) ?? "New note from unknown file";
					var filecontent = File.ReadAllText(path);

					SelectedNote       = Repository.CreateNewNote(notepath);
					SelectedNote.Title = filename;
					SelectedNote.Text  = filecontent;
				}
			}
			catch (Exception ex)
			{
				ExceptionDialog.Show(Owner, "Reading file failed", "Creating note from file failed due to an error", ex);
			}
		}
		
		private void ShowDocSearchBar()
		{
			Owner.ShowDocSearchBar();
		}

		private void HideDocSearchBar()
		{
			Owner.HideDocSearchBar();
		}

		private void ContinueSearch()
		{
			if (!Settings.DocSearchEnabled) return;
			if (Owner.DocumentSearchBar.Visibility != Visibility.Visible) return;

			Owner.DocumentSearchBar.ContinueSearch();
		}

		private void InsertSnippet(string snip)
		{
			if (SelectedNote == null) return;
			
			snip = _spsParser.Parse(snip, out bool succ);

			if (!succ)
			{
				App.Logger.Warn("Main", "Snippet has invalid format: '" + snip + "'");
			}

			Owner.NoteEdit.ReplaceSelection(snip);

			Owner.FocusScintilla();
		}

	}
}
