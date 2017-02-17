using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AlephNote.Plugins.SimpleNote
{
	class SimpleNoteConnection : IRemoteStorageConnection
	{
		private readonly SimpleNoteConfig _config;
		private readonly IWebProxy _proxy;

		private SimpleNoteAPI.APIResultAuthorize _token = null;
		private SimpleNoteAPI.APIResultIndex buckets = null;

		private HashSet<string> deletedNotesCache = new HashSet<string>(); 

		public SimpleNoteConnection(IWebProxy proxy, SimpleNoteConfig config)
		{
			_config = config;
			_proxy = proxy;
		}

		private void RefreshToken()
		{
			try
			{
				if (_token == null)
					_token = SimpleNoteAPI.Authenticate(_proxy, _config.Username, _config.Password);
			}
			catch (Exception e)
			{
				throw new Exception("Could not authenticate with SimpleNote server : " + e.Message, e);
			}
		}

		public void StartSync()
		{
			RefreshToken();

			buckets = SimpleNoteAPI.ListBuckets(_proxy, _token.access_token);
		}

		public void FinishSync()
		{
			buckets = null;
		}

		public bool NeedsUpload(INote inote)
		{
			var note = (SimpleNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return true;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null) return true;

			return remote.v < note.LocalVersion;
		}

		public bool NeedsDownload(INote inote)
		{
			var note = (SimpleNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return false;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null) return false;

			return remote.v > note.LocalVersion;
		}

		public INote DownloadNote(string id, out bool result)
		{
			RefreshToken();

			var d = SimpleNoteAPI.GetNoteData(_proxy, _token.access_token, id, _config);

			if (d.Deleted)
			{
				deletedNotesCache.Add(d.ID);
				result = false;
				return null;
			}

			result = true;
			return d;
		}

		public void DeleteNote(INote inote)
		{
			RefreshToken();

			var note = (SimpleNote)inote;
			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote != null)
			{
				if (_config.PermanentlyDeleteNotes)
					SimpleNoteAPI.DeleteNotePermanently(_proxy, _token.access_token, note);
				else
					SimpleNoteAPI.DeleteNote(_proxy, _token.access_token, note);

				deletedNotesCache.Add(note.ID);
			}
		}

		public List<string> ListMissingNotes(List<INote> localnotes)
		{
			return buckets.index
				.Where(b => localnotes.All(p => ((SimpleNote) p).ID != b.id))
				.Where(b => !deletedNotesCache.Contains(b.id))
				.Select(buck => buck.id)
				.ToList();
		}

		public RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, ConflictResolutionStrategy strategy)
		{
			RefreshToken();
			
			var note = (SimpleNote)inote;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null)
			{
				conflict = null;
				inote = SimpleNoteAPI.UploadNewNote(_proxy, _token.access_token, note, _config);
				return RemoteUploadResult.Uploaded;
			}
			else
			{
				if (remote.v > note.LocalVersion)
				{
					if (strategy == ConflictResolutionStrategy.UseClientVersion || strategy == ConflictResolutionStrategy.UseClientCreateConflictFile)
					{
						bool tmp;
						conflict = SimpleNoteAPI.GetNoteData(_proxy, _token.access_token, note.ID, _config);
						inote = SimpleNoteAPI.ChangeExistingNote(_proxy, _token.access_token, note, _config, out tmp);
						return RemoteUploadResult.Conflict;
					}
					else if (strategy == ConflictResolutionStrategy.UseServerVersion || strategy == ConflictResolutionStrategy.UseServerCreateConflictFile)
					{
						conflict = inote.Clone();
						inote = SimpleNoteAPI.GetNoteData(_proxy, _token.access_token, note.ID, _config);
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
					inote = SimpleNoteAPI.ChangeExistingNote(_proxy, _token.access_token, note, _config, out updated);
					return updated ? RemoteUploadResult.Uploaded : RemoteUploadResult.UpToDate;
				}
			}
		}

		public RemoteDownloadResult UpdateNoteFromRemote(INote inote)
		{
			RefreshToken();

			var note = (SimpleNote)inote;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null) return RemoteDownloadResult.DeletedOnRemote;

			if (remote.v == note.LocalVersion) return RemoteDownloadResult.UpToDate;

			var unote = SimpleNoteAPI.GetNoteData(_proxy, _token.access_token, note.ID, _config);
			if (unote.Deleted) return RemoteDownloadResult.DeletedOnRemote;

			inote.ApplyUpdatedData(unote);

			return RemoteDownloadResult.Updated;
		}
	}
}
