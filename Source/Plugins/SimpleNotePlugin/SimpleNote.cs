using CommonNote.PluginInterface;
using System;
using System.Collections.Generic;

namespace CommonNote.Plugins.SimpleNote
{
	class SimpleNote : INote
	{
		public string ID = "";
		public List<string> Tags = new List<string>();
		public bool Deleted = false;
		public string ShareURL = "";
		public string PublishURL = "";
		public List<string> SystemTags = new List<string>();
		public string Content = "";
		public DateTimeOffset ModificationDate = DateTime.MinValue;
		public DateTimeOffset CreationDate = DateTime.MinValue;
		public int Version;
	}
}
