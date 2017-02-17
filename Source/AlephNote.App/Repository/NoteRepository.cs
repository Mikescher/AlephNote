using System.ComponentModel;
using AlephNote.PluginInterface;
using AlephNote.Settings;
using AlephNote.WPF.Windows;
using MSHC.Util.Helper;
using MSHC.Util.Threads;
using MSHC.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace AlephNote.Repository
{
	public class NoteRepository : ObservableObject
	{
		private readonly string pathLocal;
		private readonly IRemoteProvider provider;
		private readonly IRemoteStorageConnection conn;
		private readonly IRemoteStorageConfiguration remoteconfig;
		private readonly AppSettings appconfig;
		private readonly SynchronizationThread thread;
		private readonly ISynchronizationFeedback listener;

		public readonly List<INote> LocalDeletedNotes = new List<INote>(); // deleted local but not on remote

		private readonly ObservableCollection<INote> _notes = new ObservableCollectionNoReset<INote>();
		public ObservableCollection<INote> Notes { get { return _notes; } }

		private readonly DelayedCombiningInvoker invSaveNotesLocal;
		private readonly DelayedCombiningInvoker invSaveNotesRemote;

		public IRemoteStorageConnection Connection { get { return conn; } }

		public string ConnectionName { get { return provider.DisplayTitleShort; } }

		public NoteRepository(string path, ISynchronizationFeedback fb, AppSettings cfg, IRemoteProvider prov, IRemoteStorageConfiguration config)
		{
			pathLocal = Path.Combine(path, prov.GetUniqueID().ToString("B"), FilenameHelper.ConvertStringForFilename(config.GetUniqueName()));
			conn = prov.CreateRemoteStorageConnection(cfg.CreateProxy(), config);
			remoteconfig = config;
			provider = prov;
			appconfig = cfg;
			listener = fb;
			thread = new SynchronizationThread(this, fb, cfg.ConflictResolution);

			invSaveNotesLocal = DelayedCombiningInvoker.Create(() => Application.Current.Dispatcher.BeginInvoke(new Action(SaveAllDirtyNotes)),  1 * 1000,  1 * 60 * 1000);
			invSaveNotesRemote = DelayedCombiningInvoker.Create(() => Application.Current.Dispatcher.BeginInvoke(new Action(SyncNow)),          30 * 1000, 15 * 60 * 1000);

			_notes.CollectionChanged += NoteCollectionChanged;
		}

		public void Init()
		{
			if (!Directory.Exists(pathLocal)) Directory.CreateDirectory(pathLocal);

			LoadNotesFromLocal();

			thread.Start(appconfig.GetSyncDelay());
		}

		public void Shutdown()
		{
			invSaveNotesLocal.CancelPendingRequests();
			SaveAllDirtyNotes();

			thread.SyncNowAndStopAsync();
		}

		private void LoadNotesFromLocal()
		{
			var noteFiles = Directory.GetFiles(pathLocal, "*.xml");

			foreach (var noteFile in noteFiles)
			{
				try
				{
					var doc = XDocument.Load(noteFile);

					var root = doc.Root;
					if (root == null) throw new Exception("Root == null");

					var data = root.Element("data");
					if (data == null) throw new Exception("missing data node");

					var note = provider.CreateEmptyNote(remoteconfig);
					note.Deserialize(data.Elements().FirstOrDefault());
					note.ResetLocalDirty();
					note.ResetRemoteDirty();
					
					var meta = root.Element("meta");
					if (meta == null) throw new Exception("missing meta node");

					if (XHelper.GetChildValue(meta, "dirty", false)) note.SetRemoteDirty();
					note.IsConflictNote = XHelper.GetChildValue(meta, "conflict", false);

					Notes.Add(note);
				}
				catch (Exception e)
				{
					ExceptionDialog.Show(null, "LoadNotes from local cache", "Could not load note from '" + noteFile + "'", e);
				}
			}
		}

		public INote CreateNewNote()
		{
			var note = provider.CreateEmptyNote(remoteconfig);
			Notes.Add(note);
			note.SetDirty();
			SaveNote(note);

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
			var path = Path.Combine(pathLocal, note.GetUniqueName() + ".xml");

			var root = new XElement("note");

			var meta = new XElement("meta");
			meta.Add(new XElement("date", DateTime.Now.ToString("O")));
			meta.Add(new XElement("provider", provider.GetUniqueID().ToString("B")));
			meta.Add(new XElement("dirty", !note.IsRemoteSaved));
			meta.Add(new XElement("conflict", note.IsConflictNote));
			root.Add(meta);

			root.Add(new XElement("data", note.Serialize()));

			new XDocument(root).Save(path);

			note.ResetLocalDirty();
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

			listener.OnSyncRequest();

			listener.OnNoteChanged(e);
		}

		public void DeleteNote(INote note, bool updateRemote)
		{
			var found = Notes.Remove(note);

			var path = Path.Combine(pathLocal, note.GetUniqueName() + ".xml");
			if (File.Exists(path)) File.Delete(path);

			if (found && updateRemote)
			{
				LocalDeletedNotes.Add(note);
				thread.SyncNowAsync();
			}
		}

		public void AddNote(INote note, bool updateRemote)
		{
			Notes.Add(note);
			SaveNote(note);

			invSaveNotesLocal.Request();

			if (updateRemote)
			{
				thread.SyncNowAsync();
			}
		}

		public void SyncNow()
		{
			thread.SyncNowAsync();
		}

		public void SaveAll()
		{
			SaveAllDirtyNotes();
		}
	}
}
