using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Threading;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AlephNote.Common.Operations
{
	public class RawFolderRepository
	{
		private class PathEntry { public string FilesystemPath; public DirectoryPath NotePath; public string Title; public DateTimeOffset MDate; public string Content; }

		private          bool _enabled;
		private readonly string _path;

		private readonly Encoding _encoding;
		private readonly bool _filewatcherEnabled;
		private readonly int _searchDepth;
		
		private readonly bool _syncModifications; // sync FS modifications to NR
		private readonly bool _syncCreation;      // sync FS creations     to NR
		private readonly bool _syncDeletion;      // sync FS deletions     to NR

		private readonly NoteRepository _repo;
		
		private readonly DelayedCombiningInvoker _invSyncRequest;

		private DateTimeOffset? _lastSync = null;
		private FileSystemWatcher _watcher;

		private bool _isSyncing = false;

		private List<Tuple<string, DateTimeOffset>> _repoDeletedNotes = new List<Tuple<string, DateTimeOffset>>();

		public RawFolderRepository(NoteRepository r, IAlephDispatcher _disp, AppSettings s)
		{
			_repo = r;
			_invSyncRequest = DelayedCombiningInvoker.Create(() => _disp.BeginInvoke(SyncNow), 5 * 1000, 30 * 1000);

			_enabled = s.UseRawFolderRepo;
			_path = s.RawFolderRepoPath;

			_filewatcherEnabled = s.RawFolderRepoUseFileWatcher;
			_encoding = EncodingEnumHelper.ToEncoding(s.RawFolderRepoEncoding);
			_searchDepth = s.RawFolderRepoMaxDirectoryDepth;

			_syncModifications = s.RawFolderRepoAllowModification;
			_syncCreation      = s.RawFolderRepoAllowCreation;
			_syncDeletion      = s.RawFolderRepoAllowDeletion;
		}

		public void Start()
		{
			if (!_enabled) return;

			LoggerSingleton.Inst.Debug("RawFolderSync", $"Startup local folder repository in directory '{_path}'");

			try
			{
				if (string.IsNullOrWhiteSpace(_path)) throw new Exception("Path for local folder sync cannot be empty");

				Directory.CreateDirectory(_path);

				if (_filewatcherEnabled)
				{
					_watcher = new FileSystemWatcher(_path);
					_watcher.Changed += WatcherOnChanged;
					_watcher.Created += WatcherOnChanged;
					_watcher.Deleted += WatcherOnChanged;
					_watcher.IncludeSubdirectories = true;
					_watcher.EnableRaisingEvents = true;
				}

				SyncNow();
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository initialization failed", e);
				_enabled = false;
			}
		}

		private void WatcherOnChanged(object sender, FileSystemEventArgs e)
		{
			if (_isSyncing) return;

			_invSyncRequest.Request();
			LoggerSingleton.Inst.Debug("RawFolderSync", $"Filewatcher triggered with {e.ChangeType} for '{e.Name}'", e.FullPath);
		}

		public void Shutdown()
		{
			if (!_enabled) return;

			LoggerSingleton.Inst.Debug("RawFolderSync", $"Local folder repository shutdown ('{_path}')");
			
			try
			{
				_invSyncRequest.CancelPendingRequests();
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository shutdown failed", e);
				_enabled = false;
			}
		}

		private IEnumerable<PathEntry> EnumerateFolder(DirectoryPath dsrcpath, string fsrcpath, int depth)
		{
			if (depth >= _searchDepth) yield break;

			foreach (var filepath in Directory.EnumerateFiles(fsrcpath))
			{
				if (!filepath.ToLower().EndsWith(".txt") && !filepath.ToLower().EndsWith(".md")) continue;

				yield return new PathEntry
				{
					FilesystemPath = filepath,
					NotePath = dsrcpath,
					MDate = new DateTimeOffset(new FileInfo(filepath).LastWriteTime),
					Title = FilenameHelper.ConvertStringFromFilenameBack(Path.GetFileNameWithoutExtension(filepath)),
					Content = File.ReadAllText(filepath, _encoding),
				};
			}

			foreach (var dir in Directory.EnumerateDirectories(fsrcpath))
			{
				var d = dsrcpath.SubDir(FilenameHelper.ConvertStringFromFilenameBack(Path.GetFileName(dir)));

				foreach (var item in EnumerateFolder(d, dir, depth+1)) yield return item;
			}
		}

		public void SyncNow() // run in dispatcher thread
		{
			if (!_enabled) return;

			try
			{
				_isSyncing = true;

				List<Tuple<string, DateTimeOffset>> deletes;
				lock (_repoDeletedNotes)
				{
					deletes = _repoDeletedNotes.ToList();
					_repoDeletedNotes.Clear();
				}

				var sw = Stopwatch.StartNew();

				_repo.SaveAll();

				var modifiedFilesystem = new List<Tuple<PathEntry, INote>>();
				var modifiedDatabase   = new List<Tuple<PathEntry, INote>>();
				var createdFilesystem  = new List<Tuple<PathEntry, INote>>();
				var deletedFilesystem  = new List<Tuple<PathEntry, INote>>();
				var createdNoteRepo    = new List<Tuple<PathEntry, INote>>();
				var deletedNoteRepo    = new List<Tuple<PathEntry, INote>>();

				var entriesFilesystem = EnumerateFolder(DirectoryPath.Root(), _path, 0).ToList();
				var entriesNoteRepo   = _repo.Notes.ToList();

				LoggerSingleton.Inst.Debug("RawFolderSync", 
					$"Found {entriesFilesystem.Count} entries in filesystem", 
					string.Join("\n", entriesFilesystem.Select(e => e.FilesystemPath)));
				LoggerSingleton.Inst.Debug("RawFolderSync", 
					$"Found {entriesNoteRepo.Count} entries in repository",
					string.Join("\n", entriesNoteRepo.Select(e => e.GetUniqueName()+"\t"+e.Title)));

				int successCount = 0;
				int errorCount   = 0;
				int keepCount    = 0;
				int ignoreCount  = 0;

				foreach (var entryFS in entriesFilesystem.ToList())
				{
					var entryNR = entriesNoteRepo.FirstOrDefault(e => e.Path.EqualsIgnoreCase(entryFS.NotePath) && (e.Title.ToLower() == entryFS.Title.ToLower() || (entryFS.Title == e.GetUniqueName() && e.Title=="")));

					if (entryNR != null)
					{
						// data exists in both

						if (entryNR.Text == entryFS.Content)
						{
							// nothing changed
							entriesFilesystem.Remove(entryFS);
							entriesNoteRepo.Remove(entryNR);
							keepCount++;
						}
						else
						{
							if (entryNR.ModificationDate >= entryFS.MDate)
							{
								// NoteRepo Changed
								modifiedDatabase.Add(Tuple.Create(entryFS, entryNR));
								entriesFilesystem.Remove(entryFS);
								entriesNoteRepo.Remove(entryNR);
							}
							else
							{
								// Filesystem Changed
								modifiedFilesystem.Add(Tuple.Create(entryFS, entryNR));
								entriesFilesystem.Remove(entryFS);
								entriesNoteRepo.Remove(entryNR);
							}
						}
					}
					else
					{
						// data missing in noterepo

						if (deletes.Any(d => d.Item1.ToLower() == entryFS.FilesystemPath.ToLower()))
						{
							// file removed in NR
							deletedNoteRepo.Add(Tuple.Create(entryFS, (INote)null));
							entriesFilesystem.Remove(entryFS);
						}
						else
						{
							// new file in FS
							createdFilesystem.Add(Tuple.Create(entryFS, (INote)null));
							entriesFilesystem.Remove(entryFS);
						}
					}
				}

				foreach (var entryNR in entriesNoteRepo.ToList())
				{
					// data missing in FS

					if (_lastSync == null || entryNR.ModificationDate > _lastSync) // when in doubt do not delete note. Register a [create]
					{
						// new file in NR
						createdNoteRepo.Add(Tuple.Create((PathEntry)null, entryNR));
						entriesNoteRepo.Remove(entryNR);
					}
					else
					{
						// file removed in fs
						deletedFilesystem.Add(Tuple.Create((PathEntry)null, entryNR));
						entriesNoteRepo.Remove(entryNR);
					}
				}

				if (entriesFilesystem.Any()) throw new Exception("entriesFilesystem must be empty after analyze");
				if (entriesNoteRepo.Any())   throw new Exception("entriesNoteRepo must be empty after analyze");
				
				
				foreach (var data in modifiedDatabase) // Change [Filesystem] <-- [NoteRepo]
				{
					try
					{
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Synchronized [modification] to Filesystem ({data.Item2.Title})", 
							$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{Fmt(data.Item2)}");

						File.WriteAllText(data.Item1.FilesystemPath, data.Item2.Text, _encoding);

						successCount++;
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [modification] to Filesystem failed ({data.Item2.Title})", e);
						LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [modification] to Filesystem failed ({data.Item2.Title})", e);
						errorCount++;
					}
				}
				
				foreach (var data in createdNoteRepo) // Created [Filesystem] <-- [NoteRepo]
				{
					try
					{
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Synchronized [creation] to Filesystem ({data.Item2.Title})", 
							$"Filesystem:\n{"NULL"}\n\nRepository:\n{Fmt(data.Item2)}");

						var tpath = GetFilesystemPath(data.Item2);
						Directory.CreateDirectory(Path.GetDirectoryName(tpath)??"");
						File.WriteAllText(tpath, data.Item2.Text, _encoding);

						successCount++;
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [creation] to Filesystem failed ({data.Item2.Title})", e);
						LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [creation] to Filesystem failed ({data.Item2.Title})", e);
						errorCount++;
					}
				}
				
				foreach (var data in deletedNoteRepo) // Deleted [Filesystem] <-- [NoteRepo]
				{
					try
					{
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Synchronized [deletion] to Filesystem ({data.Item1.Title})", 
							$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{"NULL"}");

						File.Delete(data.Item1.FilesystemPath);

						successCount++;
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [deletion] to Filesystem failed ({data.Item1.Title})", e);
						LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [deletion] to Filesystem failed ({data.Item1.Title})", e);
						errorCount++;
					}
				}

				foreach (var data in modifiedFilesystem) // Change [Filesystem] --> [NoteRepo]
				{
					if (!_syncModifications)
					{
						ignoreCount++;
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Event [modification] from filesystem to repository ignored due to settings", 
							$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{Fmt(data.Item2)}");
					}
					
					try
					{
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Synchronized [modification] to Repository ({data.Item2.Title})", 
							$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{Fmt(data.Item2)}");

						data.Item2.Text = data.Item1.Content;

						successCount++;
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [modification] to Repository failed ({data.Item2.Title})", e);
						LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [modification] to Repository failed ({data.Item2.Title})", e);
						errorCount++;
					}
				}
				
				foreach (var data in createdFilesystem) // Created [Filesystem] --> [NoteRepo]
				{
					if (!_syncCreation)
					{
						ignoreCount++;
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Event [creation] from filesystem to repository ignored due to settings", 
							$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{"NULL"}");
					}
					
					try
					{
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Synchronized [creation] to Repository ({data.Item1.Title})", 
							$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{"NULL"}");

						var n = _repo.CreateNewNote(data.Item1.NotePath);
						n.Title = data.Item1.Title;
						n.Text  = data.Item1.Content;

						successCount++;
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [creation] to Repository failed ({data.Item1.Title})", e);
						LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [creation] to Repository failed ({data.Item1.Title})", e);
						errorCount++;
					}
				}
				
				foreach (var data in deletedFilesystem) // Deleted [Filesystem] --> [NoteRepo]
				{
					if (!_syncDeletion)
					{
						ignoreCount++;
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Event [deletion] from filesystem to repository ignored due to settings", 
							$"Filesystem:\n{"NULL"}\n\nRepository:\n{Fmt(data.Item2)}");
					}
					
					try
					{
						LoggerSingleton.Inst.Debug("RawFolderSync", 
							$"Synchronized [deletion] to Repository ({data.Item2.Title})", 
							$"Filesystem:\n{"NULL"}\n\nRepository:\n{Fmt(data.Item2)}");

						_repo.DeleteNote(data.Item2, true);

						successCount++;
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [deletion] to Repository failed ({data.Item2.Title})", e);
						LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [deletion] to Repository failed ({data.Item2.Title})", e);
						errorCount++;
					}
				}

				_lastSync = DateTimeOffset.Now;

				sw.Stop();

				LoggerSingleton.Inst.Info("RawFolderSync", 
					$"Synchronization with local folder finished in {sw.ElapsedMilliseconds}ms",
					$"Unchanged entries:            {keepCount}\n" + 
					$"Successfully applied changes: {successCount}\n" + 
					$"Changes with sync errors:     {errorCount}\n" + 
					$"Ignored changes:              {ignoreCount}\n");
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository synchronization failed", e);
			}
			finally
			{
				_isSyncing = false;
			}
		}

		private string GetFilesystemPath(INote note)
		{
			var filename = FilenameHelper.ConvertStringForFilename(note.Title);
			if (string.IsNullOrWhiteSpace(filename)) filename = FilenameHelper.ConvertStringForFilename(note.GetUniqueName());

			var ext = ".txt";
			if (note.HasTagCasInsensitive(AppSettings.TAG_MARKDOWN)) ext = ".md";

			if (note.Path.IsRoot()) return Path.Combine(_path, filename+ext);

			var comp = new[]{ _path }
							.Concat(note.Path.Enumerate().Select(FilenameHelper.ConvertStringForFilename))
							.Concat(new[]{ filename+ext });

			return Path.Combine(comp.ToArray());
		}

		private object Fmt(PathEntry d)
		{
			StringBuilder b = new StringBuilder();
			b.AppendLine("PathEntry");
			b.AppendLine("{");
			b.AppendLine($"\tTitle:         '{d.Title}'");
			b.AppendLine($"\tContentLength: {d.Content.Length}");
			b.AppendLine($"\tContentHash:   {ChecksumHelper.MD5(d.Content)}");
			b.AppendLine($"\tChangeDate:    {d.MDate.ToString("yyyy-MM-dd HH:mm:ss")}");
			b.AppendLine($"\tPath:          {d.FilesystemPath}");
			b.AppendLine($"\tFolder:        {d.NotePath.StrSerialize()}");
			b.AppendLine("}");
			return b.ToString();
		}

		private object Fmt(INote d)
		{
			StringBuilder b = new StringBuilder();
			b.AppendLine("Note");
			b.AppendLine("{");
			b.AppendLine($"\tTitle:         '{d.Title}'");
			b.AppendLine($"\tContentLength: {d.Text.Length}");
			b.AppendLine($"\tContentHash:   {ChecksumHelper.MD5(d.Text)}");
			b.AppendLine($"\tChangeDate:    {d.ModificationDate.ToString("yyyy-MM-dd HH:mm:ss")}");
			b.AppendLine($"\tFolder:        {d.Path.StrSerialize()}");
			b.AppendLine("}");
			return b.ToString();
		}

		public void AddLocalDeletedNote(INote n)
		{
			if (!_enabled) return;

			lock (_repoDeletedNotes)
			{
				var path = GetFilesystemPath(n);

				_repoDeletedNotes.Add(Tuple.Create(path, n.ModificationDate));
			}
		}
	}
}
