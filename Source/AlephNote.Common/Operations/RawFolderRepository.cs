using AlephNote.Common.Repository;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Threading;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephNote.Common.Operations
{
	public class RawFolderRepository
	{
		private struct PathEntry { public string FilesystemPath; public DirectoryPath NotePath; public string Title; public DateTimeOffset MDate; public string Content; }

		private readonly bool _enabled;
		private readonly string _path;

		private readonly Encoding _encoding;
		private readonly bool _filewatcherEnabled;
		
		private readonly bool _syncModifications;
		private readonly bool _syncCreation;
		private readonly bool _syncDeletion;

		private readonly NoteRepository _repo;
		
		private readonly DelayedCombiningInvoker _invSyncRequest;

		private DateTimeOffset? _lastSync = null;
		private FileSystemWatcher _watcher;

		public RawFolderRepository(NoteRepository r, IAlephDispatcher _disp, AppSettings s)
		{
			_repo = r;
			_invSyncRequest = DelayedCombiningInvoker.Create(() => _disp.BeginInvoke(SyncNow), 5 * 1000, 30 * 1000);

			_enabled = s.UseRawFolderRepo;
			_path = s.RawFolderRepoPath;

			_filewatcherEnabled = s.RawFolderRepoUseFileWatcher;
			_encoding = EncodingEnumHelper.ToEncoding(s.RawFolderRepoEncoding);

			_syncModifications = s.RawFolderRepoAllowModification;
			_syncCreation      = s.RawFolderRepoAllowCreation;
			_syncDeletion      = s.RawFolderRepoAllowDeletion;
		}

		public void Start()
		{
			if (!_enabled) return;

			try
			{
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
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository initialization failed", e);
			}
		}

		private void WatcherOnChanged(object sender, FileSystemEventArgs e)
		{
			_invSyncRequest.Request();
		}

		public void Shutdown()
		{
			if (!_enabled) return;
			
			try
			{
				_invSyncRequest.CancelPendingRequests();
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository shutdown failed", e);
			}
		}

		private IEnumerable<PathEntry> EnumerateFolder(DirectoryPath dsrcpath, string fsrcpath, int depth)
		{
			if (depth >= 5) yield break;;

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
				var d = dsrcpath.SubDir(FilenameHelper.ConvertStringFromFilenameBack(Path.GetDirectoryName(dir)));

				foreach (var item in EnumerateFolder(d, dir, depth+1)) yield return item;
			}
		}

		public void SyncNow() // run in dispatcher thread
		{
			try
			{
				_repo.SaveAll();

				var modifiedFilesystem = new List<Tuple<PathEntry, INote>>();
				var modifiedDatabase   = new List<Tuple<PathEntry, INote>>();
				var createdFilesystem  = new List<Tuple<PathEntry, INote>>();
				var deletedFilesystem  = new List<Tuple<PathEntry, INote>>();
				var createdNoteRepo    = new List<Tuple<PathEntry, INote>>();
				var deletedNoteRepo    = new List<Tuple<PathEntry, INote>>();

				var entriesFilesystem = EnumerateFolder(DirectoryPath.Root(), _path, 0).ToList();
				var entriesNoteRepo   = _repo.Notes.ToList();

				foreach (var entryFS in entriesFilesystem.ToList())
				{
					var entryNR = entriesNoteRepo.FirstOrDefault(e => e.Path.EqualsIgnoreCase(entryFS.NotePath) && e.Title.ToLower() == entryFS.Title.ToLower());

					if (entryNR != null)
					{
						// data exists in both

						if (entryNR.Text == entryFS.Content)
						{
							// nothing changed
							entriesFilesystem.Remove(entryFS);
							entriesNoteRepo.Remove(entryNR);
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
							else if (entryNR.ModificationDate >= entryFS.MDate)
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

						if (_lastSync == null || _lastSync < entryFS.MDate)
						{
							// new file in FS
							createdFilesystem.Add(Tuple.Create(entryFS, entryNR));
							entriesFilesystem.Remove(entryFS);
						}
						else
						{
							// file removed in NR
							deletedNoteRepo.Add(Tuple.Create(entryFS, entryNR));
							entriesFilesystem.Remove(entryFS);
						}

					}
				}

				foreach (var entryNR in entriesNoteRepo)
				{
					// data missing in FS
					
				}
			}
			catch (Exception e)
			{
				LoggerSingleton.Inst.ShowExceptionDialog("Local folder repository synchronization failed", e);
			}
		}
	}
}
