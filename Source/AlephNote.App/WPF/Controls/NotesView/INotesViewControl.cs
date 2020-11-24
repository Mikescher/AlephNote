using System.Collections.Generic;
using AlephNote.Common.Settings.Types;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Windows;

namespace AlephNote.WPF.Controls.NotesView
{
	public interface INotesViewControl
	{
		INote GetTopNote();
		bool IsTopSortedNote(INote n);
		void RefreshView();
		bool Contains(INote n);
		DirectoryPath GetNewNotesPath();

		void DeleteFolder(DirectoryPath folder);
		void AddFolder(DirectoryPath folder);
		void MoveFolder(DirectoryPath path, int delta);
		bool ExternalScrollEmulation(int eDelta);

		IEnumerable<INote> EnumerateVisibleNotes();
		void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> list);
		IEnumerable<DirectoryPath> ListFolder();

		void FocusNotesList();
		void FocusFolderList();

        void ForceSaveNow();
        void SaveIfDirty();
    }
}
