using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemConnection : IRemoteStorageConnection
	{
		private readonly FilesystemConfig _config;

		public FilesystemConnection(FilesystemConfig config)
		{
			_config = config;
		}

		public RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, ConflictResolutionStrategy strategy)
		{
			FilesystemNote note = (FilesystemNote)inote;

			var path = note.GetPath(_config);

			if (File.Exists(note.PathBackup) && path != note.PathBackup)
			{
				var conf = ReadNoteFromPath(note.PathBackup);
				if (conf.ModificationDate != note.ModificationDate)
				{
					conflict = conf;
					if (strategy == ConflictResolutionStrategy.UseClientCreateConflictFile || strategy == ConflictResolutionStrategy.UseClientVersion)
					{
						WriteNoteToPath(note, path);
						File.Delete(note.PathBackup);
						return RemoteUploadResult.Conflict;
					}
					else
					{
						return RemoteUploadResult.Conflict;
					}
				}
				else
				{
					WriteNoteToPath(note, path);
					conflict = null;
					File.Delete(note.PathBackup);
					return RemoteUploadResult.Uploaded;
				}
			}
			else if (File.Exists(path))
			{
				var conf = ReadNoteFromPath(path);
				if (conf.ModificationDate != note.ModificationDate)
				{
					conflict = conf;
					if (strategy == ConflictResolutionStrategy.UseClientCreateConflictFile || strategy == ConflictResolutionStrategy.UseClientVersion)
					{
						WriteNoteToPath(note, path);
						File.Delete(note.PathBackup);
						return RemoteUploadResult.Conflict;
					}
					else
					{
						return RemoteUploadResult.Conflict;
					}
				}
				else
				{
					WriteNoteToPath(note, path);
					conflict = null;
					return RemoteUploadResult.Uploaded;
				}
			}
			else
			{
				WriteNoteToPath(note, path);
				conflict = null;
				return RemoteUploadResult.Uploaded;
			}
		}

		public RemoteDownloadResult UpdateNoteFromRemote(INote inote)
		{
			FilesystemNote note = (FilesystemNote) inote;

			var path = note.GetPath(_config);

			if (!File.Exists(path)) return RemoteDownloadResult.DeletedOnRemote;

			note.Title = Path.GetFileNameWithoutExtension(path);
			note.Text = File.ReadAllText(path, _config.Encoding);
			note.PathBackup = path;

			return RemoteDownloadResult.Updated;
		}

		public void StartSync()
		{
			//
		}

		public void FinishSync()
		{
			//
		}

		public INote DownloadNote(string id, out bool result)
		{
			throw new System.NotImplementedException();
		}

		public void DeleteNote(INote note)
		{
			throw new System.NotImplementedException();
		}

		public List<string> ListMissingNotes(List<INote> localnotes)
		{
			throw new System.NotImplementedException();
		}

		public bool NeedsUpload(INote note)
		{
			throw new System.NotImplementedException();
		}

		public bool NeedsDownload(INote note)
		{
			throw new System.NotImplementedException();
		}

		private FilesystemNote ReadNoteFromPath(string path)
		{
			var info = new FileInfo(path);

			var note = new FilesystemNote(Guid.NewGuid(), _config);

			note.Title = Path.GetFileNameWithoutExtension(info.FullName);
			note.Text = File.ReadAllText(info.FullName, _config.Encoding);
			note.CreationDate = info.CreationTime;
			note.ModificationDate = info.LastWriteTime;
			note.PathBackup = info.FullName;

			return note;
		}

		private void WriteNoteToPath(FilesystemNote note, string path)
		{
			File.WriteAllText(note.Text, path);

			var info = new FileInfo(path);

			note.ModificationDate = info.LastWriteTime;
		}

	}
}