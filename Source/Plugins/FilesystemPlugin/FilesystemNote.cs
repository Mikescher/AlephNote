using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;
using System.Linq;
using AlephNote.PluginInterface.Datatypes;

namespace AlephNote.Plugins.Filesystem
{
	class FilesystemNote : BasicHierachicalNote
	{
		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _title = "";
		public override string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

		private DirectoryPath _path = DirectoryPath.Root();
		public override DirectoryPath Path { get { return _path; } set { _path = value; OnPropertyChanged(); } }

		private string _pathRemote = "";
		public string PathRemote { get { return _pathRemote; } set { _pathRemote = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.MinValue;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly VoidTagList _tags = new VoidTagList();
		public override TagList Tags { get { return _tags; } }

		public override bool IsPinned { get { return false; } set { /* no */ } }

		private bool _isLocked = false;
		public override bool IsLocked { get { return _isLocked; } set { _isLocked = value; OnPropertyChanged(); } }

		private readonly FilesystemConfig _config;

		public FilesystemNote(Guid uid, FilesystemConfig cfg)
		{
			_id = uid;
			_config = cfg;
			_creationDate = DateTimeOffset.Now;
		}

		public string GetPath(FilesystemConfig cfg)
		{
			var dat = new[] { cfg.Folder }
				.Concat(Path.Enumerate().Select(p => CleanForFS(p, p)))
				.Concat(new[] { CleanForFS(Title, UniqueName) + "." + cfg.Extension });

			return System.IO.Path.Combine(dat.ToArray());
		}

		private static string CleanForFS(string str, string uniq)
		{
			var fn = ANFilenameHelper.StripStringForFilename(str, '_');
			if (string.IsNullOrWhiteSpace(fn)) fn = ANFilenameHelper.StripStringForFilename(uniq);
			return fn;
		}

		public override string UniqueName => _id.ToString("B");

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id.ToString("D")),
				new XElement("Text", XHelper.ConvertToC80Base64(_text)),
				new XElement("Title", Convert.ToBase64String(Encoding.UTF8.GetBytes(_title))),
				new XElement("PathRemote", _pathRemote),
				new XElement("CreationDate", XHelper.ToString(_creationDate)),
				new XElement("ModificationDate", XHelper.ToString(_modificationDate)),
				new XElement("Path", Path.Serialize()),
				new XElement("IsLocked", IsLocked),
			};

			var r = new XElement("fsnote", data);
			r.SetAttributeValue("plugin", FilesystemPlugin.Name);
			r.SetAttributeValue("pluginversion", FilesystemPlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			using (SuppressDirtyChanges())
			{
				_id = XHelper.GetChildValueGUID(input, "ID");
				_text = XHelper.GetChildBase64String(input, "Text");
				_title = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Title")));
				_path = DirectoryPath.Deserialize(XHelper.GetChildrenOrEmpty(input, "Path", "PathComponent"));
				_pathRemote = XHelper.GetChildValueString(input, "PathRemote");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_isLocked = XHelper.GetChildValue(input, "IsLocked", false);
			}
		}

		protected override BasicNoteImpl CreateClone()
		{
			var n = new FilesystemNote(_id, _config);

			using (n.SuppressDirtyChanges())
			{
				n._text             = _text;
				n._title            = _title;
				n._path             = _path;
				n._pathRemote       = _pathRemote;
				n._creationDate     = _creationDate;
				n._modificationDate = _modificationDate;
				n._isLocked         = _isLocked;
				return n;
			}
		}

		public override void OnAfterUpload(INote clonenote)
		{
			var other = (FilesystemNote) clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_pathRemote       = other._pathRemote;
			}
		}

		public override void ApplyUpdatedData(INote clonenote)
		{
			var other = (FilesystemNote)clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_text             = other.Text;
				_title            = other.Title;
				_path             = other.Path;
				_pathRemote       = other.PathRemote;
				_isLocked         = other.IsLocked;
			}
		}
	}
}
