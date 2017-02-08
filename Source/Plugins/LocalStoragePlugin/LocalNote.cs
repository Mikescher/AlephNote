using CommonNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MSHC.Util.Helper;

namespace CommonNote.Plugins.LocalStorage
{
	class LocalNote : BasicNote
	{
		public string Title = "";
		public string Text = "";
		public List<string> Tags = new List<string>();
		public DateTimeOffset ModificationDate = DateTimeOffset.Now;
		public DateTimeOffset CreationDate = DateTimeOffset.Now;
		
		public override string GetText()
		{
			return Text;
		}

		public override string GetTitle()
		{
			return Title;
		}

		public override IEnumerable<string> GetTags()
		{
			return Tags;
		}

		public override DateTimeOffset GetLastModified()
		{
			return ModificationDate;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("Title", Title),
				new XElement("Text", Convert.ToBase64String(Encoding.UTF8.GetBytes(Text))),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
			};

			var r = new XElement("note", data);
			r.SetAttributeValue("plugin", "LocalStoragePlugin");
			r.SetAttributeValue("pluginversion", LocalStoragePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			Title = XHelper.GetChildValueString(input, "Title");
			Text = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Text")));
			Tags = XHelper.GetChildValueStringList(input, "Tags", "Tag");
			CreationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
			ModificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
		}
	}
}
