
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CommonNote.PluginInterface
{
	public interface INote
	{
		XElement Serialize();
		void Deserialize(XElement input);

		string GetText();
		string GetTitle();
		IEnumerable<string> GetTags();
		DateTimeOffset GetLastModified();
	}

	public abstract class BasicNote : INote
	{
		public abstract XElement Serialize();
		public abstract void Deserialize(XElement input);

		public abstract string GetText();
		public abstract string GetTitle();
		public abstract IEnumerable<string> GetTags();
		public abstract DateTimeOffset GetLastModified();
	}
}
