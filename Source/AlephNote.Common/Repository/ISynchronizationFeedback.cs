using System;
using System.Collections.Generic;
using AlephNote.PluginInterface;

namespace AlephNote.Common.Repository
{
	public interface ISynchronizationFeedback
	{
		void StartSync();

		void SyncSuccess(DateTimeOffset now);
		void SyncError(List<Tuple<string, Exception>> errors);

		void OnSyncRequest();

		void OnNoteChanged(NoteChangedEventArgs e);
	}
}
