using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Common.Themes
{
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

		private Dictionary<string, ValueRef> _references;
		private Dictionary<string, string> _properties;
		private Dictionary<string, string> _defaults;

		public void Load(string filepath)
		{
			_xdoc = XDocument.Load(filepath);
		}

		public void Parse()
		{
			_name          = _xdoc.XListSingle("theme", "meta", "name");
			_version       = Version.Parse(_xdoc.XListSingle("theme", "meta", "version"));
			_compatibility = CompatibilityVersionRange.Parse(_xdoc.XListSingle("theme", "meta", "compatibility"));

			_references = new Dictionary<string, ValueRef>();
			foreach (var vr in _xdoc.XElemList("theme", "ref", "valueref@key=~&type=~"))
			{
				var key = vr.Attribute("key").Value;
				var typ = vr.Attribute("type").Value;
				var val = vr.Value;
				_references.Add(key.ToLower(), new ValueRef(key, typ, val));
			}

			_properties = new Dictionary<string, string>();
			foreach (var prop in _xdoc.XElemList("theme", "data", "*group*", "property@name=~&value=~"))
			{
				var name = prop.Attribute("name").Value.ToLower();
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

		public AlephTheme Generate(Dictionary<string, string> defaults)
		{
			_defaults = defaults;

			return new AlephTheme
			{
				Name = _name,
				Version = _version,
				Compatibility = _compatibility,

				Scintilla_Background         = GetPropertyColorRef("scintilla.background"),
				Scintilla_WhitespaceSize     = GetPropertyInteger("scintilla.whitespacesize"),
				Scintilla_WhitespaceColor    = GetPropertyColorRef("scintilla.whitespacecolor"),

				Scintilla_Default            = GetPropertyStyleSpec("scintilla.default"),
				Scintilla_Link               = GetPropertyStyleSpec("scintilla.link"),
				Scintilla_MarkdownDefault    = GetPropertyStyleSpec("scintilla.markdown.default"),
				Scintilla_MarkdownEmph       = GetPropertyStyleSpec("scintilla.markdown.emph"),
				Scintilla_MarkdownStrongEmph = GetPropertyStyleSpec("scintilla.markdown.strong_emph"),
				Scintilla_MarkdownHeader     = GetPropertyStyleSpec("scintilla.markdown.header"),
				Scintilla_MarkdownCode       = GetPropertyStyleSpec("scintilla.markdown.code"),
				Scintilla_MarkdownUrl        = GetPropertyStyleSpec("scintilla.markdown.url"),
				Scintilla_MarkdownList       = GetPropertyStyleSpec("scintilla.markdown.list"),

				Scintilla_Search_Local       = GetPropertyIndicatorSpec("scintilla.search.local"),
				Scintilla_Search_Global      = GetPropertyIndicatorSpec("scintilla.search.global"),
			};
		}

		public Dictionary<string, string> GetProperties() => _properties;

		public static AlephTheme GetDefault()
		{
			return new AlephTheme
			{
				Name = "DEFAULT_THEME_FALLBACK",
				Version = new Version(0, 0, 0, 0),
				Compatibility = CompatibilityVersionRange.Parse("*"),
			};
		}

		private ColorRef GetPropertyColorRef(string name)
		{
			var prop = GetProperty(name, null);
			if (prop == null)
			{
				if (_defaults.TryGetValue(name.ToLower(), out var v))
					prop = v;
				else
					throw new Exception($"Defaultvalue for property not found '{name}'");
			}

			return ColorRef.Parse(prop);
		}

		private int GetPropertyInteger(string name)
		{
			var prop = GetProperty(name, null);
			if (prop == null)
			{
				if (_defaults.TryGetValue(name.ToLower(), out var v))
					prop = v;
				else
					throw new Exception($"Defaultvalue for property not found '{name}'");
			}

			return int.Parse(prop);
		}

		private bool GetPropertyBoolean(string name)
		{
			var prop = GetProperty(name, null);
			if (prop == null)
			{
				if (_defaults.TryGetValue(name.ToLower(), out var v))
					prop = v;
				else
					throw new Exception($"Defaultvalue for property not found '{name}'");
			}

			return XElementExtensions.ParseBool(prop);
		}

		private ScintillaStyleSpec GetPropertyStyleSpec(string name)
		{
			var specForeground = GetPropertyColorRef(name + ":foreground");
			var specBackground = GetPropertyColorRef(name + ":background");
			var specUnderline = GetPropertyBoolean(name + ":underline");
			var specBold = GetPropertyBoolean(name + ":bold");
			var specItalic = GetPropertyBoolean(name + ":italic");

			return new ScintillaStyleSpec
			{
				Foreground = specForeground,
				Background = specBackground,
				Underline = specUnderline,
				Bold = specBold,
				Italic = specItalic,
			};
		}

		private IndicatorSpec GetPropertyIndicatorSpec(string name)
		{
			var specColor        = GetPropertyColorRef(name + ":color");
			var specAlpha        = GetPropertyInteger( name + ":alpha");
			var specOutlineAlpha = GetPropertyInteger( name + ":outline_alpha");
			var specUnderText    = GetPropertyBoolean( name + ":under_text");

			return new IndicatorSpec
			{
				Color        = specColor,
				Alpha        = specAlpha,
				OutlineAlpha = specOutlineAlpha,
				UnderText    = specUnderText,
			};
		}

		private string GetProperty(string name, string defaultValue)
		{
			if (_properties.TryGetValue(name.ToLower(), out var v)) return v;
			return defaultValue;

		}

	}
}
