using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AlephNote.Plugins.Nextcloud
{
	/// <summary>
	/// https://github.com/nextcloud/notes/wiki/API-0.2
	/// </summary>
	class NextcloudConnection : BasicRemoteConnection
	{
		private const string API_URL = "/index.php/apps/notes/api/v0.2/";

		private readonly NextcloudConfig _config;
		private readonly IWebProxy _proxy;
		private readonly IAlephLogger _logger;

		private NextcloudData _data;

		private List<NextcloudAPI.NoteRef> remoteNotes; 

		public NextcloudConnection(IAlephLogger log, IWebProxy proxy, NextcloudConfig config)
		{
			_config = config;
			_proxy = proxy;
			_logger = log;
		}
		private ISimpleJsonRest CreateAuthenticatedClient()
		{
			var client = CreateJsonRestClient(_proxy, new Uri(new Uri(_config.Host), API_URL).ToString());
			//client.SetURLAuthentication(_config.Username, _config.Password); // specs say this works - but it doesn't
			client.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.Username + ":" + _config.Password)));
			client.SetEscapeAllNonASCIICharacters(true);

			return client;
		}

		public override void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes, List<INote> localdeletednotes)
		{
			_data = (NextcloudData)data;

			using (var web = CreateAuthenticatedClient())
			{
				remoteNotes = NextcloudAPI.ListNotes(web);

				_logger.Debug(NextcloudPlugin.Name, string.Format("NextcloudAPI.ListNotes returned {0} elements", remoteNotes.Count));
			}
		}

		public override void FinishSync()
		{
			_data = null;
			remoteNotes = null;
		}

		public override bool NeedsUpload(INote inote)
		{
			var note = (NextcloudNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return true;

			var remote = remoteNotes.FirstOrDefault(p => p.id == note.RemoteID);

			if (remote == null) return true;

			return note.RemoteTimestamp == -1 || remote.modified < note.RemoteTimestamp;
		}

		public override bool NeedsDownload(INote inote)
		{
			var note = (NextcloudNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return false;

			var remote = remoteNotes.FirstOrDefault(p => p.id == note.RemoteID);

			if (remote == null) return false;

			return remote.modified > note.RemoteTimestamp && (note.RemoteTimestamp != -1);
		}

		public override List<string> ListMissingNotes(List<INote> localnotes)
		{
			return remoteNotes
				.Where(b => localnotes.All(p => ((NextcloudNote)p).RemoteID != b.id))
				.Select(buck => buck.id.ToString())
				.ToList();
		}

		public override RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, ConflictResolutionStrategy strategy)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var note = (NextcloudNote)inote;

				var remote = remoteNotes.FirstOrDefault(p => p.id == note.RemoteID);

				if (remote == null)
				{
					conflict = null;
					inote = NextcloudAPI.UploadNewNote(web, note, _config);
					return RemoteUploadResult.Uploaded;
				}
				else
				{
					if (remote.modified > note.RemoteTimestamp)
					{
						if (strategy == ConflictResolutionStrategy.UseClientVersion || strategy == ConflictResolutionStrategy.UseClientCreateConflictFile || strategy == ConflictResolutionStrategy.ManualMerge)
						{
							conflict = NextcloudAPI.GetNoteData(web, note.RemoteID, _config);
							inote = NextcloudAPI.ChangeExistingNote(web, note, _config);
							return RemoteUploadResult.Conflict;
						}
						else if (strategy == ConflictResolutionStrategy.UseServerVersion || strategy == ConflictResolutionStrategy.UseServerCreateConflictFile)
						{
							conflict = inote.Clone();
							inote = NextcloudAPI.GetNoteData(web, note.RemoteID, _config);
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
						inote = NextcloudAPI.ChangeExistingNote(web, note, _config);
						return RemoteUploadResult.Uploaded;
					}
				}
			}
		}

		public override RemoteDownloadResult UpdateNoteFromRemote(INote inote)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var note = (NextcloudNote)inote;

				var remote = remoteNotes.FirstOrDefault(p => p.id == note.RemoteID);

				if (remote == null) return RemoteDownloadResult.DeletedOnRemote;

				if (remote.modified == note.RemoteTimestamp) return RemoteDownloadResult.UpToDate;

				var unote = NextcloudAPI.GetNoteData(web, note.RemoteID, _config);

				inote.ApplyUpdatedData(unote);

				return RemoteDownloadResult.Updated;
			}
		}

		public override INote DownloadNote(string id, out bool success)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var d = NextcloudAPI.GetNoteData(web, int.Parse(id), _config);
				success = true;
				return d;
			}
		}

		public override void DeleteNote(INote inote)
		{
			using (var web = CreateAuthenticatedClient())
			{
				var note = (NextcloudNote)inote;

				if (note.IsConflictNote) return;

				var remote = remoteNotes.FirstOrDefault(p => p.id == note.RemoteID);

				if (remote != null) NextcloudAPI.DeleteNote(web, note);
			}
		}
	}
}
