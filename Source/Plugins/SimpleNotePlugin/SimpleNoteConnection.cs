﻿using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AlephNote.PluginInterface.Exceptions;

namespace AlephNote.Plugins.SimpleNote
{
	class SimpleNoteConnection : BasicRemoteConnection
	{
		private const string HOST_AUTH = @"https://auth.simperium.com/1/chalk-bump-f49/";
		private const string HOST_API  = @"https://api.simperium.com/1/chalk-bump-f49/";

		private const string API_KEY = "6ebfbdf6bfa8423e85d8733f6b6bbc25";

		private readonly SimpleNoteConfig _config;
		private readonly IWebProxy _proxy;
		private readonly AlephLogger _logger;

		private SimpleNoteAPI.APIResultAuthorize _token = null;
		private SimpleNoteAPI.APIResultIndex buckets = null;

		private SimpleNoteData _data;

		public readonly HierarchyEmulationConfig HConfig;

		public SimpleNoteConnection(AlephLogger log, IWebProxy proxy, SimpleNoteConfig config, HierarchyEmulationConfig hConfig)
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
					using (var web = CreateJsonRestClient(_proxy, HOST_AUTH))
					{
						web.AddHeader("X-Simperium-API-Key", API_KEY);
						web.SetEscapeAllNonASCIICharacters(true);

						_logger.Debug(SimpleNotePlugin.Name, "Requesting token from Simplenote server");
						_token = SimpleNoteAPI.Authenticate(web, _config.Username, _config.Password);
						_logger.Debug(SimpleNotePlugin.Name, "Simplenote server returned token for user " + _token.userid);
					}
				}
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new Exception("Could not authenticate with SimpleNote server : " + e.Message, e);
			}
		}

		private ISimpleJsonRest CreateAuthenticatedClient()
		{
			RefreshToken();

			var client = CreateJsonRestClient(_proxy, HOST_API);
			client.AddHeader("X-Simperium-API-Key", API_KEY);
			client.AddHeader("X-Simperium-Token", _token.access_token);
			client.SetEscapeAllNonASCIICharacters(true);

			return client;
		}

		public override void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes, List<INote> localdeletednotes)
		{
			_data = (SimpleNoteData) data;

			using (var web = CreateAuthenticatedClient())
			{
				buckets = SimpleNoteAPI.ListBuckets(web);

				_logger.Debug(
					SimpleNotePlugin.Name, 
					string.Format("SimpleNoteAPI.ListBuckets returned {0} elements", buckets.index.Count),
					string.Join(Environment.NewLine, buckets.index.Select(b => b.id + " ("+b.v+")")));
			}
		}

		public override void FinishSync(out bool immediateResync)
		{
			buckets = null;
			immediateResync = false;
		}

		public override bool NeedsUpload(INote inote)
		{
			var note = (SimpleNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return true;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null) return true;

			return remote.v < note.LocalVersion;
		}

		public override bool NeedsDownload(INote inote)
		{
			var note = (SimpleNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return false;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null) return false;

			return remote.v > note.LocalVersion;
		}

		public override INote DownloadNote(string id, out bool success)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var d = SimpleNoteAPI.GetNoteData(web, id, _config, this);

				if (d.Deleted)
				{
					_data.AddDeletedNote(d.ID, d.LocalVersion);
					success = false;
					return null;
				}

				_data.RemoveDeletedNote(d.ID);

				success = true;
				return d;
			}
		}

		public override void DeleteNote(INote inote)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var note = (SimpleNote) inote;

				if (note.IsConflictNote) return;

				var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

				if (remote != null)
				{
					if (_config.PermanentlyDeleteNotes)
					{
						SimpleNoteAPI.DeleteNote(web, note);
						SimpleNoteAPI.DeleteNotePermanently(web, note);
					}
					else
					{
						SimpleNoteAPI.DeleteNote(web, note);
					}

					_data.AddDeletedNote(note.ID, note.LocalVersion);
				}
			}
		}

		public override List<string> ListMissingNotes(List<INote> localnotes)
		{
			return buckets.index
				.Where(b => localnotes.All(p => ((SimpleNote) p).ID != b.id))
				.Where(b => !_data.IsDeleted(b.id, b.v))
				.Select(buck => buck.id)
				.ToList();
		}

		public override RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, out bool keepNoteRemoteDirtyWithConflict, ConflictResolutionStrategy strategy)
		{
			keepNoteRemoteDirtyWithConflict = false;

			using (var web = CreateAuthenticatedClient())
			{
				var note = (SimpleNote) inote;

				var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

				if (remote == null)
				{
					conflict = null;
					inote = SimpleNoteAPI.UploadNewNote(web, note, _config, this);
					return RemoteUploadResult.Uploaded;
				}
				else
				{
					if (remote.v > note.LocalVersion)
					{
						if (strategy == ConflictResolutionStrategy.UseClientVersion || strategy == ConflictResolutionStrategy.UseClientCreateConflictFile || strategy == ConflictResolutionStrategy.ManualMerge)
						{
							conflict = SimpleNoteAPI.GetNoteData(web, note.ID, _config, this);
							inote = SimpleNoteAPI.ChangeExistingNote(web, note, _config, this, out _);
							return RemoteUploadResult.Conflict;
						}
						else if (strategy == ConflictResolutionStrategy.UseServerVersion || strategy == ConflictResolutionStrategy.UseServerCreateConflictFile)
						{
							conflict = inote.Clone();
							inote = SimpleNoteAPI.GetNoteData(web, note.ID, _config, this);
							return RemoteUploadResult.Conflict;
						}
						else
						{
							throw new ArgumentException("strategy == " + strategy);
						}
					}
					else
					{
						conflict = null;
						bool updated;
						inote = SimpleNoteAPI.ChangeExistingNote(web, note, _config, this, out updated);
						return updated ? RemoteUploadResult.Uploaded : RemoteUploadResult.UpToDate;
					}
				}
			}
		}

		public override RemoteDownloadResult UpdateNoteFromRemote(INote inote)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var note = (SimpleNote) inote;

				var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

				if (remote == null) return RemoteDownloadResult.DeletedOnRemote;

				if (remote.v == note.LocalVersion) return RemoteDownloadResult.UpToDate;

				var unote = SimpleNoteAPI.GetNoteData(web, note.ID, _config, this);
				if (unote.Deleted) return RemoteDownloadResult.DeletedOnRemote;

				inote.ApplyUpdatedData(unote);

				return RemoteDownloadResult.Updated;
			}
		}
	}
}
