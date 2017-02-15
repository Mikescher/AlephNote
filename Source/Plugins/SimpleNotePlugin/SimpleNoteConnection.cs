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
					_token = SimpleNoteAPI.Authenticate(_proxy, _config.SimpleNoteUsername, _config.SimpleNotePassword);
			}
			catch (Exception e)
			{
				throw new Exception("Could not authenticate with SimpleNote server : " + e.Message, e);
			}
		}

		public void StartNewSync()
		{
			RefreshToken();

			buckets = SimpleNoteAPI.ListBuckets(_proxy, _token.access_token);
		}

		public void FinishNewSync()
		{
			buckets = null;
		}

		public INote DownloadNote(string id, out bool result)
		{
			RefreshToken();

			var d = SimpleNoteAPI.GetNoteData(_proxy, _token.access_token, id);

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

		public INote UploadNote(INote inote)
		{
			RefreshToken();
			
			var note = (SimpleNote)inote;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null)
			{
				return SimpleNoteAPI.ChangeNote(_proxy, _token.access_token, note);
			}
			else
			{
				return SimpleNoteAPI.UploadNote(_proxy, _token.access_token, note);
			}
		}

		public RemoteResult UpdateNote(INote inote)
		{
			RefreshToken();

			var note = (SimpleNote)inote;

			var remote = buckets.index.FirstOrDefault(p => p.id == note.ID);

			if (remote == null) return RemoteResult.DeletedOnRemote;

			if (remote.v == note.Version) return RemoteResult.UpToDate;

			var unote = SimpleNoteAPI.GetNoteData(_proxy, _token.access_token, note.ID);
			if (unote.Deleted) return RemoteResult.DeletedOnRemote;

			inote.ApplyUpdatedData(unote);

			return RemoteResult.Updated;
		}
	}
}
