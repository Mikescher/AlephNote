using AlephNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Util.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Plugins.SimpleNote
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
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private int _localVersion;
		public int LocalVersion { get { return _localVersion; } set { _localVersion = value; OnPropertyChanged(); } }

		private readonly SimpleNoteConfig _config;

		public SimpleNote(string uid, SimpleNoteConfig cfg)
		{
			_id = uid;
			_config = cfg;
		}

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		public override string Text
		{
			get
			{
				var lines = _content.Split('\n');
				if (lines.Length == 0) return string.Empty;
				if (lines.Length == 1) return string.Empty;
				if (lines.Length == 2) return lines[1];

				if (_config.BlankLineBelowTitle)
				{
					if (!string.IsNullOrWhiteSpace(lines[1])) return string.Join("\n", lines.Skip(1));

					return string.Join("\n", lines.Skip(2));
				}
				else
				{
					return string.Join("\n", lines.Skip(1));
				}

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
					if (_config.BlankLineBelowTitle)
					{
						_content = lines[0] + "\n" + "\n" + value;
					}
					else
					{
						_content = lines[0] + "\n" + value;
					}
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
				else if (lines.Length == 1)
				{
					_content = value;
				}
				else
				{
					if (_config.BlankLineBelowTitle)
					{
						if (lines.Length >= 2 && string.IsNullOrWhiteSpace(lines[1]))
						{
							_content = value + "\n" + "\n" + string.Join("\n", lines.Skip(2));
						}
						else
						{
							_content = value + "\n" + "\n" + string.Join("\n", lines.Skip(1));
						}
					}
					else
					{
						lines[0] = value;
						_content = string.Join("\n", lines);
					}
				}
				OnPropertyChanged();
			}
		}

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
				new XElement("LocalVersion", _localVersion),
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
				_localVersion = XHelper.GetChildValueInt(input, "LocalVersion");
			}
		}

		protected override BasicNote CreateClone()
		{
			var n = new SimpleNote(_id, _config);
			n._tags.Synchronize(_tags.ToList());
			n._content = _content;
			n._deleted = _deleted;
			n._shareURL = _shareURL;
			n._publicURL = _publicURL;
			n._systemTags = _systemTags.ToList();
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			n._localVersion = _localVersion;
			return n;
		}

		public override void OnAfterUpload(INote clonenote)
		{
			var other = (SimpleNote) clonenote;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate = other.CreationDate;
				_localVersion = other.LocalVersion;
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
				_localVersion = other.LocalVersion;
				_systemTags = other.SystemTags;
				_publicURL = other.PublicURL;
				_shareURL = other.ShareURL;
				_deleted = other.Deleted;
				_content = other.Content;
			}
		}
	}
}
