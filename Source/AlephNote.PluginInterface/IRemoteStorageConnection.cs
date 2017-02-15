
using System.Collections.Generic;

namespace AlephNote.PluginInterface
{
	public interface IRemoteStorageConnection
	{
		INote UploadNote(INote note);
		RemoteResult UpdateNote(INote note);
		void StartNewSync();
		void FinishNewSync();
		INote DownloadNote(string id, out bool result);
		void DeleteNote(INote note);
		List<string> ListMissingNotes(List<INote> localnotes);
	}
}
