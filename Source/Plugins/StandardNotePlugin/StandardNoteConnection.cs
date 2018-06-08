using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AlephNote.PluginInterface.Exceptions;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://github.com/standardnotes/doc/blob/master/Client%20Development%20Guide.md
	/// http://standardfile.org/#api
	/// </summary>
	public class StandardNoteConnection : BasicRemoteConnection
	{
		private readonly StandardNoteConfig _config;
		private readonly IWebProxy _proxy;
		private readonly IAlephLogger _logger;

		private StandardNoteAPI.APIResultAuthorize _token = null;
		private StandardNoteAPI.SyncResult _syncResult = null;

		public readonly HierachyEmulationConfig HConfig;

		public StandardNoteConnection(IAlephLogger log, IWebProxy proxy, StandardNoteConfig config, HierachyEmulationConfig hConfig)
		{
			HConfig = hConfig;

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
					using (var web = CreateJsonRestClient(_proxy, _config.Server))
					{
						_logger.Debug(StandardNotePlugin.Name, "Requesting token from StandardNoteServer");

						_token = StandardNoteAPI.Authenticate(web, _config.Email, _config.Password, _logger);

						_logger.Debug(StandardNotePlugin.Name, "StandardNoteServer returned token for user " + _token.user.uuid);
					}
				}
			}
			catch (StandardNoteAPIException)
			{
				throw;
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Could not authenticate with StandardNoteServer : " + e.Message, e);
			}
		}

		private ISimpleJsonRest CreateAuthenticatedClient()
		{
			RefreshToken();

			var client = CreateJsonRestClient(_proxy, _config.Server);
			client.AddHeader("Authorization", "Bearer " + _token.token);
			client.AddDTOConverter(ConvertToDTO, ConvertFromDTO);

			return client;
		}

		private string ConvertFromDTO(DateTimeOffset value)
		{
			return value.UtcDateTime.ToString("O");
		}

		private DateTimeOffset ConvertToDTO(string value)
		{
			DateTimeOffset o;
			if (DateTimeOffset.TryParse(value, out o)) return o;
			return DateTimeOffset.MinValue;
		}

		public override void StartSync(IRemoteStorageSyncPersistance idata, List<INote> ilocalnotes, List<INote> localdeletednotes)
		{
			StandardNoteAPI.Logger = _logger;

			using (var web = CreateAuthenticatedClient())
			{
				var data = (StandardNoteData)idata;

				var localnotes = ilocalnotes.Cast<StandardFileNote>().ToList();

				var upNotes = localnotes.Where(NeedsUpload).ToList();
				var delNotes = localdeletednotes.Cast<StandardFileNote>().ToList();
				var delTags = data.GetUnusedTags(localnotes.ToList());

				_syncResult = StandardNoteAPI.Sync(web, this, _token, _config, data, localnotes, upNotes, delNotes, delTags);

				_logger.Debug(StandardNotePlugin.Name, "StandardFile sync finished.",
					string.Format("upload:[notes={8} deleted={9}]" + "\r\n" + "download:[note:[retrieved={0} deleted={1} saved={2} conflicts={3} errors={4}] tags:[retrieved={5} saved={6} unsaved={7}]]",
					_syncResult.retrieved_notes.Count,
					_syncResult.deleted_notes.Count,
					_syncResult.saved_notes.Count,
					_syncResult.conflict_notes.Count,
					_syncResult.error_notes.Count,
					_syncResult.retrieved_tags.Count,
					_syncResult.saved_tags.Count,
					_syncResult.unsaved_tags.Count,
					upNotes.Count,
					delNotes.Count));
			}
		}

		public override void FinishSync()
		{
			_syncResult = null;
		}

		public override RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, ConflictResolutionStrategy strategy)
		{
			var note = (StandardFileNote) inote;

			if (_syncResult.saved_notes.Any(n => n.ID == note.ID))
			{
				note.ApplyUpdatedData(_syncResult.saved_notes.First(n => n.ID == note.ID));
				conflict = null;
				return RemoteUploadResult.Uploaded;
			}
			
			if (_syncResult.retrieved_notes.Any(n => n.ID == note.ID))
			{
				_logger.Warn(StandardNotePlugin.Name, "Uploaded note found in retrieved notes ... upload failed ?");
				note.ApplyUpdatedData(_syncResult.retrieved_notes.First(n => n.ID == note.ID));
				conflict = null;
				return RemoteUploadResult.Merged;
			}

			if (_syncResult.error_notes.Any(n => n.ID == note.ID))
			{
				throw new Exception("Could not upload note - server returned note in {unsaved_notes}");
			}

			if (_syncResult.conflict_notes.Any(n => n.ID == note.ID))
			{
				conflict = _syncResult.conflict_notes.First(n => n.ID == note.ID);
				return RemoteUploadResult.Conflict;
			}

			conflict = null;
			return RemoteUploadResult.UpToDate;
		}

		public override RemoteDownloadResult UpdateNoteFromRemote(INote inote)
		{
			var note = (StandardFileNote)inote;

			if (_syncResult.deleted_notes.Any(n => n.ID == note.ID))
			{
				var bucketNote = _syncResult.deleted_notes.First(n => n.ID == note.ID);

				note.ApplyUpdatedData(bucketNote);

				return RemoteDownloadResult.DeletedOnRemote;
			}

			if (_syncResult.retrieved_notes.Any(n => n.ID == note.ID))
			{
				var bucketNote = _syncResult.retrieved_notes.First(n => n.ID == note.ID);

				if (note.EqualsIgnoreModificationdate(bucketNote))
				{
					note.ApplyUpdatedData(bucketNote);
					return RemoteDownloadResult.UpToDate;
				}
				else
				{
					note.ApplyUpdatedData(bucketNote);
					return RemoteDownloadResult.Updated;
				}
			}

			return RemoteDownloadResult.UpToDate;
		}

		public override INote DownloadNote(string id, out bool success)
		{
			var n1 = _syncResult.retrieved_notes.FirstOrDefault(n => n.ID.ToString("N") == id);
			if (n1 != null)
			{
				success = true;
				return n1;
			}

			success = false;
			return null;
		}

		public override void DeleteNote(INote inote)
		{
			var note = (StandardFileNote)inote;

			if (_syncResult.deleted_notes.All(n => n.ID != note.ID))
			{
				_logger.Warn(StandardNotePlugin.Name, "Delete note returned no result - possibly an error");
			}
		}

		public override List<string> ListMissingNotes(List<INote> localnotes)
		{
			return _syncResult
				.retrieved_notes
				.Where(rn => localnotes.All(ln => ((StandardFileNote) ln).ID != rn.ID))
				.Select(p => p.ID.ToString("N"))
				.ToList();
		}

		public override bool NeedsUpload(INote note)
		{
			return !note.IsConflictNote && !note.IsRemoteSaved;
		}

		public override bool NeedsDownload(INote inote)
		{
			var note = (StandardFileNote)inote;

			if (_syncResult.retrieved_notes.Any(n => n.ID == note.ID)) return true;
			if (_syncResult.deleted_notes.Any(n => n.ID == note.ID)) return true;

			return false;
		}
	}
}
