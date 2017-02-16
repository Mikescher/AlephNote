using AlephNote.PluginInterface;
using System.Collections.Generic;

namespace AlephNote.Plugins.Headless
{
	class HeadlessConnection : IRemoteStorageConnection
	{
		public void StartSync()
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

		public RemoteResult UpdateNote(INote note)
		{
			return RemoteResult.UpToDate;
		}

		public INote UploadNote(INote note)
		{
			return note;
		}
	}
}
