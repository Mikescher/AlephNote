using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MSHC.Serialization;

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
		private string _author;
		private AlephThemeType _themetype;

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

		public void Parse()
		{
			_name          = _xdoc.XListSingle("theme", "meta", "name");
			_version       = Version.Parse(_xdoc.XListSingle("theme", "meta", "version"));
			_compatibility = CompatibilityVersionRange.Parse(_xdoc.XListSingle("theme", "meta", "compatibility"));
			_author        = _xdoc.XListSingleOrDefault("theme", "meta", "author") ?? "Unknown";
			_themetype     = AlephThemeTypeHelper.Parse(_xdoc.XListSingleOrDefault("theme", "meta", "type") ?? "theme");

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

			_properties = new Dictionary<string, string>();
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
			var t = new AlephTheme(_name, _version, _compatibility, _filename, _source, _author, _themetype);

			foreach (var propdef in AlephTheme.THEME_PROPERTIES)
			{
				var v = TryGetProperty(propdef.Item1);
				if (v == null) continue;

				if (v.IsIndirect) { t.AddProperty(propdef.Item1, v); continue; }

				switch (propdef.Item2)
				{
					case AlephTheme.AlephThemePropType.Color:
						v.DirectValue = ColorRef.Parse(v.XmlDirectValue);
						break;

					case AlephTheme.AlephThemePropType.Brush:
						v.DirectValue = BrushRef.Parse(v.XmlDirectValue);
						break;

					case AlephTheme.AlephThemePropType.Thickness:
						v.DirectValue = ThicknessRef.Parse(v.XmlDirectValue);
						break;

					case AlephTheme.AlephThemePropType.Integer:
						v.DirectValue = int.Parse(v.XmlDirectValue);
						break;

					case AlephTheme.AlephThemePropType.Double:
						v.DirectValue = double.Parse(v.XmlDirectValue, NumberStyles.Float, CultureInfo.InvariantCulture);
						break;

					case AlephTheme.AlephThemePropType.Boolean:
						v.DirectValue = XElementExtensions.ParseBool(v.XmlDirectValue);
						break;

					case AlephTheme.AlephThemePropType.CornerRadius:
						v.DirectValue = CornerRadiusRef.Parse(v.XmlDirectValue);
						break;

					default:
						throw new NotSupportedException();
				}
				
				t.AddProperty(propdef.Item1, v);
			}
			
			return t;
		}

		public Dictionary<string, string> GetProperties() => _properties;

		public static AlephTheme GetFallback()
		{
			var t = new AlephTheme("DEFAULT_THEME_FALLBACK", new Version(0, 0, 0, 0), CompatibilityVersionRange.ANY, "NULL", "<!-- fallback -->", "auto generated", AlephThemeType.Fallback);

			var r = new Random();

			foreach (var propdef in AlephTheme.THEME_PROPERTIES)
			{
				switch (propdef.Item2)
				{
					case AlephTheme.AlephThemePropType.Color:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=ColorRef.GetRandom(r),XmlDirectValue="random",IndirectionTarget=null});
						break;

					case AlephTheme.AlephThemePropType.Brush:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=BrushRef.CreateSolid(ColorRef.GetRandom(r)),XmlDirectValue="random",IndirectionTarget=null});
						break;

					case AlephTheme.AlephThemePropType.Thickness:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=ThicknessRef.Create(r.Next(16), r.Next(16), r.Next(16), r.Next(16)),XmlDirectValue="random",IndirectionTarget=null});
						break;

					case AlephTheme.AlephThemePropType.Integer:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=r.Next(16),XmlDirectValue="random",IndirectionTarget=null});
						break;

					case AlephTheme.AlephThemePropType.Double:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=r.NextDouble()*16,XmlDirectValue="random",IndirectionTarget=null});
						break;

					case AlephTheme.AlephThemePropType.Boolean:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=(r.Next()%2 == 0),XmlDirectValue="random",IndirectionTarget=null});
						break;

					case AlephTheme.AlephThemePropType.CornerRadius:
						t.AddProperty(propdef.Item1, new AlephThemePropertyValue{IsIndirect=false,DirectValue=CornerRadiusRef.Create(r.Next(6)),XmlDirectValue="random",IndirectionTarget=null});
						break;

					default:
						throw new NotSupportedException();
				}
			}

			return t;
		}
		
		private AlephThemePropertyValue TryGetProperty(string name)
		{
			if (_properties.TryGetValue(name.ToLower(), out var v))
			{
				if (v.StartsWith("$"))
				{
					return new AlephThemePropertyValue
					{
						IsIndirect        = true,
						IndirectionTarget = v.Substring(1),
						DirectValue       = null,
						XmlDirectValue    = v,
					};
				}
				else
				{
					return new AlephThemePropertyValue
					{
						IsIndirect        = false,
						IndirectionTarget = null,
						DirectValue       = null,
						XmlDirectValue    = v,
					};
				}
			}

			return null;
		}
	}
}
