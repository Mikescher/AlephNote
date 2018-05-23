using System;
using System.Collections.Generic;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.Repository
{
	public interface ISynchronizationFeedback
	{
		void StartSync();

		void SyncSuccess(DateTimeOffset now);
		void SyncError(List<Tuple<string, Exception>> errors);

		void OnSyncRequest();

		void OnNoteChanged(NoteChangedEventArgs e);

		void ShowConflictResolutionDialog(string uuid, string txt0, string ttl0, List<string> tgs0, DirectoryPath ndp0, string txt1, string ttl1, List<string> tgs1, DirectoryPath ndp1);
	}
}
