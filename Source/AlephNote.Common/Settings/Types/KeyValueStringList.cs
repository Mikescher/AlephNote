using AlephNote.Common.AlephXMLSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AlephNote.Common.MVVM;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;

namespace AlephNote.Common.Settings.Types
{
	public class KeyValueStringList : ObservableObject, IAlephCustomSerializableField
	{
		public readonly IReadOnlyDictionary<string, string> Data;

		public KeyValueStringList(Tuple<string, string>[] data)
		{
			Data = data.ToDictionary(d => d.Item1, d => d.Item2);
		}

		public object DeserializeNew(XElement source, AXMLSerializationSettings opt)
		{
			var d = source.Elements("Value").Select(p => Tuple.Create(p.Attribute("Key").Value, p.Value)).ToArray();
			return new KeyValueStringList(d);
		}

		public object GetTypeStr()
		{
			return "KeyValueStringList";
		}

		public void Serialize(XElement target, AXMLSerializationSettings opt)
		{
			foreach (var d in Data)
			{
				var x = new XElement("Value");
				x.Add(new XAttribute("Key", d.Key));
				if ((opt & AXMLSerializationSettings.IncludeTypeInfo) != 0) x.Add(new XAttribute("type", "String"));
				x.Value = d.Value;
				target.Add(x);
			}
		}
	}
}
