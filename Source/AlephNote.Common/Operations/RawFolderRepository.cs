using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Threading;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AlephNote.Common.Operations
{
	public class RawFolderRepository
	{
		private class PathEntry
		{
			public readonly string FilesystemPath; 
			public readonly DirectoryPath NotePath; 
			public readonly string Title; 
			public readonly DateTimeOffset MDate;
			public readonly string Content;

			public PathEntry(string p, DirectoryPath dp, string t, DateTimeOffset md, string c)
			{
				FilesystemPath = p;
				NotePath = dp;
				Title = t;
				MDate = md;
				Content = c;
			}
		}

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
		private ConcurrentDictionary<string, string> _pathMapping = new ConcurrentDictionary<string, string>(); // UUID -> lowercase(path)

		private IAlephDispatcher _dispatcher;

		public RawFolderRepository(NoteRepository r, IAlephDispatcher disp, AppSettings s)
		{
			_repo = r;
			_dispatcher = disp;
			_invSyncRequest = DelayedCombiningInvoker.Create(() => disp.BeginInvoke(SyncNow), 5 * 1000, 30 * 1000);

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
					_watcher.NotifyFilter = NotifyFilters.Attributes 
					                      | NotifyFilters.CreationTime
					                      | NotifyFilters.DirectoryName
					                      | NotifyFilters.FileName 
					                      | NotifyFilters.LastWrite 
					                      | NotifyFilters.CreationTime; 

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

				yield return new PathEntry(
					filepath, 
					dsrcpath, 
					FilenameHelper.ConvertStringFromFilenameBack(Path.GetFileNameWithoutExtension(filepath)),
					new DateTimeOffset(new FileInfo(filepath).LastWriteTime), 
					File.ReadAllText(filepath, _encoding));
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

				var sw = Stopwatch.StartNew();
				var dictNoteRepo = _repo.Notes.ToDictionary(p => p.Clone(), p => p);
				var entriesNoteRepo = dictNoteRepo.Keys.ToList();
				
				var t1 = sw.ElapsedMilliseconds;

				new Thread(() =>
				{
					if (_isSyncing)
					{
						LoggerSingleton.Inst.Warn("RawFolderSync", "Abort sync because another sync thread is already running");
						return;
					}
					
					if (!_enabled)
					{
						LoggerSingleton.Inst.Warn("RawFolderSync", "Abort sync because the enabled=false");
						return;
					}

					try
					{
						_isSyncing = true;
						DoSyncThreaded(sw, t1, entriesNoteRepo, dictNoteRepo);
					}
					catch (Exception e)
					{
						LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository synchronization failed (async)", e);
					}
					finally
					{
						_isSyncing = false;
					}

				}).Start();
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository synchronization failed (sync)", e);
			}
		}

		private void DoSyncThreaded(Stopwatch sw, long t1, List<INote> entriesNoteRepo, Dictionary<INote, INote> realNoteMapping)
		{
			List<Tuple<string, DateTimeOffset>> deletes;
			lock (_repoDeletedNotes)
			{
				deletes = _repoDeletedNotes.ToList();
				_repoDeletedNotes.Clear();
			}
			
			var unmodifiedBoth     = new List<Tuple<PathEntry, INote>>();
			var modifiedFilesystem = new List<Tuple<PathEntry, INote>>();
			var modifiedNoteRepo   = new List<Tuple<PathEntry, INote>>();
			var createdFilesystem  = new List<Tuple<PathEntry, INote>>();
			var deletedFilesystem  = new List<Tuple<PathEntry, INote>>();
			var createdNoteRepo    = new List<Tuple<PathEntry, INote>>();
			var deletedNoteRepo    = new List<Tuple<PathEntry, INote>>();

			var entriesFilesystem = EnumerateFolder(DirectoryPath.Root(), _path, 0).ToList();
			
			LoggerSingleton.Inst.Debug("RawFolderSync", 
				$"Found {entriesFilesystem.Count} entries in filesystem", 
				string.Join("\n", entriesFilesystem.Select(e => e.FilesystemPath)));
			LoggerSingleton.Inst.Debug("RawFolderSync", 
				$"Found {entriesNoteRepo.Count} entries in repository",
				string.Join("\n", entriesNoteRepo.Select(e => e.UniqueName+"\t"+e.Title)));
			
			#region Step 1: Analyze

			foreach (var map in _pathMapping.ToList())
			{
				var entryNR = entriesNoteRepo.FirstOrDefault(n => n.UniqueName== map.Key);
				var entryFS = entriesFilesystem.FirstOrDefault(n => n.FilesystemPath.ToLower() == map.Value.ToLower());

				if (entryNR==null && entryFS==null) continue; // mkay...

				if (entryNR==null && entryFS!=null)
				{
					// file removed in NR
					deletedNoteRepo.Add(Tuple.Create(entryFS, (INote)null));
					entriesFilesystem.Remove(entryFS);
					continue;
				}
				
				if (entryNR!=null && entryFS==null)
				{
					// file removed in FS
					deletedFilesystem.Add(Tuple.Create((PathEntry)null, entryNR));
					entriesNoteRepo.Remove(entryNR);
					continue;
				}
				
				if (entryNR!=null && entryFS!=null)
				{
					if (entryNR.Text == entryFS.Content && entryFS.FilesystemPath == GetFilesystemPath(entryNR))
					{
						// nothing changed
						unmodifiedBoth.Add(Tuple.Create(entryFS, entryNR));
						entriesFilesystem.Remove(entryFS);
						entriesNoteRepo.Remove(entryNR);
						continue;
					}
					else
					{
						if (_lastSync == null || entryNR.ModificationDate > _lastSync)
						{
							// NoteRepo Changed
							modifiedNoteRepo.Add(Tuple.Create(entryFS, entryNR));
							entriesFilesystem.Remove(entryFS);
							entriesNoteRepo.Remove(entryNR);
							continue;
						}
						else if (entryNR.ModificationDate >= entryFS.MDate)
						{
							// NoteRepo Changed
							modifiedNoteRepo.Add(Tuple.Create(entryFS, entryNR));
							entriesFilesystem.Remove(entryFS);
							entriesNoteRepo.Remove(entryNR);
							continue;
						}
						else
						{
							// Filesystem Changed
							modifiedFilesystem.Add(Tuple.Create(entryFS, entryNR));
							entriesFilesystem.Remove(entryFS);
							entriesNoteRepo.Remove(entryNR);
							continue;
						}
					}
				}

				throw new Exception("Invalid control flow in DoSyncThreaded::Step1");
			}

			foreach (var entryFS in entriesFilesystem.ToList())
			{
				var entryNR = entriesNoteRepo.FirstOrDefault(e => e.Path.EqualsIgnoreCase(entryFS.NotePath) && (e.Title.ToLower() == entryFS.Title.ToLower() || (entryFS.Title == e.UniqueName&& e.Title=="")));

				if (entryNR != null)
				{
					// data exists in both

					if (entryNR.Text == entryFS.Content && entryNR.Title == entryFS.Title)
					{
						// nothing changed
						unmodifiedBoth.Add(Tuple.Create(entryFS, entryNR));
						entriesFilesystem.Remove(entryFS);
						entriesNoteRepo.Remove(entryNR);
					}
					else
					{
						if (entryNR.ModificationDate >= entryFS.MDate)
						{
							// NoteRepo Changed
							modifiedNoteRepo .Add(Tuple.Create(entryFS, entryNR));
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
				
				if (_lastSync == null || entryNR.ModificationDate > _lastSync)
				{
					// new file in NR
					createdNoteRepo.Add(Tuple.Create((PathEntry)null, entryNR));
					entriesNoteRepo.Remove(entryNR);
				}
				else
				{
					// file removed in FS
					deletedFilesystem.Add(Tuple.Create((PathEntry)null, entryNR));
					entriesNoteRepo.Remove(entryNR);
				}
			}

			#endregion
			
			if (entriesFilesystem.Any()) throw new Exception("entriesFilesystem must be empty after analyze");
			if (entriesNoteRepo.Any())   throw new Exception("entriesNoteRepo must be empty after analyze");

			#region Step 2: Apply changes

			int countSuccess = 0;
			int countError   = 0;
			int countIgnored = 0;
			
			var newMapping = new List<Tuple<string, string>>(); // path -> UUID

			foreach (var data in unmodifiedBoth)
			{
				newMapping.Add(Tuple.Create(data.Item1.FilesystemPath, data.Item2.UniqueName));
			}

			foreach (var data in modifiedNoteRepo ) // Change [Filesystem] <-- [NoteRepo]
			{
				try
				{
					var p2 = GetFilesystemPath(data.Item2);
					var domove = (data.Item1.FilesystemPath.ToLower() != p2.ToLower());

					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Synchronized [modification{(domove?"+rename":"")}] to Filesystem ({data.Item2.Title})", 
						$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{Fmt(data.Item2)}");

					if (domove)
					{
						if (File.Exists(p2)) File.Delete(p2);
						File.Move(data.Item1.FilesystemPath, p2);
					}

					File.WriteAllText(p2, data.Item2.Text, _encoding);
					newMapping.Add(Tuple.Create(p2, data.Item2.UniqueName));

					countSuccess++;
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [modification] to Filesystem failed ({data.Item2.Title})", e);
					LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [modification] to Filesystem failed ({data.Item2.Title})", e);
					countError++;
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
					newMapping.Add(Tuple.Create(tpath, data.Item2.UniqueName));

					countSuccess++;
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [creation] to Filesystem failed ({data.Item2.Title})", e);
					LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [creation] to Filesystem failed ({data.Item2.Title})", e);
					countError++;
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

					countSuccess++;
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [deletion] to Filesystem failed ({data.Item1.Title})", e);
					LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [deletion] to Filesystem failed ({data.Item1.Title})", e);
					countError++;
				}
			}

			foreach (var data in modifiedFilesystem) // Change [Filesystem] --> [NoteRepo]
			{
				if (!_syncModifications)
				{
					countIgnored++;
					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Event [modification] from filesystem to repository ignored due to settings", 
						$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{Fmt(data.Item2)}");
				}
					
				try
				{
					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Synchronized [modification] to Repository ({data.Item2.Title})", 
						$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{Fmt(data.Item2)}");

					_dispatcher.Invoke(() =>
					{
						var note = realNoteMapping[data.Item2];
						note.Text = data.Item1.Content;
						note.Title = data.Item1.Title;
						newMapping.Add(Tuple.Create(data.Item1.FilesystemPath, note.UniqueName));
					});

					countSuccess++;
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [modification] to Repository failed ({data.Item2.Title})", e);
					LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [modification] to Repository failed ({data.Item2.Title})", e);
					countError++;
				}
			}
				
			foreach (var data in createdFilesystem) // Created [Filesystem] --> [NoteRepo]
			{
				if (!_syncCreation)
				{
					countIgnored++;
					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Event [creation] from filesystem to repository ignored due to settings", 
						$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{"NULL"}");
				}
					
				try
				{
					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Synchronized [creation] to Repository ({data.Item1.Title})", 
						$"Filesystem:\n{Fmt(data.Item1)}\n\nRepository:\n{"NULL"}");
					
					_dispatcher.Invoke(() =>
					{
						var n = _repo.CreateNewNote(data.Item1.NotePath);
						n.Title = data.Item1.Title;
						n.Text  = data.Item1.Content;
						newMapping.Add(Tuple.Create(data.Item1.FilesystemPath, n.UniqueName));
					});

					countSuccess++;
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [creation] to Repository failed ({data.Item1.Title})", e);
					LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [creation] to Repository failed ({data.Item1.Title})", e);
					countError++;
				}
			}
				
			foreach (var data in deletedFilesystem) // Deleted [Filesystem] --> [NoteRepo]
			{
				if (!_syncDeletion)
				{
					countIgnored++;
					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Event [deletion] from filesystem to repository ignored due to settings", 
						$"Filesystem:\n{"NULL"}\n\nRepository:\n{Fmt(data.Item2)}");
				}
					
				try
				{
					LoggerSingleton.Inst.Debug("RawFolderSync", 
						$"Synchronized [deletion] to Repository ({data.Item2.Title})", 
						$"Filesystem:\n{"NULL"}\n\nRepository:\n{Fmt(data.Item2)}");
					
					_dispatcher.Invoke(() =>
					{
						var note = realNoteMapping[data.Item2];
						_repo.DeleteNote(note, true);
					});

					countSuccess++;
				}
				catch (Exception e)
				{
					LoggerSingleton.Inst.Error("RawFolderSync", $"Synchronize [deletion] to Repository failed ({data.Item2.Title})", e);
					LoggerSingleton.Inst.ShowSyncErrorDialog($"Synchronize [deletion] to Repository failed ({data.Item2.Title})", e);
					countError++;
				}
			}

			#endregion
			
			_pathMapping.Clear();
			foreach (var newmap in newMapping) _pathMapping[newmap.Item2] = newmap.Item1;

			_lastSync = DateTimeOffset.Now;

			sw.Stop();

			LoggerSingleton.Inst.Info("RawFolderSync", 
				$"Synchronization with local folder finished in {sw.ElapsedMilliseconds}ms ({t1}ms synchronous) ({countSuccess} changes | {countError} errors)",
				$"Successful changes: {countSuccess}\n" + 
				$"Errored changes:    {countError}\n" + 
				$"Ignored changes:    {countIgnored}\n" + 
				"\n\n" + 
				$"Unchanged entries: {unmodifiedBoth.Count}\n" + 
				$"Modified(FS):      {modifiedFilesystem.Count}\n" + 
				$"Modified(NR):      {modifiedNoteRepo .Count}\n" + 
				$"Created(FS):       {createdFilesystem.Count}\n" + 
				$"Created(NR):       {createdNoteRepo.Count}\n" + 
				$"Deleted(FS):       {deletedFilesystem.Count}\n" + 
				$"Deleted(NR):       {deletedNoteRepo.Count}\n" + 
				"\n\n" + 
				"PathMapping(new):\n" + 
				string.Join("\n", newMapping.Select(m => $"  {m.Item2}      {m.Item1}")));
		}

		private string GetFilesystemPath(INote note)
		{
			var filename = FilenameHelper.ConvertStringForFilename(note.Title);
			if (string.IsNullOrWhiteSpace(filename)) filename = FilenameHelper.ConvertStringForFilename(note.UniqueName);

			var ext = ".txt";
			if (note.HasTagCaseInsensitive(AppSettings.TAG_MARKDOWN)) ext = ".md";

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
