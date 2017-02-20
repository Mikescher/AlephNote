using AlephNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Util.Helper;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Plugins.Filesystem
{
	class FilesystemNote : BasicNote
	{
		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _title = "";
		public override string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

		private string _pathBackup = "";
		public string PathBackup { get { return _pathBackup; } set { _pathBackup = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.MinValue;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly FilesystemConfig _config;

		public FilesystemNote(Guid uid, FilesystemConfig cfg)
		{
			_id = uid;
			_config = cfg;
		}

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		public string GetPath(FilesystemConfig cfg)
		{
			return Path.Combine(cfg.Folder, Title + cfg.Extension);
		}

		public override string GetUniqueName()
		{
			return _id.ToString("B");
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id.ToString("D")),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("Text", Convert.ToBase64String(Encoding.UTF8.GetBytes(_text))),
				new XElement("Title", Convert.ToBase64String(Encoding.UTF8.GetBytes(_title))),
				new XElement("PathBackup", _pathBackup),
				new XElement("CreationDate", _creationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("ModificationDate", _modificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
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
				_tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
				_text = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Text")));
				_title = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Title")));
				_pathBackup = XHelper.GetChildValueString(input, "PathBackup");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
			}
		}

		protected override BasicNote CreateClone()
		{
			var n = new FilesystemNote(_id, _config);
			n._tags.Synchronize(_tags.ToList());
			n._text = _text;
			n._title = _title;
			n._pathBackup = _pathBackup;
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			return n;
		}

		public override void OnAfterUpload(INote clonenote)
		{
			var other = (FilesystemNote) clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate     = other.CreationDate;
				_pathBackup       = other._pathBackup;
			}
		}

		public override void ApplyUpdatedData(INote clonenote)
		{
			var other = (FilesystemNote)clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate     = other.CreationDate;
				_text             = other.Text;
				_title            = other.Title;
				_pathBackup       = other.PathBackup;
			}
		}
	}
}
