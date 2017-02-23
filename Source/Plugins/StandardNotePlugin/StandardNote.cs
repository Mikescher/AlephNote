using AlephNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Util.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNote : BasicNote
	{
		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _title = "";
		public override string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		private string _encryptionKey = "";
		public string EncryptionKey { get { return _encryptionKey; } set { _encryptionKey = value; OnPropertyChanged(); } }

		private readonly StandardNoteConfig _config;

		public StandardNote(Guid uid, StandardNoteConfig cfg)
		{
			_id = uid;
			_config = cfg;
			_creationDate = DateTimeOffset.Now;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("Text", Convert.ToBase64String(Encoding.UTF8.GetBytes(_text))),
				new XElement("Title", _title),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", _creationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("Key", _encryptionKey),
			};

			var r = new XElement("standardnote", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			using (SuppressDirtyChanges())
			{
				_id = XHelper.GetChildValueGUID(input, "ID");
				_tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
				_text = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Text")));
				_title = XHelper.GetChildValueString(input, "Title");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_encryptionKey = XHelper.GetChildValueString(input, "Key");
			}
		}

		public override string GetUniqueName()
		{
			return _id.ToString("N");
		}

		public override void OnAfterUpload(INote iother)
		{
			throw new NotImplementedException();
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (StandardNote)iother;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate = other.CreationDate;
				_tags.Synchronize(other.Tags);
				_text = other.Text;
				_title = other.Title;
				_encryptionKey = other.EncryptionKey;
			}
		}

		protected override BasicNote CreateClone()
		{
			var n = new StandardNote(_id, _config);
			n._tags.Synchronize(_tags.ToList());
			n._text = _text;
			n._title = _title;
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			n._encryptionKey = _encryptionKey;
			return n;
		}
	}
}
