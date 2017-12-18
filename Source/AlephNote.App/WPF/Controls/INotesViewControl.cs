using AlephNote.PluginInterface;
using System.Collections.Generic;
using AlephNote.Common.Settings.Types;
using AlephNote.WPF.Windows;

namespace AlephNote.WPF.Controls
{
	public interface INotesViewControl
	{
		INote GetTopNote();
		void RefreshView();
		bool Contains(INote n);

		IEnumerable<INote> EnumerateVisibleNotes();
		void SetShortcuts(MainWindow mw, List<KeyValuePair<string, ShortcutDefinition>> list);
	}
}
