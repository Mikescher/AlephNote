using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlephNote.Common.MVVM;
using AlephNote.Common.Operations;
using AlephNote.Common.Settings;
using AlephNote.Common.Threading;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.Repository
{
	public class NoteRepository : ObservableObject, ISynchronizationFeedback
	{
		private readonly string pathLocalFolder;
		private readonly string pathLocalData;
		private readonly string pathLocalBase;

		private readonly IRemoteStorageConnection conn;
		private readonly RemoteStorageAccount account;
		private          AppSettings appconfig;
		private readonly SynchronizationThread thread;
		private readonly ISynchronizationFeedback listener;
		private readonly IAlephDispatcher dispatcher;
		private readonly IAlephLogger logger;

		public readonly List<INote> LocalDeletedNotes = new List<INote>(); // deleted local but not on remote

		private readonly ObservableCollection<INote> _notes = new ObservableCollectionNoReset<INote>();
		public ObservableCollection<INote> Notes { get { return _notes; } }

		private readonly object _lockSaveNote = new object();
		private readonly object _lockSaveSyncData = new object();

		private readonly DelayedCombiningInvoker invSaveNotesLocal;
		private readonly DelayedCombiningInvoker invSaveNotesRemote;
		private readonly DelayedCombiningInvoker invSaveNotesGitBackup;

		public IRemoteStorageConnection Connection { get { return conn; } }

		public string ConnectionName { get { return account.Plugin.DisplayTitleShort; } }
		public string ConnectionDisplayTitle { get { return account.DisplayTitle; } }
		public string ConnectionUUID { get { return account.ID.ToString("B"); } }
		public Guid ConnectionID { get { return account.ID; } }
		public string ProviderID { get { return account.Plugin.GetUniqueID().ToString("B"); } }
		public Guid ProviderUID { get { return account.Plugin.GetUniqueID(); } }

		public NoteRepository(string path, ISynchronizationFeedback fb, AppSettings cfg, RemoteStorageAccount acc, IAlephLogger log, IAlephDispatcher disp)
		{
			pathLocalBase = path;
			pathLocalFolder = Path.Combine(path, acc.ID.ToString("B"));
			pathLocalData = Path.Combine(path, acc.ID.ToString("B") + ".xml");
			conn = acc.Plugin.CreateRemoteStorageConnection(cfg.CreateProxy(), acc.Config, cfg.GetHierachicalConfig());
			account = acc;
			appconfig = cfg;
			listener = fb;
			logger = log;
			dispatcher = disp;
			thread = new SynchronizationThread(this, new[]{ this, fb }, cfg.ConflictResolution, log, dispatcher);

			invSaveNotesLocal     = DelayedCombiningInvoker.Create(() => dispatcher.BeginInvoke(SaveAllDirtyNotes),      10 * 1000,  1 * 60 * 1000);
			invSaveNotesRemote    = DelayedCombiningInvoker.Create(() => dispatcher.BeginInvoke(SyncNow),                45 * 1000, 15 * 60 * 1000);
			invSaveNotesGitBackup = DelayedCombiningInvoker.Create(() => dispatcher.BeginInvoke(CommitToLocalGitBackup), 10 * 1000, 15 * 60 * 1000);

			_notes.CollectionChanged += NoteCollectionChanged;
		}

		public void Init()
		{
			if (!Directory.Exists(pathLocalFolder))
			{
				logger.Info("Repository", "Create local note folder: " + pathLocalFolder);
				Directory.CreateDirectory(pathLocalFolder);
			}

			LoadNotesFromLocal();

			thread.Start(appconfig.GetSyncDelay());
		}

		public void Shutdown(bool lastSync = true)
		{
			invSaveNotesLocal.CancelPendingRequests();
			SaveAllDirtyNotes();

			if (lastSync && Notes.Any(n => !n.IsRemoteSaved))
				thread.SyncNowAndStopAsync();
			else
				thread.StopAsync();
		}

		public void KillThread()
		{
			thread.Kill();
		}

		private void LoadNotesFromLocal()
		{
			var noteFiles = Directory.GetFiles(pathLocalFolder, "*.xml");

			logger.Info("Repository", "Found " + noteFiles.Length + " files in local repository");

			foreach (var noteFile in noteFiles)
			{
				try
				{
					var note = LoadNoteFromFile(noteFile);

					Notes.Add(note);
				}
				catch (Exception e)
				{
					logger.ShowExceptionDialog("LoadNotes from local cache", "Could not load note from '" + noteFile + "'", e);
				}
			}

			logger.Info("Repository", "Loaded " + Notes.Count + " notes from local repository");
		}

		private INote LoadNoteFromFile(string noteFile)
		{
			var doc = XDocument.Load(noteFile);

			var root = doc.Root;
			if (root == null) throw new Exception("Root == null");

			var data = root.Element("data");
			if (data == null) throw new Exception("missing data node");

			var note = account.Plugin.CreateEmptyNote(Connection, account.Config);
			note.Deserialize(data.Elements().FirstOrDefault());
			note.ResetLocalDirty();
			note.ResetRemoteDirty();

			var meta = root.Element("meta");
			if (meta == null) throw new Exception("missing meta node");

			if (XHelper.GetChildValue(meta, "dirty", false)) note.SetRemoteDirty();
			note.IsConflictNote = XHelper.GetChildValue(meta, "conflict", false);
			return note;
		}

		public INote CreateNewNote(DirectoryPath p = null)
		{
			p = p ?? DirectoryPath.Root();
			var note = account.Plugin.CreateEmptyNote(Connection, account.Config);
			note.Path = p;
			Notes.Add(note);
			note.SetDirty();
			SaveNote(note);

			logger.Info("Repository", "New Note created");

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
			SaveNote(note, pathLocalFolder, true);
		}

		private void SaveNote(INote note, string localFolder, bool doRoundtrip)
		{
			lock (_lockSaveNote)
			{
				var path = Path.Combine(localFolder, note.GetUniqueName() + ".xml");
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
					}

					File.Copy(tempPath, path, true);
					note.ResetLocalDirty();
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
			meta.Add(new XElement("provider", account.Plugin.GetUniqueID().ToString("B")));
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

		private void NoteChanged(object sender, NoteChangedEventArgs e)
		{
			invSaveNotesLocal.Request();
			invSaveNotesRemote.Request();
			invSaveNotesGitBackup.Request();
			
			listener.OnSyncRequest();
			
			listener.OnNoteChanged(e);
		}

		public void DeleteNote(INote note, bool updateRemote)
		{
			logger.Info("Repository", string.Format("Delete note {0} (updateRemote={1})", note.GetUniqueName(), updateRemote));

			var found = Notes.Remove(note);

			var path = Path.Combine(pathLocalFolder, note.GetUniqueName() + ".xml");
			if (File.Exists(path)) File.Delete(path);

			if (found && updateRemote)
			{
				LocalDeletedNotes.Add(note);
				thread.SyncNowAsync();
			}
		}

		public void AddNote(INote note, bool updateRemote)
		{
			logger.Info("Repository", string.Format("Add note {0} (updateRemote={1})", note.GetUniqueName(), updateRemote));

			Notes.Add(note);
			SaveNote(note);

			invSaveNotesLocal.Request();
			invSaveNotesGitBackup.Request();

			if (updateRemote)
			{
				thread.SyncNowAsync();
			}
		}

		public void SyncNow()
		{
			logger.Info("Repository", "Sync Now");

			invSaveNotesRemote.CancelPendingRequests();

			thread.SyncNowAsync();

			CommitToLocalGitBackup(); // without invoker
		}

		private void CommitToLocalGitBackup()
		{
			LocalGitBackup.UpdateRepository(this, appconfig, logger);
		}

		public void SaveAll()
		{
			SaveAllDirtyNotes();
		}

		public IRemoteStorageSyncPersistance GetSyncData()
		{
			lock (_lockSaveSyncData)
			{
				var d = account.Plugin.CreateEmptyRemoteSyncData();
				if (File.Exists(pathLocalData))
				{
					try
					{
						var doc = XDocument.Load(pathLocalData);

						var root = doc.Root;
						if (root == null) throw new Exception("Root == null");

						d.Deserialize(root);

						return d;
					}
					catch (Exception e)
					{
						throw new Exception("Could not load synchronization state from '" + pathLocalData + "'", e);
					}
				}
				else
				{
					WriteSyncData(d, pathLocalData);
					return d;
				}
			}
		}

		public void WriteSyncData(IRemoteStorageSyncPersistance data)
		{
			lock (_lockSaveSyncData)
			{
				WriteSyncData(data, pathLocalData);
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
				if (File.Exists(pathLocalData))
				{
					logger.Info("Repository", "Delete file from local repository: " + Path.GetFileName(pathLocalData), pathLocalData);
					File.Delete(pathLocalData);
				}
			}

			var noteFiles = Directory.GetFiles(pathLocalFolder, "*.xml");
			foreach (var path in noteFiles)
			{
				logger.Info("Repository", "Delete file from local repository: " + Path.GetFileName(path), path);
				File.Delete(path);
			}

			logger.Info("Repository", "Delete folder from local repository: " + Path.GetFileName(pathLocalFolder), pathLocalFolder);
			Directory.Delete(pathLocalFolder, true);
		}

		public IEnumerable<string> EnumerateAllTags()
		{
			return Notes.SelectMany(n => n.Tags);
		}

		public void ReplaceSettings(AppSettings settings)
		{
			appconfig = settings;
		}

		public void ApplyNewAccountData(RemoteStorageAccount acc, IRemoteStorageSyncPersistance data, List<INote> notes)
		{
			var localFolder = Path.Combine(pathLocalBase, acc.ID.ToString("B"));
			var localData   = Path.Combine(pathLocalBase, acc.ID.ToString("B") + ".xml");

			if (!Directory.Exists(localFolder))
			{
				logger.Info("Repository", "Create local note folder: " + localFolder);
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

			LocalGitBackup.UpdateRepository(this, appconfig, logger);
		}

		public void SyncError(List<Tuple<string, Exception>> errors)
		{
			// [event from sync thread]

			LocalGitBackup.UpdateRepository(this, appconfig, logger);
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
