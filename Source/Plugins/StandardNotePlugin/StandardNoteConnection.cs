using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Net;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://github.com/standardnotes/doc/blob/master/Client%20Development%20Guide.md
	/// http://standardfile.org/#api
	/// </summary>
	class StandardNoteConnection : IRemoteStorageConnection
	{
		private readonly StandardNoteConfig _config;
		private readonly IWebProxy _proxy;

		private StandardNoteAPI.APIResultAuthorize _token = null;

		public StandardNoteConnection(IWebProxy proxy, StandardNoteConfig config)
		{
			_config = config;
			_proxy = proxy;
		}

		private void RefreshToken()
		{
			try
			{
				if (_token == null)
					_token = StandardNoteAPI.Authenticate(_proxy, _config.Server, _config.Email, _config.Password);
			}
			catch (Exception e)
			{
				throw new Exception("Could not authenticate with SimpleNote server : " + e.Message, e);
			}
		}

		public void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes)
		{
			RefreshToken();
		}

		public void FinishSync()
		{
			throw new NotImplementedException();
		}

		public RemoteUploadResult UploadNoteToRemote(ref INote note, out INote conflict, ConflictResolutionStrategy strategy)
		{
			throw new NotImplementedException();
		}

		public RemoteDownloadResult UpdateNoteFromRemote(INote note)
		{
			throw new NotImplementedException();
		}

		public INote DownloadNote(string id, out bool result)
		{
			throw new NotImplementedException();
		}

		public void DeleteNote(INote note)
		{
			throw new NotImplementedException();
		}

		public List<string> ListMissingNotes(List<INote> localnotes)
		{
			throw new NotImplementedException();
		}

		public bool NeedsUpload(INote note)
		{
			throw new NotImplementedException();
		}

		public bool NeedsDownload(INote note)
		{
			throw new NotImplementedException();
		}
	}
}
