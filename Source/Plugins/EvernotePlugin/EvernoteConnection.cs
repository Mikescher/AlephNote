using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.Plugins.Evernote.EDAM.NoteStore;
using AlephNote.Plugins.Evernote.EDAM.Type;
using AlephNote.Plugins.Evernote.Thrift.Protocol;
using AlephNote.Plugins.Evernote.Thrift.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Evernote
{
	/// <summary>
	/// @ https://dev.evernote.com/doc/
	/// </summary>
	class EvernoteConnection : BasicRemoteConnection
	{
		private static readonly DateTimeOffset TIMESTAMP_ORIGIN = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

		private const string HOST_SANDBOX    = @"sandbox.evernote.com";
		private const string HOST_PRODUCTION = @"www.evernote.com";

		private const string CONSUMER_SECRET = @"2cabff412a8a7020";

		private readonly EvernoteConfig _config;
		private readonly IWebProxy _proxy;
		private readonly IAlephLogger _logger;

		private EvernoteData _data;

		private bool remoteDirty;
		private NotesMetadataList bucket;
		private NoteStore.Client nsClient;
		private string _token = null;

		public readonly HierachyEmulationConfig HConfig;

		public EvernoteConnection(IAlephLogger log, IWebProxy proxy, EvernoteConfig config, HierachyEmulationConfig hConfig)
		{
			HConfig = hConfig;

			_config = config;
			_proxy = proxy;
			_logger = log;
		}


		private void RefreshToken()
		{
			if (_token == null) _token = @"S=s1:U=936a8:E=161e6dbb8e6:C=15a8f2a8970:P=1cd:A=en-devtoken:V=2:H=8b6b427253b6b3eb6899a743a6f73244";
			                    // Notestore URL https://sandbox.evernote.com/shard/s1/notestore
		}
		
		public override void StartSync(IRemoteStorageSyncPersistance data, List<INote> localnotes, List<INote> localdeletednotes)
		{
			_data = (EvernoteData)data;

			RefreshToken();

			TTransport noteStoreTransport = new THttpClient(new Uri(@"https://sandbox.evernote.com/shard/s1/notestore"));//TODO use url from OAuth
			TProtocol noteStoreProtocol = new TBinaryProtocol(noteStoreTransport);
			nsClient = new NoteStore.Client(noteStoreProtocol);

			var state = nsClient.getSyncState(_token);

			if (_data.SyncStateUpdateCount != state.UpdateCount)
			{
				_logger.Debug(EvernotePlugin.Name, string.Format("Remote has changed SyncState: {0} -> '{1}'", _data.SyncStateUpdateCount, state.UpdateCount));

				NoteFilter filter = new NoteFilter();
				filter.Order = (int) NoteSortOrder.UPDATED;

				NotesMetadataResultSpec spec = new NotesMetadataResultSpec();
				spec.IncludeUpdateSequenceNum = true;

				bucket = nsClient.findNotesMetadata(_token, filter, 0, 9999, spec);

				_data.SyncStateUpdateCount = state.UpdateCount;
			}
			else
			{
				_logger.Debug(EvernotePlugin.Name, "Remote has not changed - no need for download - SyncState := " + state.UpdateCount);

				bucket = null;
			}

			remoteDirty = false;
		}

		public override void FinishSync()
		{
			if (remoteDirty) _data.SyncStateUpdateCount = nsClient.getSyncState(_token).UpdateCount;

			_data = null;
			bucket = null;
		}

		public override bool NeedsUpload(INote inote)
		{
			var note = (EvernoteNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return true;

			if (bucket == null) return false;

			return true;
		}

		public override bool NeedsDownload(INote inote)
		{
			var note = (EvernoteNote)inote;

			if (note.IsConflictNote) return false;

			if (!note.IsRemoteSaved) return false;

			if (bucket == null) return false;

			var remote = bucket.Notes.FirstOrDefault(p => Guid.Parse(p.Guid) == note.ID);

			if (remote == null) return false;

			return remote.UpdateSequenceNum > note.UpdateSequenceNumber;
		}

		public override List<string> ListMissingNotes(List<INote> localnotes)
		{
			if (bucket== null) return new List<string>();

			return bucket.Notes
				.Where(b => localnotes.All(p => ((EvernoteNote)p).ID != Guid.Parse(b.Guid)))
				.Select(buck => buck.Guid)
				.ToList();
		}

		public override RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, ConflictResolutionStrategy strategy)
		{
			var note = (EvernoteNote)inote;

			var remote = bucket.Notes.FirstOrDefault(p => Guid.Parse(p.Guid) == note.ID);

			remoteDirty = true;

			if (remote == null)
			{
				inote = APICreateNewNote(note);

				conflict = null;
				return RemoteUploadResult.Uploaded;
			}
			else
			{
				if (remote.UpdateSequenceNum > note.UpdateSequenceNumber)
				{
					if (strategy == ConflictResolutionStrategy.UseClientVersion || strategy == ConflictResolutionStrategy.UseClientCreateConflictFile || strategy == ConflictResolutionStrategy.ManualMerge)
					{
						conflict = APIDownloadNote(note.ID);
						inote = APIUpdateNote(note);
						return RemoteUploadResult.Conflict;
					}
					else if (strategy == ConflictResolutionStrategy.UseServerVersion || strategy == ConflictResolutionStrategy.UseServerCreateConflictFile)
					{
						conflict = inote.Clone();
						inote = APIDownloadNote(note.ID);
						return RemoteUploadResult.Conflict;
					}
					else
					{
						throw new ArgumentException("strategy == " + strategy);
					}
				}
				else
				{
					inote = APIUpdateNote(note);

					conflict = null;
					return RemoteUploadResult.Uploaded;
				}
			}
		}

		public override RemoteDownloadResult UpdateNoteFromRemote(INote inote)
		{
			var note = (EvernoteNote)inote;

			var remote = bucket.Notes.FirstOrDefault(p => Guid.Parse(p.Guid) == note.ID);

			if (remote == null) return RemoteDownloadResult.DeletedOnRemote;

			if (remote.UpdateSequenceNum == note.UpdateSequenceNumber) return RemoteDownloadResult.UpToDate;

			var unote = APIDownloadNote(note.ID);

			note.ApplyUpdatedData(unote);

			return RemoteDownloadResult.Updated;
		}

		public override INote DownloadNote(string id, out bool success)
		{
			var d = APIDownloadNote(Guid.Parse(id));

			if (d == null)
			{
				success = false;
				return null;
			}

			success = true;
			return d;
		}

		public override void DeleteNote(INote inote)
		{
			var note = (EvernoteNote)inote;

			if (note.IsConflictNote) return;

			remoteDirty = true;
			nsClient.deleteNote(_token, note.ID.ToString("D"));
		}

		private EvernoteNote APIUpdateNote(EvernoteNote note)
		{
			remoteDirty = true;

			var remoteNote = new Note();
			remoteNote.Guid = note.ID.ToString("D");
			remoteNote.Title = note.InternalTitle;
			remoteNote.Content = note.CreateENML();
			remoteNote.TagNames = note.Tags.ToList();
			remoteNote.Updated = ConvertToEpochDate(note.ModificationDate);

			var updatedNote = nsClient.updateNote(_token, remoteNote);

			if (updatedNote.__isset.tagGuids) note.Tags.Synchronize(ConvertTagsFromUUID(updatedNote.TagGuids));
			if (updatedNote.__isset.updateSequenceNum) note.UpdateSequenceNumber = updatedNote.UpdateSequenceNum;
			if (updatedNote.__isset.content) note.SetTextFromENML(updatedNote.Content);
			if (updatedNote.__isset.title) note.InternalTitle = updatedNote.Title;
			if (updatedNote.__isset.updated) note.ModificationDate = ConvertFromEpochDate(updatedNote.Updated);
			if (updatedNote.__isset.created) note.CreationDate = ConvertFromEpochDate(updatedNote.Created);

			return note;
		}

		private EvernoteNote APICreateNewNote(EvernoteNote note)
		{
			remoteDirty = true;

			var remoteNote = new Note();
			remoteNote.Guid = note.ID.ToString("D");
			remoteNote.Title = note.InternalTitle;
			remoteNote.Content = note.CreateENML();
			remoteNote.TagNames = note.Tags.ToList();
			remoteNote.Created = ConvertToEpochDate(note.CreationDate);
			remoteNote.Updated = ConvertToEpochDate(note.ModificationDate);

			var createdNote = nsClient.createNote(_token, remoteNote);

			if (createdNote.__isset.tagGuids) note.Tags.Synchronize(ConvertTagsFromUUID(createdNote.TagGuids));
			if (createdNote.__isset.updateSequenceNum) note.UpdateSequenceNumber = createdNote.UpdateSequenceNum;
			if (createdNote.__isset.content) note.SetTextFromENML(createdNote.Content);
			if (createdNote.__isset.title) note.InternalTitle = createdNote.Title;
			if (createdNote.__isset.updated) note.ModificationDate = ConvertFromEpochDate(createdNote.Updated);
			if (createdNote.__isset.created) note.CreationDate = ConvertFromEpochDate(createdNote.Created);

			return note;
		}

		private EvernoteNote APIDownloadNote(Guid id)
		{
			var remote = nsClient.getNote(_token, id.ToString("D"), true, false, false, false);

			var note = new EvernoteNote(Guid.Parse(remote.Guid), _config, HConfig);

			if (remote.__isset.tagGuids) note.Tags.Synchronize(ConvertTagsFromUUID(remote.TagGuids));
			if (remote.__isset.updateSequenceNum) note.UpdateSequenceNumber = remote.UpdateSequenceNum;
			if (remote.__isset.content) note.SetTextFromENML(remote.Content);
			if (remote.__isset.title) note.InternalTitle = remote.Title;
			if (remote.__isset.updated) note.ModificationDate = ConvertFromEpochDate(remote.Updated);
			if (remote.__isset.created) note.CreationDate = ConvertFromEpochDate(remote.Created);

			return note;
		}

		private IEnumerable<string> ConvertTagsFromUUID(List<string> tagGuids)
		{
			if (tagGuids == null) return new List<string>();

			return tagGuids.Select(TagGuidToName).ToList();
		}

		private string TagGuidToName(string uuid)
		{
			var guid = Guid.Parse(uuid);

			var localTag = _data.Tags.FirstOrDefault(t => t.UUID == guid);
			if (localTag != null) return localTag.Title;

			var remoteTag = nsClient.getTag(_token, uuid);
			_data.Tags.Add(new EvernoteTagRef(guid, remoteTag.Name));

			_logger.Debug(EvernotePlugin.Name, string.Format("Downloaded tag mapping {0} -> '{1}'", remoteTag.Guid, remoteTag.Name));

			return remoteTag.Name;
		}

		private static DateTimeOffset ConvertFromEpochDate(long seconds)
		{
			if (seconds <= 0) return TIMESTAMP_ORIGIN;

			return TIMESTAMP_ORIGIN.AddSeconds(seconds);
		}

		private static long ConvertToEpochDate(DateTimeOffset offset)
		{
			return (long)offset.DateTime.ToUniversalTime().Subtract(TIMESTAMP_ORIGIN.DateTime).TotalSeconds;
		}
	}
}
