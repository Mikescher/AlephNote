using AlephNote.PluginInterface;
using System.Collections.Generic;

namespace AlephNote.Plugins.Headless
{
	class HeadlessConnection : IRemoteStorageConnection
	{
		public void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes)
		{
			// ok
		}

		public void FinishSync()
		{
			//
		}

		public INote DownloadNote(string id, out bool result)
		{
			result = false;
			return null;
		}

		public void DeleteNote(INote note)
		{
			// ok
		}

		public List<string> ListMissingNotes(List<INote> localnotes)
		{
			return new List<string>();
		}

		public bool NeedsUpload(INote note)
		{
			return false;
		}

		public bool NeedsDownload(INote note)
		{
			return false;
		}

		public RemoteDownloadResult UpdateNoteFromRemote(INote note)
		{
			return RemoteDownloadResult.UpToDate;
		}

		public RemoteUploadResult UploadNoteToRemote(ref INote note, out INote conflict, ConflictResolutionStrategy strategy)
		{
			conflict = null;
			return RemoteUploadResult.UpToDate;
		}
	}
}
