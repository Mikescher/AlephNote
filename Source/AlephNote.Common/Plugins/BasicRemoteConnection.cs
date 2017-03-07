using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AlephNote.Common
{
	public abstract class BasicRemoteConnection : IRemoteStorageConnection
	{
		public static Func<IWebProxy, string, ISimpleJsonRest> SimpleJsonRestWrapper = (rest, proxy) => { throw new NotImplementedException(); };

		public abstract void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes, List<INote> localdeletednotes);
		public abstract void FinishSync();

		public abstract bool NeedsUpload(INote note);
		public abstract bool NeedsDownload(INote note);

		public abstract List<string> ListMissingNotes(List<INote> localnotes);

		public abstract RemoteUploadResult UploadNoteToRemote(ref INote note, out INote conflict, ConflictResolutionStrategy strategy);
		public abstract RemoteDownloadResult UpdateNoteFromRemote(INote note);
		public abstract INote DownloadNote(string id, out bool success);
		public abstract void DeleteNote(INote note);

		public ISimpleJsonRest CreateJsonRestClient(IWebProxy proxy, string host)
		{
			return SimpleJsonRestWrapper(proxy, host);
		}
	}
}
