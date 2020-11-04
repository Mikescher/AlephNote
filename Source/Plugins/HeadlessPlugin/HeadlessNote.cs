using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Datatypes;
using AlephNote.PluginInterface.Util;
using MSHC.Lang.Collections;

namespace AlephNote.Plugins.Headless
{
	class HeadlessNote : BasicHierachicalNote
	{
		private Guid _id;

		private string _title = "";
		public override string Title {get { return _title; } set { _title = value; OnPropertyChanged(); }}

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private DirectoryPath _path = DirectoryPath.Root();
		public override DirectoryPath Path { get { return _path; } set { _path = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } }
		public void SetModificationDate(DateTimeOffset value) { _modificationDate = value; OnPropertyChanged(); }

		private DateTimeOffset _creationDate = DateTimeOffset.Now;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private bool _isPinned = false;
		public override bool IsPinned { get { return _isPinned; } set { _isPinned = value; OnPropertyChanged(); } }

		private bool _isLocked = false;
		public override bool IsLocked { get { return _isLocked; } set { _isLocked = value; OnPropertyChanged(); } }

		private readonly SimpleTagList _tags = new SimpleTagList();

		public override TagList Tags { get { return _tags; } }

		public HeadlessNote(Guid uid)
		{
			_id = uid;
		}

		public override string UniqueName => _id.ToString("B");

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Title", Title),
				new XElement("Text", XHelper.ConvertToC80Base64(Text)),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("ModificationDate", XHelper.ToString(ModificationDate)),
				new XElement("CreationDate", XHelper.ToString(CreationDate)),
				new XElement("Path", Path.Serialize()),
				new XElement("IsPinned", IsPinned),
				new XElement("IsLocked", IsLocked),
			};

			var r = new XElement("localnote", data);
			r.SetAttributeValue("plugin", HeadlessPlugin.Name);
			r.SetAttributeValue("pluginversion", HeadlessPlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			_id = XHelper.GetChildValueGUID(input, "ID");
			_title = XHelper.GetChildValueString(input, "Title");
			_text = XHelper.GetChildBase64String(input, "Text");
			_path = DirectoryPath.Deserialize(XHelper.GetChildrenOrEmpty(input, "Path", "PathComponent"));
			_tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
			_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
			_isPinned = XHelper.GetChildValue(input, "IsPinned", false);
			_isLocked = XHelper.GetChildValue(input, "IsLocked", false);
			_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
		}

		protected override BasicNoteImpl CreateClone()
		{
			var n = new HeadlessNote(_id);

			using (n.SuppressDirtyChanges())
			{
				n._title            = _title;
				n._text             = _text;
				n._tags.Synchronize(_tags);
				n._path             = _path;
				n._isPinned         = _isPinned;
				n._isLocked         = _isLocked;
				n._creationDate     = _creationDate;
				n._modificationDate = _modificationDate;

				return n;
			}
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (HeadlessNote)iother;

			using (SuppressDirtyChanges())
			{
				_title = other.Title;
				_text = other.Text;
				_tags.Synchronize(other.Tags);
				_path = other._path;
				_isPinned = other._isPinned;
				_isLocked = other._isLocked;
			}
		}

		public override void OnAfterUpload(INote clonenote)
		{
			//
		}

        public override void UpdateModificationDate(string propSource, bool clearConflictFlag)
        {
			SetModificationDate(DateTimeOffset.Now);
			if (clearConflictFlag && IsConflictNote) IsConflictNote = false;
        }
    }
}
