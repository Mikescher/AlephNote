using CommonNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Util.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CommonNote.Plugins.LocalStorage
{
	class LocalNote : BasicNote
	{
		public Guid ID;
		public DateTimeOffset CreationDate = DateTimeOffset.Now;

		private string _title = "";
		public override string Title {get { return _title; } set { _title = value; OnPropertyChanged(); }}

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		public LocalNote(Guid uid)
		{
			ID = uid;
		}

		public override string GetLocalUniqueName()
		{
			return ID.ToString("B");
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", ID),
				new XElement("Title", Title),
				new XElement("Text", Convert.ToBase64String(Encoding.UTF8.GetBytes(Text))),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
			};

			var r = new XElement("localnote", data);
			r.SetAttributeValue("plugin", "LocalStoragePlugin");
			r.SetAttributeValue("pluginversion", LocalStoragePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			ID = XHelper.GetChildValueGUID(input, "ID");
			Title = XHelper.GetChildValueString(input, "Title");
			Text = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Text")));
			Tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
			CreationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
			ModificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
		}
	}
}
