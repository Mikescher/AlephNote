using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlephNote.Common.Operations;
using AlephNote.Common.Settings;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Util;
using MSHC.Util.Threads;
using MSHC.WPF.MVVM;

namespace AlephNote.Common.Repository
{
	public class NoteRepository : ObservableObject, ISynchronizationFeedback, IRepository
	{
		private readonly AlephLogger _logger = LoggerSingleton.Inst;

		private readonly string _pathLocalFolder;
		private readonly string _pathLocalData;
		private readonly string _pathLocalBase;

		private readonly IRemoteStorageConnection _conn;
		private readonly RemoteStorageAccount _account;
		private          AppSettings _appconfig;
		private readonly SynchronizationThread _thread;
		private readonly ISynchronizationFeedback _listener;
		private readonly IAlephDispatcher _dispatcher;
		private readonly RawFolderRepository _rawFilesystemRepo;

		public readonly List<INote> LocalDeletedNotes = new List<INote>(); // deleted local but not on remote

		private readonly ObservableCollection<INote> _notes = new ObservableCollectionNoReset<INote>();
		public ObservableCollection<INote> Notes { get { return _notes; } }

		private readonly object _lockSaveNote = new object();
		private readonly object _lockSaveSyncData = new object();

		private readonly DelayedCombiningInvoker _invSaveNotesLocal;
		private readonly DelayedCombiningInvoker _invSaveNotesRemote;
		private readonly DelayedCombiningInvoker _invSaveNotesGitBackup;

		private RepoLock _lock;

		public IRemoteStorageConnection Connection { get { return _conn; } }
		
		public string ConnectionName { get { return _account.Plugin.DisplayTitleShort; } }
		public string ConnectionDisplayTitle { get { return _account.DisplayTitle; } }
		public string ConnectionUUID { get { return _account.ID.ToString("B"); } }
		public Guid ConnectionID { get { return _account.ID; } }
		public string ProviderID { get { return _account.Plugin.GetUniqueID().ToString("B"); } }
		public Guid ProviderUID { get { return _account.Plugin.GetUniqueID(); } }

		public bool SupportsPinning                   => _account.Plugin.SupportsPinning;
		public bool SupportsLocking                   => _account.Plugin.SupportsLocking;
		public bool SupportsTags                      => _account.Plugin.SupportsTags;
		public bool SupportsDownloadMultithreading    => _account.Plugin.SupportsDownloadMultithreading;
		public bool SupportsNewDownloadMultithreading => _account.Plugin.SupportsNewDownloadMultithreading;
		public bool SupportsUploadMultithreading      => _account.Plugin.SupportsUploadMultithreading;

		public NoteRepository(string path, ISynchronizationFeedback fb, AppSettings cfg, RemoteStorageAccount acc, IAlephDispatcher disp)
		{
			_pathLocalBase = path;
			_pathLocalFolder = Path.Combine(path, acc.ID.ToString("B"));
			_pathLocalData = Path.Combine(path, acc.ID.ToString("B") + ".xml");
			_conn = acc.Plugin.CreateRemoteStorageConnection(cfg.CreateProxy(), acc.Config, cfg.GetHierarchicalConfig());
			_account = acc;
			_appconfig = cfg;
			_listener = fb;
			_dispatcher = disp;
			_thread = new SynchronizationThread(this, new[]{ this, fb }, cfg, _dispatcher);

			_invSaveNotesLocal     = DelayedCombiningInvoker.Create(() => _dispatcher.BeginInvoke(SaveAllDirtyNotes),      10 * 1000,  1 * 60 * 1000);
			_invSaveNotesRemote    = DelayedCombiningInvoker.Create(() => _dispatcher.BeginInvoke(SyncNow),                45 * 1000, 15 * 60 * 1000);
			_invSaveNotesGitBackup = DelayedCombiningInvoker.Create(() => _dispatcher.BeginInvoke(CommitToLocalGitBackup), 10 * 1000, 15 * 60 * 1000);

			_rawFilesystemRepo = new RawFolderRepository(this, disp, cfg);

			_notes.CollectionChanged += NoteCollectionChanged;
		}

		public void Init()
		{
			var sw = Stopwatch.StartNew();

			if (!Directory.Exists(_pathLocalFolder))
			{
				_logger.Info("Repository", "Create local note folder: " + _pathLocalFolder);
				Directory.CreateDirectory(_pathLocalFolder);
			}

			_lock = RepoLock.Lock(_logger, _pathLocalFolder);

			LoadNotesFromLocal();

			_thread.Start(_appconfig.GetSyncDelay());
			
			_rawFilesystemRepo.Start();

			_logger.Trace("Repository", $"SyncThread init took {sw.ElapsedMilliseconds}ms");
		}

		public void Shutdown(bool lastSync = true)
		{
			var sw = Stopwatch.StartNew();

			_invSaveNotesLocal.CancelPendingRequests();
			SaveAllDirtyNotes();

			if (lastSync && Notes.Any(n => !n.IsRemoteSaved && !n.IsConflictNote))
				_thread.SyncNowAndStopAsync();
			else
				_thread.StopAsync();

			if (lastSync) _dispatcher.Invoke(() => _rawFilesystemRepo.SyncNow());
			_rawFilesystemRepo.Shutdown();

			_lock.Release();
			_lock = null;

			_logger.Trace("Repository", $"SyncThread shutdown took {sw.ElapsedMilliseconds}ms");
		}

		public void KillThread()
		{
			_thread.Kill();
		}

		private void LoadNotesFromLocal()
		{
			var noteFiles = Directory.GetFiles(_pathLocalFolder, "*.xml");

			_logger.Info("Repository", "Found " + noteFiles.Length + " files in local repository");

			foreach (var noteFile in noteFiles)
			{
				try
				{
					var note = LoadNoteFromFile(noteFile);
					note.IsUINote = true;

					Notes.Add(note);
				}
				catch (Exception e)
				{
					_logger.ShowExceptionDialog("LoadNotes from local cache", "Could not load note from '" + noteFile + "'", e);
				}
			}

			_logger.Trace("Repository", "Loaded " + Notes.Count + " notes from local repository");
		}

		private INote LoadNoteFromFile(string noteFile)
		{
			var doc = XDocument.Load(noteFile);

			var root = doc.Root;
			if (root == null) throw new Exception("Root == null");

			var data = root.Element("data");
			if (data == null) throw new Exception("missing data node");

			var note = _account.Plugin.CreateEmptyNote(Connection, _account.Config);
			note.Deserialize(data.Elements().FirstOrDefault());
			note.ResetLocalDirty("Reset local dirty after deserialization");
			note.ResetRemoteDirty("Reset remote dirty after deserialization");

			var meta = root.Element("meta");
			if (meta == null) throw new Exception("missing meta node");

			if (XHelper.GetChildValue(meta, "dirty", false)) note.SetRemoteDirty("Set remote dirty from deserialization-value (pending sync from last app-run)");
			note.IsConflictNote = XHelper.GetChildValue(meta, "conflict", false);
			return note;
		}

		public INote CreateNewNote(DirectoryPath p = null)
		{
			p = p ?? DirectoryPath.Root();
			var note = _account.Plugin.CreateEmptyNote(Connection, _account.Config);
			note.Path = p;
			note.IsUINote=true;
			Notes.Add(note);
			note.SetDirty("Set newly created note to dirty");
			SaveNote(note);

			_logger.Info("Repository", "New Note created");

			return note;
		}

		private void SaveAllDirtyNotes()
		{
			foreach (var note in _notes)
			{
				if (!note.IsLocalSaved) SaveNote(note);
			}
		}

		public void SaveNote(INote note)
		{
			SaveNote(note, _pathLocalFolder, true);
		}

		private void SaveNote(INote note, string localFolder, bool doRoundtrip)
		{
			lock (_lockSaveNote)
			{
				var path = Path.Combine(localFolder, note.UniqueName+ ".xml");
				var tempPath = Path.GetTempFileName();

				var doc = SerializeNote(note);

				using (var file = File.OpenWrite(tempPath)) doc.Save(file);

				try
				{
					if (doRoundtrip)
					{
						var roundtrip = LoadNoteFromFile(tempPath);

						if (roundtrip.Text != note.Text) throw new Exception("a.Text != b.Text");
						if (roundtrip.Title != note.Title) throw new Exception("a.Title != b.Title");
						if (roundtrip.IsPinned != note.IsPinned) throw new Exception("a.IsPinned != b.IsPinned");
						if (roundtrip.IsLocked != note.IsLocked) throw new Exception("a.IsLocked != b.IsLocked");
						if (!roundtrip.Path.Equals(note.Path)) throw new Exception("a.Path != b.Path");
					}

					File.Copy(tempPath, path, true);
					note.ResetLocalDirty("Reset local dirty after SaveNote()");
					File.Delete(tempPath);
				}
				catch (Exception e)
				{
					throw new Exception("Serialization failed (Sanity check):" + e.Message, e);
				}
			}
		}

		public XDocument SerializeNote(INote note)
		{
			var root = new XElement("note");

			var meta = new XElement("meta");
			meta.Add(new XElement("date", DateTime.Now.ToString("O")));
			meta.Add(new XElement("provider", _account.Plugin.GetUniqueID().ToString("B")));
			meta.Add(new XElement("dirty", !note.IsRemoteSaved));
			meta.Add(new XElement("conflict", note.IsConflictNote));
			meta.Add(new XElement("real_title", note.Title));
			meta.Add(new XElement("real_path", string.Join("/", note.Path.Enumerate())));
			root.Add(meta);

			root.Add(new XElement("data", note.Serialize()));

			return new XDocument(root);
		}

		private void NoteCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (var note in e.NewItems.Cast<INote>())
				{
					note.OnChanged += NoteChanged;
				}
			}

			if (e.OldItems != null)
			{
				foreach (var note in e.OldItems.Cast<INote>())
				{
					note.OnChanged -= NoteChanged;
				}
			}
		}

		private void NoteChanged(object sender, NoteChangedEventArgs e) // only local changes
		{
			_invSaveNotesLocal.Request();
			_invSaveNotesRemote.Request();
			_invSaveNotesGitBackup.Request();
			
			_listener.OnSyncRequest();
			
			_listener.OnNoteChanged(e);
		}

		public void DeleteNote(INote note, bool updateRemote)
		{
			_logger.Info("Repository", string.Format("Delete note {0} (updateRemote={1})", note.UniqueName, updateRemote));

			var found = Notes.Remove(note);

			var path = Path.Combine(_pathLocalFolder, note.UniqueName+ ".xml");
			if (File.Exists(path)) File.Delete(path);

			if (found && updateRemote)
			{
				LocalDeletedNotes.Add(note);
				_rawFilesystemRepo.AddLocalDeletedNote(note);
				_thread.SyncNowAsync();
			}
		}

		public void AddNote(INote note, bool updateRemote)
		{
			_logger.Info("Repository", $"Add note {note.UniqueName} (updateRemote={updateRemote})");

			note.IsUINote = true;
			Notes.Add(note);
			SaveNote(note);

			_invSaveNotesLocal.Request();
			_invSaveNotesGitBackup.Request();

			if (updateRemote)
			{
				_thread.SyncNowAsync();
			}
		}

		public INote FindNoteByID(string nid)
		{
			foreach (var n in _notes)
			{
				if (n.UniqueName== nid) return n;
			}
			return null;
		}

		public void SyncNow() // = StartSyncNow, real sync happens asynchronous
		{
			_logger.Info("Repository", "Sync Now");

			_invSaveNotesRemote.CancelPendingRequests();

			_dispatcher.Invoke(() => _rawFilesystemRepo.SyncNow()); //synchron

			_thread.SyncNowAsync();

			CommitToLocalGitBackup(); // without invoker
		}

		private void CommitToLocalGitBackup()
		{
			LocalGitBackup.UpdateRepository(this, _appconfig);
		}

		public void SaveAll()
		{
			SaveAllDirtyNotes();
		}

		public IRemoteStorageSyncPersistance GetSyncData()
		{
			lock (_lockSaveSyncData)
			{
				var d = _account.Plugin.CreateEmptyRemoteSyncData();
				if (File.Exists(_pathLocalData))
				{
					try
					{
						var doc = XDocument.Load(_pathLocalData);

						var root = doc.Root;
						if (root == null) throw new Exception("Root == null");

						d.Deserialize(root);

						return d;
					}
					catch (Exception e)
					{
						throw new Exception("Could not load synchronization state from '" + _pathLocalData + "'", e);
					}
				}
				else
				{
					WriteSyncData(d, _pathLocalData);
					return d;
				}
			}
		}

		public void WriteSyncData(IRemoteStorageSyncPersistance data)
		{
			lock (_lockSaveSyncData)
			{
				WriteSyncData(data, _pathLocalData);
			}
		}

		private void WriteSyncData(IRemoteStorageSyncPersistance data, string pathData)
		{
			var tempPath = Path.GetTempFileName();

			var x = new XDocument(data.Serialize());
			using (var file = File.OpenWrite(tempPath)) x.Save(file);

			File.Copy(tempPath, pathData, true);
			File.Delete(tempPath);
		}

		public void DeleteLocalData()
		{
			lock (_lockSaveSyncData)
			{
				if (File.Exists(_pathLocalData))
				{
					_logger.Info("Repository", "Delete file from local repository: " + Path.GetFileName(_pathLocalData), _pathLocalData);
					File.Delete(_pathLocalData);
				}
			}

			var noteFiles = Directory.GetFiles(_pathLocalFolder, "*.xml");
			foreach (var path in noteFiles)
			{
				_logger.Info("Repository", "Delete file from local repository: " + Path.GetFileName(path), path);
				File.Delete(path);
			}

			_logger.Info("Repository", "Delete folder from local repository: " + Path.GetFileName(_pathLocalFolder), _pathLocalFolder);
			Directory.Delete(_pathLocalFolder, true);
		}

		public void ShowConflictResolutionDialog(string uuid, string txt0, string ttl0, List<string> tgs0, DirectoryPath ndp0, string txt1, string ttl1, List<string> tgs1, DirectoryPath ndp1)
		{
			_listener.ShowConflictResolutionDialog(uuid, txt0, ttl0, tgs0, ndp0, txt1, ttl1, tgs1, ndp1);
		}

		public IEnumerable<string> EnumerateAllTags()
		{
			return Notes.SelectMany(n => n.Tags);
		}

		public void ReplaceSettings(AppSettings settings)
		{
			_appconfig = settings;
		}

		public void ApplyNewAccountData(RemoteStorageAccount acc, IRemoteStorageSyncPersistance data, List<INote> notes)
		{
			var localFolder = Path.Combine(_pathLocalBase, acc.ID.ToString("B"));
			var localData   = Path.Combine(_pathLocalBase, acc.ID.ToString("B") + ".xml");

			if (!Directory.Exists(localFolder))
			{
				_logger.Info("Repository", "Create local note folder: " + localFolder);
				Directory.CreateDirectory(localFolder);
			}

			WriteSyncData(data, localData);

			foreach (var n in notes)
			{
				SaveNote(n, localFolder, false);
			}
		}

		public void StartSync()
		{
			// [event from sync thread]
		}

		public void SyncSuccess(DateTimeOffset now)
		{
			// [event from sync thread]

			LocalGitBackup.UpdateRepository(this, _appconfig);
		}

		public void SyncError(List<Tuple<string, Exception>> errors)
		{
			// [event from sync thread]

			LocalGitBackup.UpdateRepository(this, _appconfig);
		}

		public void OnSyncRequest()
		{
			// [event from sync thread]
		}

		public void OnNoteChanged(NoteChangedEventArgs e)
		{
			// [event from sync thread]
		}
	}
}
