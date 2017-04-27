using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System.Collections.Generic;

namespace AlephNote.Plugins.Headless
{
	class HeadlessConnection : BasicRemoteConnection
	{
		public override void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes, List<INote> localdeletednotes)
		{
			// ok
		}

		public override void FinishSync()
		{
			//
		}

		public override bool NeedsUpload(INote note)
		{
			return false;
		}

		public override bool NeedsDownload(INote note)
		{
			return false;
		}

		public override INote DownloadNote(string id, out bool success)
		{
			success = false;
			return null;
		}

		public override void DeleteNote(INote note)
		{
			// ok
		}

		public override List<string> ListMissingNotes(List<INote> localnotes)
		{
			return new List<string>();
		}

		public override RemoteDownloadResult UpdateNoteFromRemote(INote note)
		{
			return RemoteDownloadResult.UpToDate;
		}

		public override RemoteUploadResult UploadNoteToRemote(ref INote note, out INote conflict, ConflictResolutionStrategy strategy)
		{
			conflict = null;
			return RemoteUploadResult.UpToDate;
		}
	}
}
