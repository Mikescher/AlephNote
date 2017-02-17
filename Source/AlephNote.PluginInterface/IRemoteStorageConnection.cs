
using System.Collections.Generic;

namespace AlephNote.PluginInterface
{
	public interface IRemoteStorageConnection
	{
		RemoteUploadResult UploadNoteToRemote(ref INote note, out INote conflict, ConflictResolutionStrategy strategy);
		RemoteDownloadResult UpdateNoteFromRemote(INote note);
		void StartSync();
		void FinishSync();
		INote DownloadNote(string id, out bool result);
		void DeleteNote(INote note);
		List<string> ListMissingNotes(List<INote> localnotes);

		bool NeedsUpload(INote note);
		bool NeedsDownload(INote note);
	}
}
