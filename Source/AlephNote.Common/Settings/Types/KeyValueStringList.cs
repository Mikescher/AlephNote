using AlephNote.Common.AlephXMLSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Settings.Types
{
	public class KeyValueStringList : ObservableObject, IAlephCustomSerializableField
	{
		public readonly IReadOnlyCollection<Tuple<string, string>> Data;

		public KeyValueStringList(Tuple<string, string>[] data)
		{
			Data = data.ToList();
		}

		public object DeserializeNew(XElement source)
		{
			var d = source.Elements("Value").Select(p => Tuple.Create(p.Attribute("Key").Value, p.Value)).ToArray();
			return new KeyValueStringList(d);
		}

		public object GetTypeStr()
		{
			return "KeyValueStringList";
		}

		public void Serialize(XElement target)
		{
			foreach (var d in Data)
			{
				var x = new XElement("Value");
				x.Add(new XAttribute("Key", d.Item1));
				x.Add(new XAttribute("type", "String"));
				x.Value = d.Item2;
				target.Add(x);
			}
		}
	}
}
