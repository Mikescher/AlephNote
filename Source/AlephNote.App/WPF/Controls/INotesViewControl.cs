using System.Collections;
using AlephNote.PluginInterface;
using System.Collections.Generic;
using AlephNote.Common.Settings.Types;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Windows;

namespace AlephNote.WPF.Controls
{
	public interface INotesViewControl
	{
		INote GetTopNote();
		void RefreshView();
		bool Contains(INote n);
		DirectoryPath GetNewNotesPath();

		void DeleteFolder(DirectoryPath folder);
		void AddFolder(DirectoryPath folder);

		IEnumerable<INote> EnumerateVisibleNotes();
		void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> list);
		IEnumerable<DirectoryPath> ListFolder();
	}
}
