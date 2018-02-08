using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace AlephNote.Common.Themes
{
	[DebuggerDisplay("{Name} v{Version} ({SourceFilename})")]
	public sealed class AlephTheme
	{
		#region Definition

		public enum AlephThemePropType { Color, Brush, Thickness, Integer, Double, Boolean }

		public static readonly Tuple<string, AlephThemePropType>[] THEME_PROPERTIES =
		{
			Tuple.Create("window.background"                                     , AlephThemePropType.Brush     ),
			Tuple.Create("window.foreground"                                     , AlephThemePropType.Brush     ),
			Tuple.Create("window.splitter"                                       , AlephThemePropType.Brush     ),

			Tuple.Create("window.notetitle:foreground"                           , AlephThemePropType.Brush     ),
			Tuple.Create("window.changedate:foreground"                          , AlephThemePropType.Brush     ),

			Tuple.Create("window.menubar:background"                             , AlephThemePropType.Brush     ),
			Tuple.Create("window.menubar:foreground"                             , AlephThemePropType.Brush     ),

			Tuple.Create("window.tageditor.popup:background"                     , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.popup:bordercolor"                    , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.popup:foreground"                     , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.placeholder"                          , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.foreground"                           , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.tag:background"                       , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.tag:bordercolor_default"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.tag:bordercolor_highlighted"          , AlephThemePropType.Brush     ),
			Tuple.Create("window.tageditor.tag:foreground"                       , AlephThemePropType.Brush     ),

			Tuple.Create("window.pathdisplay:foregroud"                          , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.element:foregroud"                  , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.element:background"                 , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.element:background_hover"           , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.element:border"                     , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.button:foreground"                  , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.button:background"                  , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.button:border"                      , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup:background"                   , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup:border"                       , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup:border_thickness"             , AlephThemePropType.Thickness ),
			Tuple.Create("window.pathdisplay.popup.text:foreground"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.list:foreground"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.list:background"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.list.selected:foreground"     , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.list.selected:background"     , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.button:foreground"            , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.button:background"            , AlephThemePropType.Brush     ),
			Tuple.Create("window.pathdisplay.popup.button:border"                , AlephThemePropType.Brush     ),

			Tuple.Create("window.inlinesearch:background"                        , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch:foreground"                        , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.textbox:background"                , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.textbox:foreground"                , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnSearch:background"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnSearch:foreground"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnCaseIns:background"             , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnCaseIns:foreground"             , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnWholeWord:background"           , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnWholeWord:foreground"           , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnRegex:background"               , AlephThemePropType.Brush     ),
			Tuple.Create("window.inlinesearch.btnRegex:foreground"               , AlephThemePropType.Brush     ),

			Tuple.Create("window.globalsearch.input:foreground"                  , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.input:background"                  , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.input:border"                      , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.placeholder:foreground"            , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.button:foreground"                 , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.button:background"                 , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.button:border"                     , AlephThemePropType.Brush     ),
			Tuple.Create("window.globalsearch.button:border_thickness"           , AlephThemePropType.Thickness ),

			Tuple.Create("window.notesview.flat:background"                      , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat:foreground"                      , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat.selected:background"             , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat.selected:foreground"             , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat.datetime1:foreground"            , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat.datetime2:foreground"            , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat.preview:foreground"              , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.flat.separator"                       , AlephThemePropType.Brush     ),

			Tuple.Create("window.notesview.hierachical.list:background"          , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list:foreground"          , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list.selected:background" , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list.selected:foreground" , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list.datetime1:foreground", AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list.datetime2:foreground", AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list.preview:foreground"  , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.list.separator"           , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.splitter"                 , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.tree:background"          , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.tree:foreground"          , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.tree.selected:background" , AlephThemePropType.Brush     ),
			Tuple.Create("window.notesview.hierachical.tree.selected:foreground" , AlephThemePropType.Brush     ),

			Tuple.Create("window.statusbar:background"                           , AlephThemePropType.Brush     ),
			Tuple.Create("window.statusbar:foreground"                           , AlephThemePropType.Brush     ),
			Tuple.Create("window.statusbar.btnReload:foreground"                 , AlephThemePropType.Brush     ),
			Tuple.Create("window.statusbar.btnReload:background"                 , AlephThemePropType.Brush     ),
			Tuple.Create("window.statusbar.btnReload:border"                     , AlephThemePropType.Brush     ),

			//-------------------------------------------------------------------------------------

			Tuple.Create("scintilla.background"                                  , AlephThemePropType.Color     ),

			Tuple.Create("scintilla.whitespace:size"                             , AlephThemePropType.Integer   ),
			Tuple.Create("scintilla.whitespace:color"                            , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.whitespace:background"                       , AlephThemePropType.Color     ),

			Tuple.Create("scintilla.caret:foreground"                            , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.caret:background"                            , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.caret:background_alpha"                      , AlephThemePropType.Integer   ),
			Tuple.Create("scintilla.caret:visible"                               , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.default:foreground"                          , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.default:background"                          , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.default:underline"                           , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.default:bold"                                , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.default:italic"                              , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.link:foreground"                             , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.link:background"                             , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.link:underline"                              , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.link:bold"                                   , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.link:italic"                                 , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.default:foreground"                 , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.default:background"                 , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.default:underline"                  , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.default:bold"                       , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.default:italic"                     , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.emph:foreground"                    , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.emph:background"                    , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.emph:underline"                     , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.emph:bold"                          , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.emph:italic"                        , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.strong_emph:foreground"             , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.strong_emph:background"             , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.strong_emph:underline"              , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.strong_emph:bold"                   , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.strong_emph:italic"                 , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.header:foreground"                  , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.header:background"                  , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.header:underline"                   , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.header:bold"                        , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.header:italic"                      , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.code:foreground"                    , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.code:background"                    , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.code:underline"                     , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.code:bold"                          , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.code:italic"                        , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.list:foreground"                    , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.list:background"                    , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.list:underline"                     , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.list:bold"                          , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.list:italic"                        , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.markdown.url:foreground"                     , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.url:background"                     , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.markdown.url:underline"                      , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.url:bold"                           , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.markdown.url:italic"                         , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.margin.numbers:foreground"                   , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.margin.numbers:background"                   , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.margin.numbers:underline"                    , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.margin.numbers:bold"                         , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.margin.numbers:italic"                       , AlephThemePropType.Boolean   ),
			Tuple.Create("scintilla.margin.symbols:background"                   , AlephThemePropType.Color     ),

			Tuple.Create("scintilla.search.local:color"                          , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.search.local:alpha"                          , AlephThemePropType.Integer   ),
			Tuple.Create("scintilla.search.local:outline_alpha"                  , AlephThemePropType.Integer   ),
			Tuple.Create("scintilla.search.local:under_text"                     , AlephThemePropType.Boolean   ),

			Tuple.Create("scintilla.search.global:color"                         , AlephThemePropType.Color     ),
			Tuple.Create("scintilla.search.global:alpha"                         , AlephThemePropType.Integer   ),
			Tuple.Create("scintilla.search.global:outline_alpha"                 , AlephThemePropType.Integer   ),
			Tuple.Create("scintilla.search.global:under_text"                    , AlephThemePropType.Boolean   ),
		};

		#endregion

		public readonly bool IsFallback;

		public string Name { get; set; }
		public Version Version { get; set; }
		public CompatibilityVersionRange Compatibility { get; set; }
		public string SourceFilename { get; set; }
		public string Source { get; set; }

		private Dictionary<string, object> AllProperties = new Dictionary<string, object>();

		public AlephTheme(string n, Version v, CompatibilityVersionRange c, string fn, string src, bool fb)
		{
			Name = n;
			Version = v;
			Compatibility = c;
			SourceFilename = fn;
			Source = src;

			IsFallback = fb;
		}

		public void AddProperty(string name, object prop) => AllProperties.Add(name.ToLower(), prop);

		public T Get<T>(string name)
		{
			var obj = Get(name);
			if (obj is T result) return result;
			throw new Exception($"ThemeProperty has wrong type: {name} (Expected: {typeof(T)}, Actual: {obj?.GetType()})");
		}

		public object Get(string name)
		{
			if (AllProperties.TryGetValue(name.ToLower(), out var obj)) return obj;
			throw new Exception($"ThemeProperty not found: {name}");
		}

		public string GetStrRepr(string name)
		{
			if (!AllProperties.TryGetValue(name.ToLower(), out var obj)) return "N/A";

			if (obj == null) return "NULL";

			if (obj is double objDouble) return objDouble.ToString(CultureInfo.InvariantCulture);

			return obj.ToString();
		}
	}
}
