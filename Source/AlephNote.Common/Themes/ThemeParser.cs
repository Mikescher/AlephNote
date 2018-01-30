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
			var t = new AlephTheme(_name, _version, _compatibility, _filename, false);

			CollectPropertyBrushRef(t, "window.background");
			CollectPropertyBrushRef(t, "window.foreground");
			CollectPropertyBrushRef(t, "window.splitter");

			CollectPropertyBrushRef(t, "window.notetitle:foreground");
			CollectPropertyBrushRef(t, "window.changedate:foreground");

			CollectPropertyBrushRef(t, "window.tageditor.popup:background");
			CollectPropertyBrushRef(t, "window.tageditor.popup:bordercolor");
			CollectPropertyBrushRef(t, "window.tageditor.popup:foreground");
			CollectPropertyBrushRef(t, "window.tageditor.placeholder");
			CollectPropertyBrushRef(t, "window.tageditor.foreground");
			CollectPropertyBrushRef(t, "window.tageditor.tag:background");
			CollectPropertyBrushRef(t, "window.tageditor.tag:bordercolor_default");
			CollectPropertyBrushRef(t, "window.tageditor.tag:bordercolor_highlighted");
			CollectPropertyBrushRef(t, "window.tageditor.tag:foreground");

			CollectPropertyBrushRef(t, "window.pathdisplay:foregroud");
			CollectPropertyBrushRef(t, "window.pathdisplay.element:foregroud");
			CollectPropertyBrushRef(t, "window.pathdisplay.element:background");
			CollectPropertyBrushRef(t, "window.pathdisplay.element:background_hover");
			CollectPropertyBrushRef(t, "window.pathdisplay.element:border");
			CollectPropertyBrushRef(t, "window.pathdisplay.button:foreground");
			CollectPropertyBrushRef(t, "window.pathdisplay.button:background");
			CollectPropertyBrushRef(t, "window.pathdisplay.button:border");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup:background");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup.text:foreground");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup.list:foreground");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup.list:background");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup.button:foreground");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup.button:background");
			CollectPropertyBrushRef(t, "window.pathdisplay.popup.button:border");

			CollectPropertyBrushRef(t, "window.inlinesearch:background");
			CollectPropertyBrushRef(t, "window.inlinesearch:foreground");
			CollectPropertyBrushRef(t, "window.inlinesearch.textbox:background");
			CollectPropertyBrushRef(t, "window.inlinesearch.textbox:foreground");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnSearch:background");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnSearch:foreground");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnCaseIns:background");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnCaseIns:foreground");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnWholeWord:background");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnWholeWord:foreground");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnRegex:background");
			CollectPropertyBrushRef(t, "window.inlinesearch.btnRegex:foreground");

			CollectPropertyBrushRef(t, "window.globalsearch.input:foreground");
			CollectPropertyBrushRef(t, "window.globalsearch.input:background");
			CollectPropertyBrushRef(t, "window.globalsearch.input:border");
			CollectPropertyBrushRef(t, "window.globalsearch.placeholder:foreground");
			CollectPropertyBrushRef(t, "window.globalsearch.button:foreground");
			CollectPropertyBrushRef(t, "window.globalsearch.button:background");
			CollectPropertyBrushRef(t, "window.globalsearch.button:border");

			CollectPropertyBrushRef(t, "window.notesview.flat:background");
			CollectPropertyBrushRef(t, "window.notesview.flat:foreground");
			CollectPropertyBrushRef(t, "window.notesview.flat.selected:background");
			CollectPropertyBrushRef(t, "window.notesview.flat.datetime1:foreground");
			CollectPropertyBrushRef(t, "window.notesview.flat.datetime2:foreground");
			CollectPropertyBrushRef(t, "window.notesview.flat.preview:foreground");
			CollectPropertyBrushRef(t, "window.notesview.flat.separator");

			//-------------------------------------------------------------------------------------

			CollectPropertyColorRef(t, "scintilla.background");

			CollectPropertyInteger( t, "scintilla.whitespace:size");
			CollectPropertyColorRef(t, "scintilla.whitespace:color");
			CollectPropertyColorRef(t, "scintilla.whitespace:background");

			CollectPropertyColorRef(t, "scintilla.margin.linenumbers:background");
			CollectPropertyColorRef(t, "scintilla.margin.listsymbols:background");

			CollectPropertyColorRef(t, "scintilla.caret:foreground");
			CollectPropertyColorRef(t, "scintilla.caret:background");
			CollectPropertyInteger( t, "scintilla.caret:background_alpha");
			CollectPropertyBoolean( t, "scintilla.caret:visible");

			CollectPropertyStyleSpec(t, "scintilla.default");
			CollectPropertyStyleSpec(t, "scintilla.link");
			CollectPropertyStyleSpec(t, "scintilla.markdown.default");
			CollectPropertyStyleSpec(t, "scintilla.markdown.emph");
			CollectPropertyStyleSpec(t, "scintilla.markdown.strong_emph");
			CollectPropertyStyleSpec(t, "scintilla.markdown.header");
			CollectPropertyStyleSpec(t, "scintilla.markdown.code");
			CollectPropertyStyleSpec(t, "scintilla.markdown.list");
			CollectPropertyStyleSpec(t, "scintilla.markdown.url");

			CollectPropertyIndicatorSpec(t, "scintilla.search.local");
			CollectPropertyIndicatorSpec(t, "scintilla.search.global");

			return t;
		}

		public Dictionary<string, string> GetProperties() => _properties;

		public static AlephTheme GetDefault()
		{
			return new AlephTheme("DEFAULT_THEME_FALLBACK", new Version(0, 0, 0, 0), CompatibilityVersionRange.ANY, "NULL", true);
		}

		private void CollectPropertyColorRef(AlephTheme t, string name)
		{
			t.AddProperty(name, ColorRef.Parse(GetProperty(name)));
		}

		private void CollectPropertyBrushRef(AlephTheme t, string name)
		{
			t.AddProperty(name, BrushRef.Parse(GetProperty(name)));
		}

		private void CollectPropertyInteger(AlephTheme t, string name)
		{
			t.AddProperty(name, int.Parse(GetProperty(name)));
		}

		private void CollectPropertyBoolean(AlephTheme t, string name)
		{
			t.AddProperty(name, XElementExtensions.ParseBool(GetProperty(name)));
		}

		private void CollectPropertyStyleSpec(AlephTheme t, string name)
		{
			CollectPropertyColorRef(t, name + ":foreground");
			CollectPropertyColorRef(t, name + ":background");
			CollectPropertyBoolean( t, name + ":underline");
			CollectPropertyBoolean( t, name + ":bold");
			CollectPropertyBoolean( t, name + ":italic");
		}

		private void CollectPropertyIndicatorSpec(AlephTheme t, string name)
		{
			CollectPropertyColorRef(t, name + ":color");
			CollectPropertyInteger( t, name + ":alpha");
			CollectPropertyInteger( t, name + ":outline_alpha");
			CollectPropertyBoolean( t, name + ":under_text");
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
