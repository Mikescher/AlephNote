using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AlephNote.Common.Themes
{
	[DebuggerDisplay("{Name} v{Version} ({SourceFilename})")]
	public sealed class AlephTheme
	{
		#region Definition

		public enum AlephThemePropType { Color, Brush, Thickness, Integer, Double, Boolean, CornerRadius }

		public static readonly Tuple<string, AlephThemePropType>[] THEME_PROPERTIES =
		{
			Tuple.Create("window.background",                                      AlephThemePropType.Brush       ),
			Tuple.Create("window.foreground",                                      AlephThemePropType.Brush       ),
			Tuple.Create("window.splitter",                                        AlephThemePropType.Brush       ),

			Tuple.Create("window.notetitle:foreground",                            AlephThemePropType.Brush       ),
			Tuple.Create("window.notetitle:caret",                                 AlephThemePropType.Brush       ),
			Tuple.Create("window.notetitle.placeholder",                           AlephThemePropType.Brush       ),
			Tuple.Create("window.changedate:foreground",                           AlephThemePropType.Brush       ),

			Tuple.Create("window.menubar:background",                              AlephThemePropType.Brush       ),
			Tuple.Create("window.menubar:foreground",                              AlephThemePropType.Brush       ),
			Tuple.Create("window.menubar.submenu:foreground",                      AlephThemePropType.Brush       ),

			Tuple.Create("window.tageditor.popup:background",                      AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.popup:bordercolor",                     AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.popup:foreground",                      AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.placeholder",                           AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.foreground",                            AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tag:background",                        AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tag:bordercolor_default",               AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tag:bordercolor_highlighted",           AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tag:foreground",                        AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tag:borderradius",                      AlephThemePropType.CornerRadius),
			Tuple.Create("window.tageditor.tag:highlight",                         AlephThemePropType.Color       ),
			Tuple.Create("window.tageditor.tagbutton:foreground",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tagbutton:background",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tagbutton:border",                      AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tagbutton:border_thickness",            AlephThemePropType.Thickness   ),
			Tuple.Create("window.tageditor.tagbutton.popup:background",            AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tagbutton.popup:bordercolor",           AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tagbutton.popup:foreground",            AlephThemePropType.Brush       ),
			Tuple.Create("window.tageditor.tagbutton.popup:border_thickness",      AlephThemePropType.Thickness   ),
			Tuple.Create("window.tageditor.tagbutton.popup:padding",               AlephThemePropType.Thickness   ),
			Tuple.Create("window.tageditor.tagbutton.popup:margin",                AlephThemePropType.Thickness   ),

			Tuple.Create("window.pathdisplay:foregroud",                           AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.element:foregroud",                   AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.element:background",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.element:background_hover",            AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.element:border",                      AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.button:foreground",                   AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.button:background",                   AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.button:border",                       AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup:background",                    AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup:border",                        AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup:border_thickness",              AlephThemePropType.Thickness   ),
			Tuple.Create("window.pathdisplay.popup.text:foreground",               AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.list:foreground",               AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.list:background",               AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.list.selected:foreground",      AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.list.selected:background",      AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.button:foreground",             AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.button:background",             AlephThemePropType.Brush       ),
			Tuple.Create("window.pathdisplay.popup.button:border",                 AlephThemePropType.Brush       ),

			Tuple.Create("window.inlinesearch:background",                         AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch:foreground",                         AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.textbox:background",                 AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.textbox:foreground",                 AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnSearch:background",               AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnSearch:foreground",               AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnCaseIns:background",              AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnCaseIns:foreground",              AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnWholeWord:background",            AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnWholeWord:foreground",            AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnRegex:background",                AlephThemePropType.Brush       ),
			Tuple.Create("window.inlinesearch.btnRegex:foreground",                AlephThemePropType.Brush       ),

			Tuple.Create("window.globalsearch.input:foreground",                   AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.input:background",                   AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.input:border",                       AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.placeholder:foreground",             AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.button:foreground",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.button:background",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.button:border",                      AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.button:border_thickness",            AlephThemePropType.Thickness   ),
			Tuple.Create("window.globalsearch.tagbutton:foreground",               AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.tagbutton:background",               AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.tagbutton:border",                   AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.tagbutton:border_thickness",         AlephThemePropType.Thickness   ),
			Tuple.Create("window.globalsearch.tagbutton.popup:background",         AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.tagbutton.popup:bordercolor",        AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.tagbutton.popup:foreground",         AlephThemePropType.Brush       ),
			Tuple.Create("window.globalsearch.tagbutton.popup:border_thickness",   AlephThemePropType.Thickness   ),
			Tuple.Create("window.globalsearch.tagbutton.popup:padding",            AlephThemePropType.Thickness   ),
			Tuple.Create("window.globalsearch.tagbutton.popup:margin",             AlephThemePropType.Thickness   ),

			Tuple.Create("window.notesview.flat:background",                       AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat:foreground",                       AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat:bordercolor",                      AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.selected:background",              AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.selected:foreground",              AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.datetime1:foreground",             AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.datetime2:foreground",             AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.preview:foreground",               AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.separator",                        AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.vborder:foreground",               AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.vborder:background",               AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.flat.vborder:margin",                   AlephThemePropType.Thickness   ),
			Tuple.Create("window.notesview.flat.scrollbar_v:visible",              AlephThemePropType.Boolean     ),

			Tuple.Create("window.notesview.hierachical.list:background",           AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list:foreground",           AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list:bordercolor",          AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.selected:background",  AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.selected:foreground",  AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.datetime1:foreground", AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.datetime2:foreground", AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.preview:foreground",   AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.separator",            AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.vborder:foreground",   AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.vborder:background",   AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.list.vborder:margin",       AlephThemePropType.Thickness   ),
			Tuple.Create("window.notesview.hierachical.list.scrollbar_v:visible",  AlephThemePropType.Boolean     ),
			Tuple.Create("window.notesview.hierachical.splitter",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.tree:background",           AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.tree:foreground",           AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.tree:bordercolor",          AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.tree.selected:background",  AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.tree.selected:foreground",  AlephThemePropType.Brush       ),
			Tuple.Create("window.notesview.hierachical.tree.scrollbar_v:visible",  AlephThemePropType.Boolean     ),

			Tuple.Create("window.statusbar:background",                            AlephThemePropType.Brush       ),
			Tuple.Create("window.statusbar:foreground",                            AlephThemePropType.Brush       ),
			Tuple.Create("window.statusbar.btnReload:foreground",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.statusbar.btnReload:background",                  AlephThemePropType.Brush       ),
			Tuple.Create("window.statusbar.btnReload:border",                      AlephThemePropType.Brush       ),

			//-------------------------------------------------------------------------------------

			Tuple.Create("scintilla.background",                                   AlephThemePropType.Color       ),
			Tuple.Create("scintilla.bordercolor",                                  AlephThemePropType.Brush       ),

			Tuple.Create("scintilla.whitespace:size",                              AlephThemePropType.Integer     ),
			Tuple.Create("scintilla.whitespace:color",                             AlephThemePropType.Color       ),
			Tuple.Create("scintilla.whitespace:background",                        AlephThemePropType.Color       ),

			Tuple.Create("scintilla.caret:foreground",                             AlephThemePropType.Color       ),
			Tuple.Create("scintilla.caret:background",                             AlephThemePropType.Color       ),
			Tuple.Create("scintilla.caret:background_alpha",                       AlephThemePropType.Integer     ),
			Tuple.Create("scintilla.caret:visible",                                AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.selection:foreground",                         AlephThemePropType.Color       ),
			Tuple.Create("scintilla.selection:override_foreground",                AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.selection:background",                         AlephThemePropType.Color       ),

			Tuple.Create("scintilla.scrollbar_h:visible",                          AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.scrollbar_v:visible",                          AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.default:foreground",                           AlephThemePropType.Color       ),
			Tuple.Create("scintilla.default:background",                           AlephThemePropType.Color       ),
			Tuple.Create("scintilla.default:underline",                            AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.default:bold",                                 AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.default:italic",                               AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.link:foreground",                              AlephThemePropType.Color       ),
			Tuple.Create("scintilla.link:background",                              AlephThemePropType.Color       ),
			Tuple.Create("scintilla.link:underline",                               AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.link:bold",                                    AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.link:italic",                                  AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.default:foreground",                  AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.default:background",                  AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.default:underline",                   AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.default:bold",                        AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.default:italic",                      AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.emph:foreground",                     AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.emph:background",                     AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.emph:underline",                      AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.emph:bold",                           AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.emph:italic",                         AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.strong_emph:foreground",              AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.strong_emph:background",              AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.strong_emph:underline",               AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.strong_emph:bold",                    AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.strong_emph:italic",                  AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.header:foreground",                   AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.header:background",                   AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.header:underline",                    AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.header:bold",                         AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.header:italic",                       AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.code:foreground",                     AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.code:background",                     AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.code:underline",                      AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.code:bold",                           AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.code:italic",                         AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.list:foreground",                     AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.list:background",                     AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.list:underline",                      AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.list:bold",                           AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.list:italic",                         AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.markdown.url:foreground",                      AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.url:background",                      AlephThemePropType.Color       ),
			Tuple.Create("scintilla.markdown.url:underline",                       AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.url:underline_link",                  AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.url:bold",                            AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.markdown.url:italic",                          AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.margin.numbers:foreground",                    AlephThemePropType.Color       ),
			Tuple.Create("scintilla.margin.numbers:background",                    AlephThemePropType.Color       ),
			Tuple.Create("scintilla.margin.numbers:underline",                     AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.margin.numbers:bold",                          AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.margin.numbers:italic",                        AlephThemePropType.Boolean     ),
			Tuple.Create("scintilla.margin.symbols:background",                    AlephThemePropType.Color       ),

			Tuple.Create("scintilla.search.local:color",                           AlephThemePropType.Color       ),
			Tuple.Create("scintilla.search.local:alpha",                           AlephThemePropType.Integer     ),
			Tuple.Create("scintilla.search.local:outline_alpha",                   AlephThemePropType.Integer     ),
			Tuple.Create("scintilla.search.local:under_text",                      AlephThemePropType.Boolean     ),

			Tuple.Create("scintilla.search.global:color",                          AlephThemePropType.Color       ),
			Tuple.Create("scintilla.search.global:alpha",                          AlephThemePropType.Integer     ),
			Tuple.Create("scintilla.search.global:outline_alpha",                  AlephThemePropType.Integer     ),
			Tuple.Create("scintilla.search.global:under_text",                     AlephThemePropType.Boolean     ),
		};

		public static readonly Tuple<string, Uri>[] RESOURCES =
		{
			Tuple.Create("folder_all.png",       new Uri("pack://application:,,,/AlephNote;component/Resources/folder_all.png")       ),
			Tuple.Create("folder_any.png",       new Uri("pack://application:,,,/AlephNote;component/Resources/folder_any.png")       ),
			Tuple.Create("folder_none.png",      new Uri("pack://application:,,,/AlephNote;component/Resources/folder_none.png")      ),
			Tuple.Create("folder_root.png",      new Uri("pack://application:,,,/AlephNote;component/Resources/folder_root.png")      ),

			Tuple.Create("IconGreen.ico",        new Uri("pack://application:,,,/AlephNote;component/Resources/IconGreen.ico")        ),
			Tuple.Create("IconRed.ico",          new Uri("pack://application:,,,/AlephNote;component/Resources/IconRed.ico")          ),
			Tuple.Create("IconSync.ico",         new Uri("pack://application:,,,/AlephNote;component/Resources/IconSync.ico")         ),
			Tuple.Create("IconYellow.ico",       new Uri("pack://application:,,,/AlephNote;component/Resources/IconYellow.ico")       ),

			Tuple.Create("lock.png",             new Uri("pack://application:,,,/AlephNote;component/Resources/lock.png")             ),
			Tuple.Create("lock_open.png",        new Uri("pack://application:,,,/AlephNote;component/Resources/lock_open.png")        ),
			Tuple.Create("lock_small.png",       new Uri("pack://application:,,,/AlephNote;component/Resources/lock_small.png")       ),

			Tuple.Create("plus.png",             new Uri("pack://application:,,,/AlephNote;component/Resources/plus.png")             ),
			Tuple.Create("refresh.png",          new Uri("pack://application:,,,/AlephNote;component/Resources/refresh.png")          ),
			Tuple.Create("star.png",             new Uri("pack://application:,,,/AlephNote;component/Resources/star.png")             ),
			Tuple.Create("tag.png",              new Uri("pack://application:,,,/AlephNote;component/Resources/tag.png")              ),

			Tuple.Create("margin_check_mix.png", new Uri("pack://application:,,,/AlephNote;component/Resources/margin_check_mix.png") ),
			Tuple.Create("margin_check_off.png", new Uri("pack://application:,,,/AlephNote;component/Resources/margin_check_off.png") ),
			Tuple.Create("margin_check_on.png",  new Uri("pack://application:,,,/AlephNote;component/Resources/margin_check_on.png")  ),

		};

		#endregion

		public string Name { get; }
		public Version Version { get; }
		public string Author { get; }
		public CompatibilityVersionRange Compatibility { get; }
		public string SourceFilename { get; }
		public string Source { get; }
		public AlephThemeType ThemeType { get; }
		public IReadOnlyDictionary<string, byte[]> Resources { get; }
		
		private readonly Dictionary<string, AlephThemePropertyValue> _allProperties = new Dictionary<string, AlephThemePropertyValue>();

		public AlephTheme(string n, Version v, CompatibilityVersionRange c, string fn, string src, string a, AlephThemeType att, IReadOnlyDictionary<string, byte[]> res)
		{
			Name           = n;
			Version        = v;
			Author         = a;
			Compatibility  = c;
			SourceFilename = fn;
			Source         = src;
			ThemeType      = att;
			Resources      = res;
		}

		public void AddProperty(string name, AlephThemePropertyValue value)
		{
			_allProperties.Add(name.ToLower(), value);
		}

		public AlephThemePropertyValue TryGet(string name)
		{
			return _allProperties.TryGetValue(name.ToLower(), out var result) ? result : null;
		}
		
		public string GetXmlStr(string name)
		{
			var obj = TryGet(name);
			return obj?.XmlDirectValue ?? "N/A";
		}

		public static Uri GetDefaultResourceUri(string name)
		{
			return RESOURCES.First(p => string.Equals(p.Item1, name, StringComparison.CurrentCultureIgnoreCase)).Item2;
		}
	}
}
