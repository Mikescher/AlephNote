using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AlephNote.Common.Settings;
using AlephNote.WPF.Dialogs;
using MSHC.WPF.MVVM;
using Microsoft.Win32;
using MSHC.Lang.Collections;
using MSHC.Util.Helper;
using Ookii.Dialogs.Wpf;
using ScintillaNET;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand CreateNewNoteCommand
		{
			get { return new RelayCommand(CreateNote); }
		}

		public ICommand CreateNewNoteFromClipboardCommand
		{
			get { return new RelayCommand(CreateNoteFromClipboard); }
		}

		public ICommand CreateNewNoteFromTextfileCommand
		{
			get { return new RelayCommand(CreateNewNoteFromTextfile); }
		}

		public ICommand ExportCommand
		{
			get { return new RelayCommand(ExportNote); }
		}

		public ICommand DeleteCommand
		{
			get { return new RelayCommand(DeleteNote); }
		}

		public ICommand DocumentSearchCommand
		{
			get { return new RelayCommand(ShowDocSearchBar); }
		}

		public ICommand DocumentContinueSearchCommand
		{
			get { return new RelayCommand(ContinueSearch); }
		}

		public ICommand CloseDocumentSearchCommand
		{
			get { return new RelayCommand(HideDocSearchBar); }
		}

		public ICommand InsertSnippetCommand
		{
			get { return new RelayCommand<string>(InsertSnippet); }
		}

		public ICommand ChangePathCommand
		{
			get { return new RelayCommand(() => Owner.PathEditor.ChangePath()); }
		}

		public ICommand DuplicateNoteCommand
		{
			get { return new RelayCommand(DuplicateNote); }
		}

		public ICommand PinUnpinNoteCommand
		{
			get { return new RelayCommand(PinUnpinNote); }
		}

		public ICommand LockUnlockNoteCommand
		{
			get { return new RelayCommand(LockUnlockNote); }
		}

		public ICommand InsertHyperlinkCommand
		{
			get { return new RelayCommand(InsertHyperlink); }
		}

		public ICommand InsertFilelinkCommand
		{
			get { return new RelayCommand(InsertFilelink); }
		}

		public ICommand InsertNotelinkCommand
		{
			get { return new RelayCommand(InsertNotelink); }
		}

		public ICommand InsertMaillinkCommand
		{
			get { return new RelayCommand(InsertMaillink); }
		}

		public ICommand MoveCurrentLineUpCommand
		{
			get { return new RelayCommand(MoveCurrentLineUp); }
		}

		public ICommand MoveCurrentLineDownCommand
		{
			get { return new RelayCommand(MoveCurrentLineDown); }
		}

		public ICommand DuplicateCurrentLineCommand
		{
			get { return new RelayCommand(DuplicateCurrentLine); }
		}

		public ICommand CopyCurrentLineCommand
		{
			get { return new RelayCommand(CopyCurrentLine); }
		}

		public ICommand CutCurrentLineCommand
		{
			get { return new RelayCommand(CutCurrentLine); }
		}

		public ICommand CopyAllowLineCommand
		{
			get { return new RelayCommand(CopyAllowLine); }
		}

		public ICommand CutAllowLineCommand
		{
			get { return new RelayCommand(CutAllowLine); }
		}

		private void ExportNote()
		{
			if (SelectedNote == null) return;

			var selection = GetAllSelectedNotes();
			if (selection.Count > 1)
			{
				var dialog = new VistaFolderBrowserDialog();
				if (!(dialog.ShowDialog() ?? false)) return;

				try
				{
					var directory = dialog.SelectedPath;
					foreach (var note in selection)
					{
						var filenameRaw = FilenameHelper.StripStringForFilename(note.Title, FilenameHelper.ValidityMode.AllowWhitelist);
						var filename = filenameRaw;
						var ext = SelectedNote.HasTagCaseInsensitive(AppSettings.TAG_MARKDOWN) ? ".md" : ".txt";

						int i = 1;
						while (File.Exists(Path.Combine(directory, filename + ext)))
						{
							i++;
							filename = $"{filenameRaw} ({i})";
						}

						File.WriteAllText(Path.Combine(directory, filename + ext), note.Text, Encoding.UTF8);
					}
				}
				catch (Exception e)
				{
					App.Logger.Error("Main", "Could not write to file", e);
					ExceptionDialog.Show(Owner, "Could not write to file", e, string.Empty);
				}
			}
			else
			{
				var sfd = new SaveFileDialog();

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

				if (sfd.ShowDialog() != true) return;
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

				var selection = GetAllSelectedNotes();
				if (selection.Count > 1)
				{
					if (MessageBox.Show(Owner, $"Do you really want to delete {selection.Count} notes?", "Delete multiple notes?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

					foreach (var note in selection) Repository.DeleteNote(note, true);

					SelectedNote = Repository.Notes.FirstOrDefault();
				}
				else
				{
					if (MessageBox.Show(Owner, "Do you really want to delete this note?", "Delete note?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

					Repository.DeleteNote(SelectedNote, true);

					SelectedNote = Repository.Notes.FirstOrDefault();
				}
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Could not delete note(s)", e);
				ExceptionDialog.Show(Owner, "Could not delete note(s)", e, string.Empty);
			}
		}

		private void DuplicateNote()
		{
			if (SelectedNote == null) return;

			if (Owner.Visibility == Visibility.Hidden) ShowMainWindow();

			var selection = GetAllSelectedNotes();
			if (selection.Count > 1)
			{
				if (MessageBox.Show(Owner, $"Do you really want to duplicate {selection.Count} notes?", "Duplicate multiple notes?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

				var lastNote = SelectedNote;
				foreach (var note in selection)
				{
					var title = note.Title;
					var path = note.Path;
					var text = note.Text;
					var tags = note.Tags.ToList();

					var ntitle = title + " (copy)";
					var i = 2;
					while (Repository.Notes.Any(n => n.Title.ToLower() == ntitle.ToLower())) ntitle = title + " (copy-" + (i++) + ")";
					title = ntitle;

					lastNote = Repository.CreateNewNote(path);

					lastNote.Title = title;
					lastNote.Text = text;
					lastNote.Tags.SynchronizeCollection(tags);
				}

				SelectedNote = lastNote;
			}
			else
			{
				var title = SelectedNote.Title;
				var path = SelectedNote.Path;
				var text = SelectedNote.Text;
				var tags = SelectedNote.Tags.ToList();

				var ntitle = title + " (copy)";
				var i = 2;
				while (Repository.Notes.Any(n => n.Title.ToLower() == ntitle.ToLower())) ntitle = title + " (copy-" + (i++) + ")";
				title = ntitle;

				SelectedNote = Repository.CreateNewNote(path);

				SelectedNote.Title = title;
				SelectedNote.Text = text;
				SelectedNote.Tags.SynchronizeCollection(tags);
			}
		}

		private void PinUnpinNote()
		{
			if (!Repository.SupportsPinning)
			{
				MessageBox.Show(Owner, "Pinning notes is not supported by your note provider", "Unsupported oprtation!", MessageBoxButton.OK);
				return;
			}

			if (SelectedNote == null) return;

			var selection = GetAllSelectedNotes();
			if (selection.Count > 1)
			{
				var newpin = !selection[0].IsPinned;

				if (MessageBox.Show(Owner, $"Do you really want to {(newpin ? "pin" : "unpin")} {selection.Count} notes?", $"{(newpin ? "Pin" : "Unpin")} multiple note?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

				foreach (var note in selection) note.IsPinned = newpin;
			}
			else
			{
				SelectedNote.IsPinned = !SelectedNote.IsPinned;
			}
		}

		private void LockUnlockNote()
		{
			if (!Repository.SupportsLocking)
			{
				MessageBox.Show(Owner, "Locking notes is not supported by your note provider", "Unsupported oprtation!", MessageBoxButton.OK);
				return;
			}

			if (SelectedNote == null) return;

			var selection = GetAllSelectedNotes();
			if (selection.Count > 1)
			{
				var newlock = !selection[0].IsLocked;

				if (MessageBox.Show(Owner, $"Do you really want to {(newlock ? "lock" : "unlock")} {selection.Count} notes?", $"{(newlock ? "Lock" : "Unlock")} multiple note?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

				foreach (var note in selection) note.IsLocked = newlock;
			}
			else
			{
				SelectedNote.IsLocked = !SelectedNote.IsLocked;
			}
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
					var filename = Path.GetFileNameWithoutExtension(path) ?? "New note from unknown file";
					var filecontent = File.ReadAllText(path);

					SelectedNote = Repository.CreateNewNote(notepath);
					SelectedNote.Title = filename;
					SelectedNote.Text = filecontent;
				}
			}
			else if (Clipboard.ContainsText())
			{
				if (Owner.Visibility == Visibility.Hidden) ShowMainWindow();

				var notetitle = "New note from clipboard";
				var notecontent = Clipboard.GetText();
				if (!string.IsNullOrWhiteSpace(notecontent))
				{
					SelectedNote = Repository.CreateNewNote(notepath);
					SelectedNote.Title = notetitle;
					SelectedNote.Text = notecontent;
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
					var filename = Path.GetFileNameWithoutExtension(path) ?? "New note from unknown file";
					var filecontent = File.ReadAllText(path);

					SelectedNote = Repository.CreateNewNote(notepath);
					SelectedNote.Title = filename;
					SelectedNote.Text = filecontent;
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

			snip = _spsParser.Parse(snip, Repository, SelectedNote, out bool succ);

			if (!succ)
			{
				App.Logger.Warn("Main", "Snippet has invalid format: '" + snip + "'");
			}

			Owner.NoteEdit.ReplaceSelection(snip);

			Owner.FocusScintilla();
		}

		private void InsertHyperlink()
		{
			if (SelectedNote == null) return;

			if (!GenericInputDialog.ShowInputDialog(Owner, "Insert website address", "Hyperlink location", "", out var url)) return;
			if (string.IsNullOrWhiteSpace(url)) return;

			if (!(url.ToLower().StartsWith("http://") || url.ToLower().StartsWith("https://"))) url = "https://" + url;

			Owner.NoteEdit.ReplaceSelection(url);
			Owner.FocusScintilla();
		}

		private void InsertFilelink()
		{
			if (SelectedNote == null) return;

			var ofd = new OpenFileDialog();

			var inst = MainWindow.Instance;

			if (inst != null && inst.IsVisible && inst.IsActive && !inst.IsClosed)
			{
				if (ofd.ShowDialog(inst) != true) return;
			}
			else
			{
				if (ofd.ShowDialog() != true) return;
			}

			var uri = new Uri(ofd.FileName).AbsoluteUri;

			Owner.NoteEdit.ReplaceSelection(uri);
			Owner.FocusScintilla();
		}

		private void InsertMaillink()
		{
			if (SelectedNote == null) return;

			if (!GenericInputDialog.ShowInputDialog(Owner, "Insert mail address", "Email address", "", out var url)) return;
			if (string.IsNullOrWhiteSpace(url)) return;

			url = "mailto:" + url;

			Owner.NoteEdit.ReplaceSelection(url);
			Owner.FocusScintilla();
		}

		private void InsertNotelink()
		{
			if (SelectedNote == null) return;

			if (!NoteChooserDialog.ShowInputDialog(Owner, "Choose note to link", Repository, null, out var note)) return;

			var uri = "note://" + note.UniqueName;

			Owner.NoteEdit.ReplaceSelection(uri);
			Owner.FocusScintilla();
		}

		private void MoveCurrentLineUp()
		{
			if (SelectedNote == null) return;

			var hasSelection = Owner.NoteEdit.Selections.Any(s => s.End - s.Start > 1);

			Owner.NoteEdit.ExecuteCmd(Command.MoveSelectedLinesUp);

			if (!hasSelection) Owner.NoteEdit.SetEmptySelection(Owner.NoteEdit.CurrentPosition);
		}

		private void MoveCurrentLineDown()
		{
			if (SelectedNote == null) return;

			var hasSelection = Owner.NoteEdit.Selections.Any(s => s.End - s.Start > 1);

			Owner.NoteEdit.ExecuteCmd(Command.MoveSelectedLinesDown);

			if (!hasSelection) Owner.NoteEdit.SetEmptySelection(Owner.NoteEdit.CurrentPosition);
		}

		private void DuplicateCurrentLine()
		{
			if (SelectedNote == null) return;

			var lineidx = Owner.NoteEdit.CurrentLine;
			var lines = Owner.NoteEdit.Lines;
			if (lineidx < 0 || lineidx >= lines.Count) return;

			if (lineidx == lines.Count - 1)
				Owner.NoteEdit.InsertText(lines[lineidx].EndPosition, "\r\n" + lines[lineidx].Text);
			else
				Owner.NoteEdit.InsertText(lines[lineidx].EndPosition, lines[lineidx].Text);
		}

		private void CopyCurrentLine()
		{
			if (SelectedNote == null) return;

			var lineidx = Owner.NoteEdit.CurrentLine;
			var lines = Owner.NoteEdit.Lines;
			if (lineidx < 0 || lineidx >= lines.Count) return;

			Owner.NoteEdit.CopyRange(lines[lineidx].Position, lines[lineidx].EndPosition);
		}

		private void CutCurrentLine()
		{
			if (SelectedNote == null) return;

			var lineidx = Owner.NoteEdit.CurrentLine;
			var lines = Owner.NoteEdit.Lines;
			if (lineidx < 0 || lineidx >= lines.Count) return;

			Owner.NoteEdit.CopyRange(lines[lineidx].Position, lines[lineidx].EndPosition);
			Owner.NoteEdit.DeleteRange(lines[lineidx].Position, lines[lineidx].Length);
		}

		private void CopyAllowLine()
		{
			if (SelectedNote == null) return;

			Owner.NoteEdit.CopyAllowLine();
		}

		private void CutAllowLine()
		{
			if (SelectedNote == null) return;

			var lineidx = Owner.NoteEdit.CurrentLine;
			var lines = Owner.NoteEdit.Lines;
			if (lineidx < 0 || lineidx >= lines.Count) return;

			var hasSelection = Owner.NoteEdit.Selections.Any(s => s.End - s.Start > 1);

			if (hasSelection)
			{
				Owner.NoteEdit.Cut();
			}
			else
			{
				Owner.NoteEdit.CopyAllowLine();
				Owner.NoteEdit.DeleteRange(lines[lineidx].Position, lines[lineidx].Length);
			}
		}
	}
}