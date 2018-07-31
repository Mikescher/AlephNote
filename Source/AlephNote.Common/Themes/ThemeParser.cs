using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Themes
{
	/// <summary>
	/// 
	///     Valid values:
	///     
	///     Boolean   : ['1', '0', 'true', 'false']
	///     Integer   : /[0-9]+/
	///     Double    : /[0-9]+(\.[0-9]+)?/
	///     Color     : ['#RGB', '#RRGGBB', '#ARGB', '#AARRGGBB', 'transparent']
	///     Brush     : Color
	///                 solid://Color
	///                 gradient://(Double:Color  |  )+
	///     
	///     Indirect  : $propname   // lazy resolved
	///     Const refs: @refname    // early resolved
	/// 
	/// </summary>
	public class ThemeParser
	{
		private class ValueRef
		{
			public readonly string Key;
			public readonly string Type;
			public readonly string Value;
			public ValueRef(string k, string t, string v) { Key = k; Type = t; Value = v; }
		}

		private XDocument _xdoc;

		private string _name;
		private Version _version;
		private CompatibilityVersionRange _compatibility;
		private string _filename;
		private string _source;

		private Dictionary<string, ValueRef> _references;
		private Dictionary<string, string> _properties;

		public void LoadFromString(string content, string filename)
		{
			_xdoc = XDocument.Parse(_source = content);
			_filename = filename.ToLower();
		}

		public void Load(string filepath)
		{
			_source = File.ReadAllText(filepath);
			_xdoc = XDocument.Parse(_source);
			_filename = Path.GetFileName(filepath).ToLower();
		}

		public void Parse(Dictionary<string, string> baseValues)
		{
			_name          = _xdoc.XListSingle("theme", "meta", "name");
			_version       = Version.Parse(_xdoc.XListSingle("theme", "meta", "version"));
			_compatibility = CompatibilityVersionRange.Parse(_xdoc.XListSingle("theme", "meta", "compatibility"));

			_references = new Dictionary<string, ValueRef>();
			foreach (var vr in _xdoc.XElemList("theme", "ref", "valueref@key=~&type=~"))
			{
				var key = vr.Attribute("key").Value;
				var typ = vr.Attribute("type").Value;
				var value = vr.Value;

				if (value.StartsWith("@"))
				{
					value = value.Substring(1);

					if (_references.TryGetValue(value.ToLower(), out var vref))
						value = vref.Value;
					else
						throw new Exception($"Reference not found: '{value}'");
				}

				_references.Add(key.ToLower(), new ValueRef(key, typ, value));
			}

			_properties = new Dictionary<string, string>(baseValues);
			foreach (var prop in _xdoc.XElemList("theme", "data", "*group*", "property@name=~&value=~"))
			{
				var name  = prop.Attribute("name").Value.ToLower();
				var value = prop.Attribute("value").Value;

				if (value.StartsWith("@"))
				{
					value = value.Substring(1);

					if (_references.TryGetValue(value.ToLower(), out var vref))
						value = vref.Value;
					else
						throw new Exception($"Reference not found: '{value}'");
				}

				_properties[name] = value;
			}
		}

		public AlephTheme Generate()
		{
			var t = new AlephTheme(_name, _version, _compatibility, _filename, _source, false);

			foreach (var propdef in AlephTheme.THEME_PROPERTIES)
			{
				switch (propdef.Item2)
				{
					case AlephTheme.AlephThemePropType.Color:
						t.AddProperty(propdef.Item1, ColorRef.Parse(GetProperty(propdef.Item1)));
						break;

					case AlephTheme.AlephThemePropType.Brush:
						t.AddProperty(propdef.Item1, BrushRef.Parse(GetProperty(propdef.Item1)));
						break;

					case AlephTheme.AlephThemePropType.Thickness:
						t.AddProperty(propdef.Item1, ThicknessRef.Parse(GetProperty(propdef.Item1)));
						break;

					case AlephTheme.AlephThemePropType.Integer:
						t.AddProperty(propdef.Item1, int.Parse(GetProperty(propdef.Item1)));
						break;

					case AlephTheme.AlephThemePropType.Double:
						t.AddProperty(propdef.Item1, double.Parse(GetProperty(propdef.Item1), NumberStyles.Float, CultureInfo.InvariantCulture));
						break;

					case AlephTheme.AlephThemePropType.Boolean:
						t.AddProperty(propdef.Item1, XElementExtensions.ParseBool(GetProperty(propdef.Item1)));
						break;

					case AlephTheme.AlephThemePropType.CornerRadius:
						t.AddProperty(propdef.Item1, CornerRadiusRef.Parse(GetProperty(propdef.Item1)));
						break;

					default:
						throw new NotSupportedException();
				}
			}
			
			return t;
		}

		public Dictionary<string, string> GetProperties() => _properties;

		public static AlephTheme GetFallback()
		{
			var t = new AlephTheme("DEFAULT_THEME_FALLBACK", new Version(0, 0, 0, 0), CompatibilityVersionRange.ANY, "NULL", "<!-- fallback -->", true);

			var r = new Random();

			foreach (var propdef in AlephTheme.THEME_PROPERTIES)
			{
				switch (propdef.Item2)
				{
					case AlephTheme.AlephThemePropType.Color:
						t.AddProperty(propdef.Item1, ColorRef.GetRandom(r));
						break;

					case AlephTheme.AlephThemePropType.Brush:
						t.AddProperty(propdef.Item1, BrushRef.CreateSolid(ColorRef.GetRandom(r)));
						break;

					case AlephTheme.AlephThemePropType.Thickness:
						t.AddProperty(propdef.Item1, ThicknessRef.Create(r.Next(16), r.Next(16), r.Next(16), r.Next(16)));
						break;

					case AlephTheme.AlephThemePropType.Integer:
						t.AddProperty(propdef.Item1, r.Next(16));
						break;

					case AlephTheme.AlephThemePropType.Double:
						t.AddProperty(propdef.Item1, r.NextDouble()*16);
						break;

					case AlephTheme.AlephThemePropType.Boolean:
						t.AddProperty(propdef.Item1, r.Next()%2 == 0);
						break;

					case AlephTheme.AlephThemePropType.CornerRadius:
						t.AddProperty(propdef.Item1, CornerRadiusRef.Create(r.Next(6)));
						break;

					default:
						throw new NotSupportedException();
				}
			}

			return t;
		}
		
		private string GetProperty(string name, int depth=0, string origName=null)
		{
			if (depth >= 4) throw new Exception($"Max recursion depth reached for property '{origName}'");

			if (_properties.TryGetValue(name.ToLower(), out var v))
			{
				if (v.StartsWith("$")) return GetProperty(v.Substring(1), depth + 1, origName ?? name);

				return v;
			}

			throw new Exception($"Value for property '{name}' not found");
		}

	}
}
