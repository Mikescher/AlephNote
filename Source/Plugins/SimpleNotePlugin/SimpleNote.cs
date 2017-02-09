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
		public string ID;
		public bool Deleted = false;
		public string ShareURL = "";
		public string PublishURL = "";
		public List<string> SystemTags = new List<string>();
		public string Content = "";
		public DateTimeOffset CreationDate = DateTime.MinValue;
		public int Version;

		public SimpleNote(string uid)
		{
			ID = uid;
		}

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		public override string Text
		{
			get
			{
				var lines = Content.Split('\n');
				if (lines.Length <= 1) return string.Empty;

				return string.Join("\n", lines.Skip(1));
			}
			set
			{
				var lines = Content.Split('\n');
				if (lines.Length == 0)
				{
					Content = value;
				}
				else
				{
					Content = lines[0] + "\n" + value;
				}
				OnPropertyChanged();
			}
		}

		public override string Title
		{
			get
			{
				return Content.Split('\n').FirstOrDefault() ?? string.Empty;
			}
			set
			{
				var lines = Content.Split('\n');
				if (lines.Length == 0)
				{
					Content = value;
				}
				else
				{
					lines[0] = value;
					Content = string.Join("\n", lines);
				}
				OnPropertyChanged();
			}
		}


		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		public override string GetLocalUniqueName()
		{
			return ID;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", ID),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("Deleted", Deleted),
				new XElement("ShareURL", ShareURL),
				new XElement("PublishURL", PublishURL),
				new XElement("SystemTags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("Content", Convert.ToBase64String(Encoding.UTF8.GetBytes(Content))),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("Version", Version),
			};

			var r = new XElement("simplenote", data);
			r.SetAttributeValue("plugin", "SimpleNotePlugin");
			r.SetAttributeValue("pluginversion", SimpleNotePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			ID = XHelper.GetChildValueString(input, "ID");
			Tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
			Deleted = XHelper.GetChildValueBool(input, "Deleted");
			ShareURL = XHelper.GetChildValueString(input, "ShareURL");
			PublishURL = XHelper.GetChildValueString(input, "PublishURL");
			SystemTags = XHelper.GetChildValueStringList(input, "SystemTags", "Tag");
			Content = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Content")));
			CreationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
			ModificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
			Version = XHelper.GetChildValueInt(input, "Version");
		}
	}
}
