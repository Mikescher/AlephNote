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

		public static readonly Dictionary<string, AlephThemePropType> THEME_PROPERTIES = new Dictionary<string, AlephThemePropType>
		{
			{"window.background"                                     , AlephThemePropType.Brush     },
			{"window.foreground"                                     , AlephThemePropType.Brush     },
			{"window.splitter"                                       , AlephThemePropType.Brush     },

			{"window.notetitle:foreground"                           , AlephThemePropType.Brush     },
			{"window.changedate:foreground"                          , AlephThemePropType.Brush     },

			{"window.menubar:background"                             , AlephThemePropType.Brush     },
			{"window.menubar:foreground"                             , AlephThemePropType.Brush     },

			{"window.tageditor.popup:background"                     , AlephThemePropType.Brush     },
			{"window.tageditor.popup:bordercolor"                    , AlephThemePropType.Brush     },
			{"window.tageditor.popup:foreground"                     , AlephThemePropType.Brush     },
			{"window.tageditor.placeholder"                          , AlephThemePropType.Brush     },
			{"window.tageditor.foreground"                           , AlephThemePropType.Brush     },
			{"window.tageditor.tag:background"                       , AlephThemePropType.Brush     },
			{"window.tageditor.tag:bordercolor_default"              , AlephThemePropType.Brush     },
			{"window.tageditor.tag:bordercolor_highlighted"          , AlephThemePropType.Brush     },
			{"window.tageditor.tag:foreground"                       , AlephThemePropType.Brush     },

			{"window.pathdisplay:foregroud"                          , AlephThemePropType.Brush     },
			{"window.pathdisplay.element:foregroud"                  , AlephThemePropType.Brush     },
			{"window.pathdisplay.element:background"                 , AlephThemePropType.Brush     },
			{"window.pathdisplay.element:background_hover"           , AlephThemePropType.Brush     },
			{"window.pathdisplay.element:border"                     , AlephThemePropType.Brush     },
			{"window.pathdisplay.button:foreground"                  , AlephThemePropType.Brush     },
			{"window.pathdisplay.button:background"                  , AlephThemePropType.Brush     },
			{"window.pathdisplay.button:border"                      , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup:background"                   , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup:border"                       , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup:border_thickness"             , AlephThemePropType.Thickness },
			{"window.pathdisplay.popup.text:foreground"              , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.list:foreground"              , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.list:background"              , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.list.selected:foreground"     , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.list.selected:background"     , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.button:foreground"            , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.button:background"            , AlephThemePropType.Brush     },
			{"window.pathdisplay.popup.button:border"                , AlephThemePropType.Brush     },

			{"window.inlinesearch:background"                        , AlephThemePropType.Brush     },
			{"window.inlinesearch:foreground"                        , AlephThemePropType.Brush     },
			{"window.inlinesearch.textbox:background"                , AlephThemePropType.Brush     },
			{"window.inlinesearch.textbox:foreground"                , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnSearch:background"              , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnSearch:foreground"              , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnCaseIns:background"             , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnCaseIns:foreground"             , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnWholeWord:background"           , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnWholeWord:foreground"           , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnRegex:background"               , AlephThemePropType.Brush     },
			{"window.inlinesearch.btnRegex:foreground"               , AlephThemePropType.Brush     },

			{"window.globalsearch.input:foreground"                  , AlephThemePropType.Brush     },
			{"window.globalsearch.input:background"                  , AlephThemePropType.Brush     },
			{"window.globalsearch.input:border"                      , AlephThemePropType.Brush     },
			{"window.globalsearch.placeholder:foreground"            , AlephThemePropType.Brush     },
			{"window.globalsearch.button:foreground"                 , AlephThemePropType.Brush     },
			{"window.globalsearch.button:background"                 , AlephThemePropType.Brush     },
			{"window.globalsearch.button:border"                     , AlephThemePropType.Brush     },

			{"window.notesview.flat:background"                      , AlephThemePropType.Brush     },
			{"window.notesview.flat:foreground"                      , AlephThemePropType.Brush     },
			{"window.notesview.flat.selected:background"             , AlephThemePropType.Brush     },
			{"window.notesview.flat.selected:foreground"             , AlephThemePropType.Brush     },
			{"window.notesview.flat.datetime1:foreground"            , AlephThemePropType.Brush     },
			{"window.notesview.flat.datetime2:foreground"            , AlephThemePropType.Brush     },
			{"window.notesview.flat.preview:foreground"              , AlephThemePropType.Brush     },
			{"window.notesview.flat.separator"                       , AlephThemePropType.Brush     },

			{"window.notesview.hierachical.list:background"          , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list:foreground"          , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list.selected:background" , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list.selected:foreground" , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list.datetime1:foreground", AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list.datetime2:foreground", AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list.preview:foreground"  , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.list.separator"           , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.splitter"                 , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.tree:background"          , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.tree:foreground"          , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.tree.selected:background" , AlephThemePropType.Brush     },
			{"window.notesview.hierachical.tree.selected:foreground" , AlephThemePropType.Brush     },

			{"window.statusbar:background"                           , AlephThemePropType.Brush     },
			{"window.statusbar:foreground"                           , AlephThemePropType.Brush     },
			{"window.statusbar.btnReload:foreground"                 , AlephThemePropType.Brush     },
			{"window.statusbar.btnReload:background"                 , AlephThemePropType.Brush     },
			{"window.statusbar.btnReload:border"                     , AlephThemePropType.Brush     },

//-------------------------------------------------------------------------------------

			{"scintilla.background"                                  , AlephThemePropType.Color     },

			{"scintilla.whitespace:size"                             , AlephThemePropType.Integer   },
			{"scintilla.whitespace:color"                            , AlephThemePropType.Color     },
			{"scintilla.whitespace:background"                       , AlephThemePropType.Color     },

			{"scintilla.margin.linenumbers:background"               , AlephThemePropType.Color     },
			{"scintilla.margin.listsymbols:background"               , AlephThemePropType.Color     },

			{"scintilla.caret:foreground"                            , AlephThemePropType.Color     },
			{"scintilla.caret:background"                            , AlephThemePropType.Color     },
			{"scintilla.caret:background_alpha"                      , AlephThemePropType.Integer   },
			{"scintilla.caret:visible"                               , AlephThemePropType.Boolean   },

			{"scintilla.default:foreground"                          , AlephThemePropType.Color     },
			{"scintilla.default:background"                          , AlephThemePropType.Color     },
			{"scintilla.default:underline"                           , AlephThemePropType.Boolean   },
			{"scintilla.default:bold"                                , AlephThemePropType.Boolean   },
			{"scintilla.default:italic"                              , AlephThemePropType.Boolean   },

			{"scintilla.link:foreground"                             , AlephThemePropType.Color     },
			{"scintilla.link:background"                             , AlephThemePropType.Color     },
			{"scintilla.link:underline"                              , AlephThemePropType.Boolean   },
			{"scintilla.link:bold"                                   , AlephThemePropType.Boolean   },
			{"scintilla.link:italic"                                 , AlephThemePropType.Boolean   },

			{"scintilla.markdown.default:foreground"                 , AlephThemePropType.Color     },
			{"scintilla.markdown.default:background"                 , AlephThemePropType.Color     },
			{"scintilla.markdown.default:underline"                  , AlephThemePropType.Boolean   },
			{"scintilla.markdown.default:bold"                       , AlephThemePropType.Boolean   },
			{"scintilla.markdown.default:italic"                     , AlephThemePropType.Boolean   },

			{"scintilla.markdown.emph:foreground"                    , AlephThemePropType.Color     },
			{"scintilla.markdown.emph:background"                    , AlephThemePropType.Color     },
			{"scintilla.markdown.emph:underline"                     , AlephThemePropType.Boolean   },
			{"scintilla.markdown.emph:bold"                          , AlephThemePropType.Boolean   },
			{"scintilla.markdown.emph:italic"                        , AlephThemePropType.Boolean   },

			{"scintilla.markdown.strong_emph:foreground"             , AlephThemePropType.Color     },
			{"scintilla.markdown.strong_emph:background"             , AlephThemePropType.Color     },
			{"scintilla.markdown.strong_emph:underline"              , AlephThemePropType.Boolean   },
			{"scintilla.markdown.strong_emph:bold"                   , AlephThemePropType.Boolean   },
			{"scintilla.markdown.strong_emph:italic"                 , AlephThemePropType.Boolean   },

			{"scintilla.markdown.header:foreground"                  , AlephThemePropType.Color     },
			{"scintilla.markdown.header:background"                  , AlephThemePropType.Color     },
			{"scintilla.markdown.header:underline"                   , AlephThemePropType.Boolean   },
			{"scintilla.markdown.header:bold"                        , AlephThemePropType.Boolean   },
			{"scintilla.markdown.header:italic"                      , AlephThemePropType.Boolean   },

			{"scintilla.markdown.code:foreground"                    , AlephThemePropType.Color     },
			{"scintilla.markdown.code:background"                    , AlephThemePropType.Color     },
			{"scintilla.markdown.code:underline"                     , AlephThemePropType.Boolean   },
			{"scintilla.markdown.code:bold"                          , AlephThemePropType.Boolean   },
			{"scintilla.markdown.code:italic"                        , AlephThemePropType.Boolean   },

			{"scintilla.markdown.list:foreground"                    , AlephThemePropType.Color     },
			{"scintilla.markdown.list:background"                    , AlephThemePropType.Color     },
			{"scintilla.markdown.list:underline"                     , AlephThemePropType.Boolean   },
			{"scintilla.markdown.list:bold"                          , AlephThemePropType.Boolean   },
			{"scintilla.markdown.list:italic"                        , AlephThemePropType.Boolean   },

			{"scintilla.markdown.url:foreground"                     , AlephThemePropType.Color     },
			{"scintilla.markdown.url:background"                     , AlephThemePropType.Color     },
			{"scintilla.markdown.url:underline"                      , AlephThemePropType.Boolean   },
			{"scintilla.markdown.url:bold"                           , AlephThemePropType.Boolean   },
			{"scintilla.markdown.url:italic"                         , AlephThemePropType.Boolean   },

			{"scintilla.margin.numbers:foreground"                   , AlephThemePropType.Color     },
			{"scintilla.margin.numbers:background"                   , AlephThemePropType.Color     },
			{"scintilla.margin.numbers:underline"                    , AlephThemePropType.Boolean   },
			{"scintilla.margin.numbers:bold"                         , AlephThemePropType.Boolean   },
			{"scintilla.margin.numbers:italic"                       , AlephThemePropType.Boolean   },
			{"scintilla.margin.symbols:background"                   , AlephThemePropType.Color     },

			{"scintilla.search.local:color"                          , AlephThemePropType.Color     },
			{"scintilla.search.local:alpha"                          , AlephThemePropType.Integer   },
			{"scintilla.search.local:outline_alpha"                  , AlephThemePropType.Integer   },
			{"scintilla.search.local:under_text"                     , AlephThemePropType.Boolean   },

			{"scintilla.search.global:color"                         , AlephThemePropType.Color     },
			{"scintilla.search.global:alpha"                         , AlephThemePropType.Integer   },
			{"scintilla.search.global:outline_alpha"                 , AlephThemePropType.Integer   },
			{"scintilla.search.global:under_text"                    , AlephThemePropType.Boolean   },
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
