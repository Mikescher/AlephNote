using CommonNote.PluginInterface;
using CommonNote.Settings;
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

namespace CommonNote.Repository
{
	public class NoteRepository : ObservableObject
	{
		private readonly string pathLocal;
		private readonly IRemoteProvider provider;
		private readonly IRemoteStorageConnection conn;
		private readonly AppSettings appconfig;
		private readonly SynchronizationThread thread;
		private readonly ISynchronizationFeedback listener;

		public readonly List<INote> LocalDeletedNotes = new List<INote>(); // deleted local but not on remote

		private readonly ObservableCollection<INote> _notes = new ObservableCollectionNoReset<INote>();
		public ObservableCollection<INote> Notes { get { return _notes; } }

		private readonly DelayedCombiningInvoker invSaveNotesLocal;

		public IRemoteStorageConnection Connection { get { return conn; } }

		public string ConnectionName { get { return provider.DisplayTitleShort; } }

		public NoteRepository(string path, ISynchronizationFeedback fb, AppSettings cfg, IRemoteProvider prov, IRemoteStorageConfiguration config)
		{
			pathLocal = Path.Combine(path, prov.GetUniqueID().ToString("B"), config.GetUniqueName());
			conn = prov.CreateRemoteStorageConnection(cfg.CreateProxy(), config);
			provider = prov;
			appconfig = cfg;
			listener = fb;
			thread = new SynchronizationThread(this, fb);

			invSaveNotesLocal = DelayedCombiningInvoker.Create(() => Application.Current.Dispatcher.BeginInvoke(new Action(SaveAllDirtyNotes)), 1 * 1000, 60 * 1000);

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

			thread.Stop();
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

					var note = provider.CreateEmptyNode();
					note.Deserialize(data.Elements().FirstOrDefault());

					note.ResetLocalDirty();
					note.ResetRemoteDirty();

					Notes.Add(note);
				}
				catch (Exception e)
				{
					MessageBox.Show("Cannot load note from '" + noteFile + "'.\r\n\r\n" + e);
				}
			}
		}

		public INote CreateNewNote()
		{
			var note = provider.CreateEmptyNode();
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
			meta.Add("date", DateTime.Now.ToString("O"));
			meta.Add("provider", provider.GetUniqueID().ToString("B"));
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

		private void NoteChanged(object sender, EventArgs e)
		{
			invSaveNotesLocal.Request();
			listener.OnSyncRequest();
		}

		public void DeleteNote(INote note, bool updateRemote)
		{
			var found = Notes.Remove(note);

			if (found && updateRemote)
			{
				LocalDeletedNotes.Add(note);
				thread.SyncNow();
			}
		}

		public void AddNote(INote note, bool updateRemote)
		{
			Notes.Add(note);
			invSaveNotesLocal.Request();

			if (updateRemote)
			{
				thread.SyncNow();
			}
		}

		public void SyncNow()
		{
			thread.SyncNow();
		}
	}
}
