using AlephNote.Common.AlephXMLSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Settings.Types
{
	public class KeyValueCustomList<TValue> : ObservableObject, IAlephCustomSerializableField where TValue : IAlephCustomSerializableField
	{
		public readonly TValue DefaultDictValue;

		public readonly IReadOnlyDictionary<string, TValue> Data;

		public KeyValueCustomList(IEnumerable<Tuple<string, TValue>> data, TValue ddv)
		{
			Data = data.ToDictionary(d => d.Item1, d => d.Item2);
			DefaultDictValue = ddv;
		}

		public object DeserializeNew(XElement source)
		{
			var d = source.Elements("Value").Select(p => Tuple.Create(p.Attribute("Key").Value, (TValue)DefaultDictValue.DeserializeNew(p.Element("GenericValue")))).ToArray();
			return new KeyValueCustomList<TValue>(d, DefaultDictValue);
		}

		public object GetTypeStr()
		{
			return $"KeyValueCustomList<{DefaultDictValue.GetTypeStr()}>";
		}

		public void Serialize(XElement target)
		{
			foreach (var d in Data)
			{
				var x = new XElement("Value");
				x.Add(new XAttribute("Key", d.Key));
				x.Add(new XAttribute("type", "GenericValue"));

				var s = new XElement("GenericValue");
				d.Value.Serialize(s);
				x.Add(s);

				target.Add(x);
			}
		}

		public bool TryGetValue(string key, out TValue value)
		{
			return Data.TryGetValue(key, out value);
		}

		public bool Contains(string key)
		{
			return Data.ContainsKey(key);
		}
	}
}
