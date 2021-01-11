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
		private readonly AlephLogger _logger;

		private StandardNoteAPI.SyncResult _syncResult = null;
		private bool _immediateResync = false;
		private List<Guid> _lastUploadBatch = new List<Guid>();

		public readonly HierarchyEmulationConfig HConfig;

		public StandardNoteConnection(AlephLogger log, IWebProxy proxy, StandardNoteConfig config, HierarchyEmulationConfig hConfig)
		{
			HConfig = hConfig;

			_config = config;
			_proxy = proxy;
			_logger = log;
		}

		private void RefreshToken(StandardNoteData dat)
		{
			try
			{
				if (dat.SessionData == null || dat.SessionData.AccessExpiration > DateTime.Now) dat.SessionData = null;

				if (dat.SessionData != null) return;

				using (var web = CreateJsonRestClient(_proxy, _config.Server))
				{
					_logger.Debug(StandardNotePlugin.Name, "Requesting token from StandardNoteServer");

					dat.SessionData = StandardNoteAPI.Authenticate(web, _config.Email, _config.Password, _logger);

					_logger.Debug(StandardNotePlugin.Name, $"StandardNoteServer returned token \"{dat.SessionData.Token}\" (until {dat.SessionData.AccessExpiration:yyyy-MM-dd HH:mm:ss})");
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

		private ISimpleJsonRest CreateAuthenticatedClient(StandardNoteData dat)
		{
			RefreshToken(dat);

			var client = CreateJsonRestClient(_proxy, _config.Server);
			client.AddHeader("Authorization", "Bearer " + dat.SessionData.Token);
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

			_immediateResync = false;

			var data = (StandardNoteData)idata;

			using (var web = CreateAuthenticatedClient(data))
			{
				var localnotes = ilocalnotes.Cast<StandardFileNote>().ToList();

				var upNotes = localnotes.Where(NeedsUploadReal).ToList();
				var delNotes = localdeletednotes.Cast<StandardFileNote>().ToList();
				var delTags = data.GetUnusedTags(localnotes.ToList());

				_syncResult = StandardNoteAPI.Sync(web, this, _config, data, localnotes, upNotes, delNotes, delTags);
				_lastUploadBatch = upNotes.Select(p => p.ID).ToList();

				_logger.Debug(StandardNotePlugin.Name, "StandardFile sync finished.",
					$"upload:\n" +
					$"[\n" +
					$"    notes   = {upNotes.Count}\n" +
					$"    deleted = {delNotes.Count}\n" +
					$"]\n"+
					$"\n" +
					$"download:\n"+
					$"[\n"+
					$"    note:\n" +
					$"    [\n" +
					$"        retrieved      = {_syncResult.retrieved_notes.Count}\n" +
					$"        deleted        = {_syncResult.deleted_notes.Count}\n" +
					$"        saved          = {_syncResult.saved_notes.Count}\n" +
					$"        sync_conflicts = {_syncResult.syncconflict_notes.Count}\n" +
					$"        uuid_conflicts = {_syncResult.uuidconflict_notes.Count}\n" +
					$"    ]\n" +
					$"    tags:\n" +
					$"    [\n" +
					$"        retrieved      = {_syncResult.retrieved_tags.Count}\n" +
					$"        deleted        = {_syncResult.deleted_tags.Count}\n" +
					$"        saved          = {_syncResult.saved_tags.Count}\n" +
					$"        sync_conflicts = {_syncResult.syncconflict_tags.Count}\n" +
					$"        uuid_conflicts = {_syncResult.uuidconflict_tags.Count}\n" +
					$"    ]\n" +
					$"    items_keys:\n" +
					$"    [\n" +
					$"        retrieved      = {_syncResult.retrieved_keys.Count}\n" +
					$"        deleted        = {_syncResult.deleted_keys.Count}\n" +
					$"        saved          = {_syncResult.saved_keys.Count}\n" +
					$"        sync_conflicts = {_syncResult.syncconflict_keys.Count}\n" +
					$"        uuid_conflicts = {_syncResult.uuidconflict_keys.Count}\n" +
					$"    ]\n" +
					$"]");
			}
		}

		public override void FinishSync(out bool immediateResync)
		{
			immediateResync = _immediateResync;

			_syncResult = null;
		}

		public override RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, out bool keepNoteRemoteDirtyWithConflict, ConflictResolutionStrategy strategy)
		{
			var note = (StandardFileNote) inote;
			keepNoteRemoteDirtyWithConflict = false;

			var item_saved = _syncResult.saved_notes.FirstOrDefault(n => n.ID == note.ID);
			if (item_saved != null)
			{
				note.ApplyUpdatedData(item_saved);
				conflict = null;
				return RemoteUploadResult.Uploaded;
			}

			var item_retrieved = _syncResult.retrieved_notes.FirstOrDefault(n => n.ID == note.ID);
			if (item_retrieved != null)
			{
				_logger.Warn(StandardNotePlugin.Name, "Uploaded note found in retrieved notes ... upload failed ?");
				note.ApplyUpdatedData(item_retrieved);
				conflict = null;
				return RemoteUploadResult.Merged;
			}

			var item_syncconflict = _syncResult.syncconflict_notes.FirstOrDefault(n => n.servernote != null && n.servernote.ID == note.ID);
			if (item_syncconflict != default)
			{
				var conf = item_syncconflict.servernote;

				if (strategy == ConflictResolutionStrategy.UseClientVersion || strategy == ConflictResolutionStrategy.UseClientCreateConflictFile)
                {
					conflict = conf;

					// note needs to be uploaded again, because this time server returned [conflict] and did not apply data
					keepNoteRemoteDirtyWithConflict = true;

					// update mdate, so we don't get a conflict next time and we upload to new (client) note
					note.RawModificationDate = item_syncconflict.servernote.ModificationDate;

					// trigger another sync after this one (to upload actual/new client note)
					_immediateResync = true;

					return RemoteUploadResult.Conflict;
				}
				else if (strategy == ConflictResolutionStrategy.UseServerVersion || strategy == ConflictResolutionStrategy.UseServerCreateConflictFile)
				{
					conflict = inote;
					inote = conf;

					_immediateResync = false;

					return RemoteUploadResult.Conflict;
				}
				else if (strategy == ConflictResolutionStrategy.ManualMerge)
				{
					conflict = inote;
					inote = conf;

					_immediateResync = false;

					return RemoteUploadResult.Conflict;
				}
				else
				{
					throw new Exception("Unknown ConflictResolutionStrategy");
				}
			}

			var item_uuidconflict = _syncResult.uuidconflict_notes.FirstOrDefault(n => n.servernote != null && n.servernote.ID == note.ID);
			if (item_uuidconflict != default)
			{
				throw new Exception($"Could not upload note {note.UniqueName} due to an UUID conflict"); // you're fucked!
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

				if (note.NoteDataEquals(bucketNote))
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
			return _lastUploadBatch.Contains(((StandardFileNote)note).ID);
		}
		
		private bool NeedsUploadReal(INote note)
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
