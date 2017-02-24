using AlephNote.PluginInterface;
using MSHC.Lang.Collections;
using MSHC.Serialization;
using MSHC.Util.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AlephNote.Plugins.Headless
{
	class HeadlessNote : BasicNote
	{
		private Guid _id;

		private string _title = "";
		public override string Title {get { return _title; } set { _title = value; OnPropertyChanged(); }}

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.Now;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();

		public override ObservableCollection<string> Tags { get { return _tags; } }

		public HeadlessNote(Guid uid)
		{
			_id = uid;
		}

		public override string GetUniqueName()
		{
			return _id.ToString("B");
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Title", Title),
				new XElement("Text", Convert.ToBase64String(Encoding.UTF8.GetBytes(Text))),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
			};

			var r = new XElement("localnote", data);
			r.SetAttributeValue("plugin", HeadlessPlugin.Name);
			r.SetAttributeValue("pluginversion", HeadlessPlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			_id = XHelper.GetChildValueGUID(input, "ID");
			Title = XHelper.GetChildValueString(input, "Title");
			Text = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Text")));
			Tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
			CreationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
			ModificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
		}

		protected override BasicNote CreateClone()
		{
			var n = new HeadlessNote(_id);
			n._title = _title;
			n._text = _text;
			n._tags.Synchronize(_tags);
			return n;
		}

		public override void ApplyUpdatedData(INote other)
		{
			using (SuppressDirtyChanges())
			{
				_title = other.Title;
				_text = other.Text;
				_tags.Synchronize(other.Tags);
			}
		}

		public override void OnAfterUpload(INote clonenote)
		{
			//
		}
	}
}
