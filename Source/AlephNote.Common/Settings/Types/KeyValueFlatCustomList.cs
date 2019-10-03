using AlephNote.Common.AlephXMLSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using AlephNote.PluginInterface.Objects;
using AlephNote.PluginInterface.Objects.AXML;
using MSHC.WPF.MVVM;

namespace AlephNote.Common.Settings.Types
{
	public class KeyValueFlatCustomList<TValue> : ObservableObject, IAlephCustomSerializableField, IEnumerable<KeyValuePair<string, TValue>> where TValue : IAlephCustomFlatSerializableField
	{
		public readonly TValue DefaultDictValue;

		private readonly Dictionary<string, TValue> _data;

		public IReadOnlyDictionary<string, TValue> ListData => _data;

		public KeyValueFlatCustomList(IEnumerable<(string, TValue)> data, TValue ddv)
		{
			_data = data.ToDictionary(d => d.Item1, d => d.Item2);
			DefaultDictValue = ddv;
		}

		public object DeserializeNew(XElement source, AXMLSerializationSettings opt)
		{
			var d = source.Elements("Value").Select(p => (p.Attribute("Key").Value, (TValue)DefaultDictValue.DeserializeNew(p.Value, opt))).ToArray();
			return new KeyValueFlatCustomList<TValue>(d, DefaultDictValue);
		}

		public object GetTypeStr()
		{
			return $"KeyValueCustomList[{DefaultDictValue.GetTypeStr()}]";
		}

		public void Serialize(XElement target, AXMLSerializationSettings opt)
		{
			foreach (var d in _data)
			{
				var x = new XElement("Value");
				x.Add(new XAttribute("Key", d.Key));
				if ((opt & AXMLSerializationSettings.IncludeTypeInfo) != 0) x.Add(new XAttribute("type", d.Value.GetTypeStr()));
				x.Value = d.Value.Serialize(opt);
				target.Add(x);
			}
		}

		public bool TryGetValue(string key, out TValue value)
		{
			return _data.TryGetValue(key, out value);
		}

		public bool Contains(string key)
		{
			return _data.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
		{
			return ListData.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ListData.GetEnumerator();
		}

		public KeyValueFlatCustomList<TValue> Concat((string, TValue) v)
		{
			return new KeyValueFlatCustomList<TValue>(
				ListData
					.AsEnumerable()
					.Select(ld => (ld.Key, ld.Value))
					.Concat(new []{v})
					.GroupBy(p => p.Item1)
					.Select(p => p.First()), 
				DefaultDictValue);
		}
	}
}
