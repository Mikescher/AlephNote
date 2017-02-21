using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlephNote.Plugins.Filesystem
{
	public class FilesystemConnection : IRemoteStorageConnection
	{
		private readonly FilesystemConfig _config;

		private List<string> _syncScan = null; 

		public FilesystemConnection(FilesystemConfig config)
		{
			_config = config;
		}

		public RemoteUploadResult UploadNoteToRemote(ref INote inote, out INote conflict, ConflictResolutionStrategy strategy)
		{
			FilesystemNote note = (FilesystemNote)inote;

			var path = note.GetPath(_config);

			if (File.Exists(note.PathRemote) && path != note.PathRemote && !File.Exists(path))
			{
				WriteNoteToPath(note, path);
				conflict = null;
				File.Delete(note.PathRemote);
				note.PathRemote = path;
				return RemoteUploadResult.Uploaded;
			}
			else if (File.Exists(note.PathRemote) && path != note.PathRemote && File.Exists(path))
			{
				var conf = ReadNoteFromPath(note.PathRemote);
				if (conf.ModificationDate != note.ModificationDate)
				{
					conflict = conf;
					if (strategy == ConflictResolutionStrategy.UseClientCreateConflictFile || strategy == ConflictResolutionStrategy.UseClientVersion)
					{
						WriteNoteToPath(note, path);
						File.Delete(note.PathRemote);
						note.PathRemote = path;
						return RemoteUploadResult.Conflict;
					}
					else
					{
						note.PathRemote = path;
						return RemoteUploadResult.Conflict;
					}
				}
				else
				{
					WriteNoteToPath(note, path);
					conflict = null;
					File.Delete(note.PathRemote);
					note.PathRemote = path;
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
						if (note.PathRemote != "") File.Delete(note.PathRemote);
						note.PathRemote = path;
						return RemoteUploadResult.Conflict;
					}
					else
					{
						note.PathRemote = path;
						return RemoteUploadResult.Conflict;
					}
				}
				else
				{
					WriteNoteToPath(note, path);
					conflict = null;
					note.PathRemote = path;
					return RemoteUploadResult.Uploaded;
				}
			}
			else
			{
				WriteNoteToPath(note, path);
				conflict = null;
				note.PathRemote = path;
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
			note.PathRemote = path;

			return RemoteDownloadResult.Updated;
		}

		public void StartSync()
		{
			_syncScan = Directory
				.EnumerateFiles(_config.Folder)
				.Where(p => (Path.GetExtension(p) ?? "").ToLower() == "." + _config.Extension.ToLower())
				.ToList();
		}

		public void FinishSync()
		{
			_syncScan = null;
		}

		public List<string> ListMissingNotes(List<INote> localnotes)
		{
			var remoteNotes = _syncScan.ToList();

			foreach (var lnote in localnotes.Cast<FilesystemNote>())
			{
				var r = remoteNotes.FirstOrDefault(p => p.ToLower() == lnote.PathRemote.ToLower());
				if (r != null) remoteNotes.Remove(r);
			}

			return remoteNotes;
		}

		public INote DownloadNote(string path, out bool result)
		{
			if (File.Exists(path))
			{
				result = true;
				return ReadNoteFromPath(path);
			}
			else
			{
				result = false;
				return null;
			}
		}

		public void DeleteNote(INote inote)
		{
			var note = (FilesystemNote) inote;

			if (note.IsConflictNote) return;

			if (File.Exists(note.PathRemote)) File.Delete(note.PathRemote);
		}

		public bool NeedsUpload(INote inote)
		{
			var note = (FilesystemNote)inote;

			if (note.IsConflictNote) return false;
			if (string.IsNullOrWhiteSpace(note.Title)) return false;

			if (!note.IsRemoteSaved) return true;
			if (string.IsNullOrWhiteSpace(note.PathRemote)) return true;
			if (!File.Exists(note.PathRemote)) return false;

			return false;
		}

		public bool NeedsDownload(INote inote)
		{
			var note = (FilesystemNote)inote;

			if (note.IsConflictNote) return false;
			if (string.IsNullOrWhiteSpace(note.Title)) return false;

			if (!note.IsRemoteSaved) return false;

			if (string.IsNullOrWhiteSpace(note.PathRemote)) return false;
			if (!File.Exists(note.PathRemote)) return true;
			
			var remote = ReadNoteFromPath(note.PathRemote);

			return remote.ModificationDate > note.ModificationDate;
		}

		private FilesystemNote ReadNoteFromPath(string path)
		{
			var info = new FileInfo(path);

			var note = new FilesystemNote(Guid.NewGuid(), _config);

			note.Title = Path.GetFileNameWithoutExtension(info.FullName);
			note.Text = File.ReadAllText(info.FullName, _config.Encoding);
			note.CreationDate = info.CreationTime;
			note.ModificationDate = info.LastWriteTime;
			note.PathRemote = info.FullName;

			return note;
		}

		private void WriteNoteToPath(FilesystemNote note, string path)
		{
			File.WriteAllText(path, note.Text);

			var info = new FileInfo(path);

			note.ModificationDate = info.LastWriteTime;
		}

	}
}