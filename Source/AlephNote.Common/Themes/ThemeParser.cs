using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
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

		private Dictionary<string, ValueRef> _references;
		private Dictionary<string, string> _properties;

		public void Load(string filepath)
		{
			_xdoc = XDocument.Load(filepath);
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
				var val = vr.Value;
				_references.Add(key.ToLower(), new ValueRef(key, typ, val));
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
			return new AlephTheme(_name, _version, _compatibility, _filename)
			{
				Window_Background            = GetPropertyColorRef("window.background"),

				Window_MainMenu_Foreground   = GetPropertyColorRef("window.mainmenu:foreground"),
				Window_MainMenu_Background   = GetPropertyBrushRef("window.mainmenu:background"),

				Window_NoteTitle_Foreground  = GetPropertyColorRef("window.notetitle:foreground"),
				Window_ChangeDate_Foreground = GetPropertyColorRef("window.changedate:foreground"),


				Scintilla_Background             = GetPropertyColorRef("scintilla.background"),

				Scintilla_WhitespaceSize         = GetPropertyInteger( "scintilla.whitespace:size"),
				Scintilla_WhitespaceColor        = GetPropertyColorRef("scintilla.whitespace:color"),
				Scintilla_WhitespaceBackground   = GetPropertyColorRef("scintilla.whitespace:background"),

				Scintilla_MarginLineNumbersColor = GetPropertyColorRef("scintilla.margin.linenumbers:background"),
				Scintilla_MarginListSymbolsColor = GetPropertyColorRef("scintilla.margin.listsymbols:background"),

				Scintilla_CaretForeground        = GetPropertyColorRef("scintilla.caret:foreground"),
				Scintilla_CaretBackground        = GetPropertyColorRef("scintilla.caret:background"),
				Scintilla_CaretBackgroundAlpha   = GetPropertyInteger( "scintilla.caret:background_alpha"),
				Scintilla_CaretVisible           = GetPropertyBoolean( "scintilla.caret:visible"),

				Scintilla_Default                = GetPropertyStyleSpec("scintilla.default"),
				Scintilla_Link                   = GetPropertyStyleSpec("scintilla.link"),
				Scintilla_MarkdownDefault        = GetPropertyStyleSpec("scintilla.markdown.default"),
				Scintilla_MarkdownEmph           = GetPropertyStyleSpec("scintilla.markdown.emph"),
				Scintilla_MarkdownStrongEmph     = GetPropertyStyleSpec("scintilla.markdown.strong_emph"),
				Scintilla_MarkdownHeader         = GetPropertyStyleSpec("scintilla.markdown.header"),
				Scintilla_MarkdownCode           = GetPropertyStyleSpec("scintilla.markdown.code"),
				Scintilla_MarkdownList           = GetPropertyStyleSpec("scintilla.markdown.list"),
				Scintilla_MarkdownURL            = GetPropertyStyleSpec("scintilla.markdown.url"),

				Scintilla_Search_Local           = GetPropertyIndicatorSpec("scintilla.search.local"),
				Scintilla_Search_Global          = GetPropertyIndicatorSpec("scintilla.search.global"),
			};
		}

		public Dictionary<string, string> GetProperties() => _properties;

		public static AlephTheme GetDefault()
		{
			return new AlephTheme("DEFAULT_THEME_FALLBACK", new Version(0, 0, 0, 0), CompatibilityVersionRange.Parse("*"), "NULL");
		}

		private ColorRef GetPropertyColorRef(string name)
		{
			return ColorRef.Parse(GetProperty(name));
		}

		private BrushRef GetPropertyBrushRef(string name)
		{
			return BrushRef.Parse(GetProperty(name));
		}

		private int GetPropertyInteger(string name)
		{
			return int.Parse(GetProperty(name));
		}

		private bool GetPropertyBoolean(string name)
		{
			return XElementExtensions.ParseBool(GetProperty(name));
		}

		private ScintillaStyleSpec GetPropertyStyleSpec(string name)
		{
			var specForeground = GetPropertyColorRef(name + ":foreground");
			var specBackground = GetPropertyColorRef(name + ":background");
			var specUnderline  = GetPropertyBoolean( name + ":underline");
			var specBold       = GetPropertyBoolean( name + ":bold");
			var specItalic     = GetPropertyBoolean( name + ":italic");

			return new ScintillaStyleSpec
			{
				Foreground = specForeground,
				Background = specBackground,
				Underline  = specUnderline,
				Bold       = specBold,
				Italic     = specItalic,
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
