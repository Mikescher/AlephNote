using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Datatypes;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Nextcloud
{
	public class NextcloudNote : BasicHierachicalNote
	{
		private int _remoteID;
		public int RemoteID { get { return _remoteID; } set { _remoteID = value; OnPropertyChanged(); } }

		private Guid _localID;
		public Guid LocalID { get { return _localID; } set { _localID = value; OnPropertyChanged(); } }

		private string _content = "";
		public string Content { get { return _content ?? string.Empty; } set { _content = value ?? string.Empty; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.Now;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly VoidTagList _tags = new VoidTagList();
		public override TagList Tags { get { return _tags; } }

		private DirectoryPath _path = DirectoryPath.Root();
		public override DirectoryPath Path { get { return _path; } set { _path = value; OnPropertyChanged(); } }

		public override bool IsPinned { get { return false; } set { /* no */ } }

		public override bool IsLocked { get { return false; } set { /* no */ } }

		private int _remoteTimestamp = -1;
		public int RemoteTimestamp { get { return _remoteTimestamp; } set { _remoteTimestamp = value; OnPropertyChanged(); } }

		private bool _favorite = false;
		public bool Favorite { get { return _favorite; } set { _favorite = value; OnPropertyChanged(); } }

		private string _etag = "";
		public string ETag { get { return _etag; } set { _etag = value??""; OnPropertyChanged(); } }

		private readonly NextcloudConfig _config;

		public NextcloudNote(int rid, Guid lid, NextcloudConfig cfg)
		{
			_remoteID = rid;
			_localID = lid;
			_config = cfg;
		}

		public override string Text
		{
			get
			{
				var lines = _content.Replace("\r\n", "\n").Split('\n');
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
				return _content.Replace("\r\n", "\n").Split('\n').FirstOrDefault() ?? string.Empty;
			}
			set
			{
				var lines = _content.Replace("\r\n", "\n").Split('\n');
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

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("RemoteID", _remoteID),
				new XElement("LocalID", _localID),
				new XElement("Content", XHelper.ConvertToC80Base64(_content)),
				new XElement("ModificationDate", XHelper.ToString(ModificationDate)),
				new XElement("CreationDate", XHelper.ToString(_creationDate)),
				new XElement("Timestamp", _remoteTimestamp),
				new XElement("Favorite", _favorite),
				new XElement("ETag", _etag),
				new XElement("Path", Path.Serialize().Select(p=>(object)p).ToArray()),
			};

			var r = new XElement("nextcloudnote", data);
			r.SetAttributeValue("plugin", NextcloudPlugin.Name);
			r.SetAttributeValue("pluginversion", NextcloudPlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			using (SuppressDirtyChanges())
			{
				_remoteID = XHelper.GetChildValueInt(input, "RemoteID");
				_localID = XHelper.GetChildValueGUID(input, "LocalID");
				_content = XHelper.GetChildBase64String(input, "Content");
				Path = DirectoryPath.Deserialize(XHelper.GetChildrenOrEmpty(input, "Path", "PathComponent"));
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_remoteTimestamp = XHelper.GetChildValueInt(input, "Timestamp");
				_favorite = XHelper.GetChildValue(input, "Favorite", false);
				_etag = XHelper.GetChildValue(input, "ETag", "");
			}
		}

		public override string UniqueName => _localID.ToString("N");

		public override void OnAfterUpload(INote iother)
		{
			var other = (NextcloudNote)iother;

			using (SuppressDirtyChanges())
			{
				if (Title != other.Title) Title = other.Title;

				_modificationDate = other.ModificationDate;
				_remoteTimestamp = other.RemoteTimestamp;
				_remoteID = other.RemoteID;
				_favorite = other.Favorite;
				_etag = other.ETag;
			}
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (NextcloudNote)iother;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_tags.Synchronize(other.Tags);
				_content = other.Content;
				_path = other._path;
				_remoteTimestamp = other.RemoteTimestamp;
				_remoteID = other.RemoteID;
				_favorite = other._favorite;
				_etag = other._etag;
			}
		}

		protected override BasicHierachicalNote CreateClone()
		{
			var n = new NextcloudNote(_remoteID, _localID, _config);
			
			using (n.SuppressDirtyChanges())
			{
				n._content          = _content;
				n._path             = _path;
				n._creationDate     = _creationDate;
				n._modificationDate = _modificationDate;
				n._remoteTimestamp  = _remoteTimestamp;
				n._favorite         = _favorite;
				n._etag             = _etag;

				return n;
			}
		}
	}
}
