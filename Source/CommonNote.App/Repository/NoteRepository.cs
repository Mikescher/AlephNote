using CommonNote.PluginInterface;
using MSHC.WPF.MVVM;
using System;
using System.Collections.ObjectModel;
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

		private ObservableCollection<INote> _notes = new ObservableCollection<INote>();
		public ObservableCollection<INote> Notes { get { return _notes; } }

		public NoteRepository(string path, IRemoteProvider prov, IRemoteStorageConfiguration config)
		{
			pathLocal = Path.Combine(path, prov.GetUniqueID().ToString("B"), config.GetUniqueName());
			conn = prov.CreateRemoteStorageConnection(config);
			provider = prov;
		}

		public void Init()
		{
			if (!Directory.Exists(pathLocal)) Directory.CreateDirectory(pathLocal);

			LoadNotesFromLocal();
		}

		public void Shutdown()
		{
			
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

					Notes.Add(note);
				}
				catch (Exception e)
				{
					MessageBox.Show("Cannot load note from '" + noteFile + "'.\r\n\r\n" + e);
				}
			}
		}
	}
}
