using System;
using System.Collections.Generic;

namespace AlephNote.Repository
{
	public interface ISynchronizationFeedback
	{
		void StartSync();

		void SyncSuccess(DateTimeOffset now);
		void SyncError(List<Tuple<string, Exception>> errors);

		void OnSyncRequest();
	}
}
