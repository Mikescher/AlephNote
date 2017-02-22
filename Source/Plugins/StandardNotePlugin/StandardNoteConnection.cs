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
		private readonly IAlephLogger _logger;

		private StandardNoteAPI.APIResultAuthorize _token = null;

		public StandardNoteConnection(IAlephLogger log, IWebProxy proxy, StandardNoteConfig config)
		{
			_config = config;
			_proxy = proxy;
			_logger = log;
		}

		private void RefreshToken()
		{
			try
			{
				if (_token == null)
				{
					_logger.Debug(StandardNotePlugin.Name, "Requesting token from Simplenote server");

					_token = StandardNoteAPI.Authenticate(_proxy, _config.Server, _config.Email, _config.Password, _logger);

					_logger.Debug(StandardNotePlugin.Name, "Simplenote server returned token for user " + _token.user.uuid);
				}
			}
			catch (Exception e)
			{
				throw new Exception("Could not authenticate with SimpleNote server : " + e.Message, e);
			}
		}

		public void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes)
		{
			RefreshToken();

			var upNotes = new List<StandardNote>();
			foreach (var inote in localnotes)
			{
				var note = (StandardNote) inote;

				if (note.IsConflictNote) continue;

				if (!note.IsRemoteSaved) upNotes.Add(note);
			}

			StandardNoteAPI.Sync(_proxy, _token, _config.Server, (StandardNoteData)data, upNotes);
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
