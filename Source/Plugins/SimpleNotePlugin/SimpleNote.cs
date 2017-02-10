using CommonNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Util.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CommonNote.Plugins.SimpleNote
{
	class SimpleNote : BasicNote
	{
		private string _id;
		public string ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private bool _deleted = false;
		public bool Deleted { get { return _deleted; } set { _deleted = value; OnPropertyChanged(); } }

		private string _shareURL = "";
		public string ShareURL { get { return _shareURL; } set { _shareURL = value; OnPropertyChanged(); } }

		private string _publicURL = "";
		public string PublicURL { get { return _publicURL; } set { _publicURL = value; OnPropertyChanged(); } }

		private List<string> _systemTags = new List<string>();
		public List<string> SystemTags { get { return _systemTags; } set { _systemTags = value; OnPropertyChanged(); } }

		private string _content = "";
		public string Content { get { return _content; } set { _content = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.MinValue;
		public DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private int _version;
		public int Version { get { return _version; } set { _version = value; OnPropertyChanged(); } }

		public SimpleNote(string uid)
		{
			_id = uid;
		}

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		public override string Text
		{
			get
			{
				var lines = _content.Split('\n');
				if (lines.Length <= 1) return string.Empty;

				return string.Join("\n", lines.Skip(1));
			}
			set
			{
				var lines = _content.Split('\n');
				if (lines.Length == 0)
				{
					_content = value;
				}
				else
				{
					_content = lines[0] + "\n" + value;
				}
				OnPropertyChanged();
			}
		}

		public override string Title
		{
			get
			{
				return _content.Split('\n').FirstOrDefault() ?? string.Empty;
			}
			set
			{
				var lines = _content.Split('\n');
				if (lines.Length == 0)
				{
					_content = value;
				}
				else
				{
					lines[0] = value;
					_content = string.Join("\n", lines);
				}
				OnPropertyChanged();
			}
		}


		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		public override string GetUniqueName()
		{
			return _id;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("Deleted", _deleted),
				new XElement("ShareURL", _shareURL),
				new XElement("PublishURL", _publicURL),
				new XElement("SystemTags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("Content", Convert.ToBase64String(Encoding.UTF8.GetBytes(_content))),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", _creationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("Version", _version),
			};

			var r = new XElement("simplenote", data);
			r.SetAttributeValue("plugin", "SimpleNotePlugin");
			r.SetAttributeValue("pluginversion", SimpleNotePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			using (SuppressDirtyChanges())
			{
				_id = XHelper.GetChildValueString(input, "ID");
				_tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
				_deleted = XHelper.GetChildValueBool(input, "Deleted");
				_shareURL = XHelper.GetChildValueString(input, "ShareURL");
				_publicURL = XHelper.GetChildValueString(input, "PublishURL");
				_systemTags = XHelper.GetChildValueStringList(input, "SystemTags", "Tag");
				_content = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Content")));
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_version = XHelper.GetChildValueInt(input, "Version");
			}
		}

		protected override BasicNote CreateClone()
		{
			var n = new SimpleNote(_id);
			n._tags.Synchronize(_tags.ToList());
			n._content = _content;
			n._deleted = _deleted;
			n._shareURL = _shareURL;
			n._publicURL = _publicURL;
			n._systemTags = _systemTags.ToList();
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			n._version = _version;
			return n;
		}

		public override void OnAfterUpload(INote clonenote)
		{
			var other = (SimpleNote) clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate = other.CreationDate;
				_version = other.Version;
				_systemTags = other.SystemTags;
				_publicURL = other.PublicURL;
				_shareURL = other.ShareURL;
				_deleted = other.Deleted;
			}
		}

		public override void ApplyUpdatedData(INote clonenote)
		{
			var other = (SimpleNote)clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_version = other.Version;
				_systemTags = other.SystemTags;
				_publicURL = other.PublicURL;
				_shareURL = other.ShareURL;
				_deleted = other.Deleted;
				_content = other.Content;
			}
		}
	}
}
