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

		public static readonly (string, AlephThemePropType)[] THEME_PROPERTIES =
		{
			("window.background",                                      AlephThemePropType.Brush       ),
			("window.foreground",                                      AlephThemePropType.Brush       ),
			("window.splitter",                                        AlephThemePropType.Brush       ),

			("window.notetitle:foreground",                            AlephThemePropType.Brush       ),
			("window.notetitle:caret",                                 AlephThemePropType.Brush       ),
			("window.notetitle.placeholder",                           AlephThemePropType.Brush       ),
			("window.changedate:foreground",                           AlephThemePropType.Brush       ),

			("window.menubar:background",                              AlephThemePropType.Brush       ),
			("window.menubar:foreground",                              AlephThemePropType.Brush       ),
			("window.menubar.submenu:foreground",                      AlephThemePropType.Brush       ),

			("window.tageditor.popup:background",                      AlephThemePropType.Brush       ),
			("window.tageditor.popup:bordercolor",                     AlephThemePropType.Brush       ),
			("window.tageditor.popup:foreground",                      AlephThemePropType.Brush       ),
			("window.tageditor.placeholder",                           AlephThemePropType.Brush       ),
			("window.tageditor.foreground",                            AlephThemePropType.Brush       ),
			("window.tageditor.tag:background",                        AlephThemePropType.Brush       ),
			("window.tageditor.tag:bordercolor_default",               AlephThemePropType.Brush       ),
			("window.tageditor.tag:bordercolor_highlighted",           AlephThemePropType.Brush       ),
			("window.tageditor.tag:foreground",                        AlephThemePropType.Brush       ),
			("window.tageditor.tag:borderradius",                      AlephThemePropType.CornerRadius),
			("window.tageditor.tag:highlight",                         AlephThemePropType.Color       ),
			("window.tageditor.tagbutton:foreground",                  AlephThemePropType.Brush       ),
			("window.tageditor.tagbutton:background",                  AlephThemePropType.Brush       ),
			("window.tageditor.tagbutton:border",                      AlephThemePropType.Brush       ),
			("window.tageditor.tagbutton:border_thickness",            AlephThemePropType.Thickness   ),
			("window.tageditor.tagbutton.popup:background",            AlephThemePropType.Brush       ),
			("window.tageditor.tagbutton.popup:bordercolor",           AlephThemePropType.Brush       ),
			("window.tageditor.tagbutton.popup:foreground",            AlephThemePropType.Brush       ),
			("window.tageditor.tagbutton.popup:border_thickness",      AlephThemePropType.Thickness   ),
			("window.tageditor.tagbutton.popup:padding",               AlephThemePropType.Thickness   ),
			("window.tageditor.tagbutton.popup:margin",                AlephThemePropType.Thickness   ),

			("window.pathdisplay:foregroud",                           AlephThemePropType.Brush       ),
			("window.pathdisplay.element:foregroud",                   AlephThemePropType.Brush       ),
			("window.pathdisplay.element:background",                  AlephThemePropType.Brush       ),
			("window.pathdisplay.element:background_hover",            AlephThemePropType.Brush       ),
			("window.pathdisplay.element:border",                      AlephThemePropType.Brush       ),
			("window.pathdisplay.button:foreground",                   AlephThemePropType.Brush       ),
			("window.pathdisplay.button:background",                   AlephThemePropType.Brush       ),
			("window.pathdisplay.button:border",                       AlephThemePropType.Brush       ),
			("window.pathdisplay.popup:background",                    AlephThemePropType.Brush       ),
			("window.pathdisplay.popup:border",                        AlephThemePropType.Brush       ),
			("window.pathdisplay.popup:border_thickness",              AlephThemePropType.Thickness   ),
			("window.pathdisplay.popup.text:foreground",               AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.list:foreground",               AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.list:background",               AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.list.selected:foreground",      AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.list.selected:background",      AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.button:foreground",             AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.button:background",             AlephThemePropType.Brush       ),
			("window.pathdisplay.popup.button:border",                 AlephThemePropType.Brush       ),

			("window.inlinesearch:background",                         AlephThemePropType.Brush       ),
			("window.inlinesearch:foreground",                         AlephThemePropType.Brush       ),
			("window.inlinesearch.textbox:background",                 AlephThemePropType.Brush       ),
			("window.inlinesearch.textbox:foreground",                 AlephThemePropType.Brush       ),
			("window.inlinesearch.btnSearch:background",               AlephThemePropType.Brush       ),
			("window.inlinesearch.btnSearch:foreground",               AlephThemePropType.Brush       ),
			("window.inlinesearch.btnCaseIns:background",              AlephThemePropType.Brush       ),
			("window.inlinesearch.btnCaseIns:foreground",              AlephThemePropType.Brush       ),
			("window.inlinesearch.btnWholeWord:background",            AlephThemePropType.Brush       ),
			("window.inlinesearch.btnWholeWord:foreground",            AlephThemePropType.Brush       ),
			("window.inlinesearch.btnRegex:background",                AlephThemePropType.Brush       ),
			("window.inlinesearch.btnRegex:foreground",                AlephThemePropType.Brush       ),

			("window.globalsearch.input:foreground",                   AlephThemePropType.Brush       ),
			("window.globalsearch.input:background",                   AlephThemePropType.Brush       ),
			("window.globalsearch.input:border",                       AlephThemePropType.Brush       ),
			("window.globalsearch.placeholder:foreground",             AlephThemePropType.Brush       ),
			("window.globalsearch.button:foreground",                  AlephThemePropType.Brush       ),
			("window.globalsearch.button:background",                  AlephThemePropType.Brush       ),
			("window.globalsearch.button:border",                      AlephThemePropType.Brush       ),
			("window.globalsearch.button:border_thickness",            AlephThemePropType.Thickness   ),
			("window.globalsearch.tagbutton:foreground",               AlephThemePropType.Brush       ),
			("window.globalsearch.tagbutton:background",               AlephThemePropType.Brush       ),
			("window.globalsearch.tagbutton:border",                   AlephThemePropType.Brush       ),
			("window.globalsearch.tagbutton:border_thickness",         AlephThemePropType.Thickness   ),
			("window.globalsearch.tagbutton.popup:background",         AlephThemePropType.Brush       ),
			("window.globalsearch.tagbutton.popup:bordercolor",        AlephThemePropType.Brush       ),
			("window.globalsearch.tagbutton.popup:foreground",         AlephThemePropType.Brush       ),
			("window.globalsearch.tagbutton.popup:border_thickness",   AlephThemePropType.Thickness   ),
			("window.globalsearch.tagbutton.popup:padding",            AlephThemePropType.Thickness   ),
			("window.globalsearch.tagbutton.popup:margin",             AlephThemePropType.Thickness   ),

			("window.notesview.flat:background",                       AlephThemePropType.Brush       ),
			("window.notesview.flat:foreground",                       AlephThemePropType.Brush       ),
			("window.notesview.flat.selected:background",              AlephThemePropType.Brush       ),
			("window.notesview.flat.selected:foreground",              AlephThemePropType.Brush       ),
			("window.notesview.flat.datetime1:foreground",             AlephThemePropType.Brush       ),
			("window.notesview.flat.datetime2:foreground",             AlephThemePropType.Brush       ),
			("window.notesview.flat.preview:foreground",               AlephThemePropType.Brush       ),
			("window.notesview.flat.separator",                        AlephThemePropType.Brush       ),
			("window.notesview.flat.vborder:foreground",               AlephThemePropType.Brush       ),
			("window.notesview.flat.vborder:background",               AlephThemePropType.Brush       ),
			("window.notesview.flat.vborder:margin",                   AlephThemePropType.Thickness   ),

			("window.notesview.hierachical.list:background",           AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list:foreground",           AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.selected:background",  AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.selected:foreground",  AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.datetime1:foreground", AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.datetime2:foreground", AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.preview:foreground",   AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.separator",            AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.vborder:foreground",   AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.vborder:background",   AlephThemePropType.Brush       ),
			("window.notesview.hierachical.list.vborder:margin",       AlephThemePropType.Thickness   ),
			("window.notesview.hierachical.splitter",                  AlephThemePropType.Brush       ),
			("window.notesview.hierachical.tree:background",           AlephThemePropType.Brush       ),
			("window.notesview.hierachical.tree:foreground",           AlephThemePropType.Brush       ),
			("window.notesview.hierachical.tree.selected:background",  AlephThemePropType.Brush       ),
			("window.notesview.hierachical.tree.selected:foreground",  AlephThemePropType.Brush       ),

			("window.statusbar:background",                            AlephThemePropType.Brush       ),
			("window.statusbar:foreground",                            AlephThemePropType.Brush       ),
			("window.statusbar.btnReload:foreground",                  AlephThemePropType.Brush       ),
			("window.statusbar.btnReload:background",                  AlephThemePropType.Brush       ),
			("window.statusbar.btnReload:border",                      AlephThemePropType.Brush       ),

			//-------------------------------------------------------------------------------------

			("scintilla.background",                                   AlephThemePropType.Color       ),

			("scintilla.whitespace:size",                              AlephThemePropType.Integer     ),
			("scintilla.whitespace:color",                             AlephThemePropType.Color       ),
			("scintilla.whitespace:background",                        AlephThemePropType.Color       ),

			("scintilla.caret:foreground",                             AlephThemePropType.Color       ),
			("scintilla.caret:background",                             AlephThemePropType.Color       ),
			("scintilla.caret:background_alpha",                       AlephThemePropType.Integer     ),
			("scintilla.caret:visible",                                AlephThemePropType.Boolean     ),

			("scintilla.selection:foreground",                         AlephThemePropType.Color       ),
			("scintilla.selection:override_foreground",                AlephThemePropType.Boolean     ),
			("scintilla.selection:background",                         AlephThemePropType.Color       ),

			("scintilla.scrollbar_h:visible",                          AlephThemePropType.Boolean     ),
			("scintilla.scrollbar_v:visible",                          AlephThemePropType.Boolean     ),

			("scintilla.default:foreground",                           AlephThemePropType.Color       ),
			("scintilla.default:background",                           AlephThemePropType.Color       ),
			("scintilla.default:underline",                            AlephThemePropType.Boolean     ),
			("scintilla.default:bold",                                 AlephThemePropType.Boolean     ),
			("scintilla.default:italic",                               AlephThemePropType.Boolean     ),

			("scintilla.link:foreground",                              AlephThemePropType.Color       ),
			("scintilla.link:background",                              AlephThemePropType.Color       ),
			("scintilla.link:underline",                               AlephThemePropType.Boolean     ),
			("scintilla.link:bold",                                    AlephThemePropType.Boolean     ),
			("scintilla.link:italic",                                  AlephThemePropType.Boolean     ),

			("scintilla.markdown.default:foreground",                  AlephThemePropType.Color       ),
			("scintilla.markdown.default:background",                  AlephThemePropType.Color       ),
			("scintilla.markdown.default:underline",                   AlephThemePropType.Boolean     ),
			("scintilla.markdown.default:bold",                        AlephThemePropType.Boolean     ),
			("scintilla.markdown.default:italic",                      AlephThemePropType.Boolean     ),

			("scintilla.markdown.emph:foreground",                     AlephThemePropType.Color       ),
			("scintilla.markdown.emph:background",                     AlephThemePropType.Color       ),
			("scintilla.markdown.emph:underline",                      AlephThemePropType.Boolean     ),
			("scintilla.markdown.emph:bold",                           AlephThemePropType.Boolean     ),
			("scintilla.markdown.emph:italic",                         AlephThemePropType.Boolean     ),

			("scintilla.markdown.strong_emph:foreground",              AlephThemePropType.Color       ),
			("scintilla.markdown.strong_emph:background",              AlephThemePropType.Color       ),
			("scintilla.markdown.strong_emph:underline",               AlephThemePropType.Boolean     ),
			("scintilla.markdown.strong_emph:bold",                    AlephThemePropType.Boolean     ),
			("scintilla.markdown.strong_emph:italic",                  AlephThemePropType.Boolean     ),

			("scintilla.markdown.header:foreground",                   AlephThemePropType.Color       ),
			("scintilla.markdown.header:background",                   AlephThemePropType.Color       ),
			("scintilla.markdown.header:underline",                    AlephThemePropType.Boolean     ),
			("scintilla.markdown.header:bold",                         AlephThemePropType.Boolean     ),
			("scintilla.markdown.header:italic",                       AlephThemePropType.Boolean     ),

			("scintilla.markdown.code:foreground",                     AlephThemePropType.Color       ),
			("scintilla.markdown.code:background",                     AlephThemePropType.Color       ),
			("scintilla.markdown.code:underline",                      AlephThemePropType.Boolean     ),
			("scintilla.markdown.code:bold",                           AlephThemePropType.Boolean     ),
			("scintilla.markdown.code:italic",                         AlephThemePropType.Boolean     ),

			("scintilla.markdown.list:foreground",                     AlephThemePropType.Color       ),
			("scintilla.markdown.list:background",                     AlephThemePropType.Color       ),
			("scintilla.markdown.list:underline",                      AlephThemePropType.Boolean     ),
			("scintilla.markdown.list:bold",                           AlephThemePropType.Boolean     ),
			("scintilla.markdown.list:italic",                         AlephThemePropType.Boolean     ),

			("scintilla.markdown.url:foreground",                      AlephThemePropType.Color       ),
			("scintilla.markdown.url:background",                      AlephThemePropType.Color       ),
			("scintilla.markdown.url:underline",                       AlephThemePropType.Boolean     ),
			("scintilla.markdown.url:underline_link",                  AlephThemePropType.Boolean     ),
			("scintilla.markdown.url:bold",                            AlephThemePropType.Boolean     ),
			("scintilla.markdown.url:italic",                          AlephThemePropType.Boolean     ),

			("scintilla.margin.numbers:foreground",                    AlephThemePropType.Color       ),
			("scintilla.margin.numbers:background",                    AlephThemePropType.Color       ),
			("scintilla.margin.numbers:underline",                     AlephThemePropType.Boolean     ),
			("scintilla.margin.numbers:bold",                          AlephThemePropType.Boolean     ),
			("scintilla.margin.numbers:italic",                        AlephThemePropType.Boolean     ),
			("scintilla.margin.symbols:background",                    AlephThemePropType.Color       ),

			("scintilla.search.local:color",                           AlephThemePropType.Color       ),
			("scintilla.search.local:alpha",                           AlephThemePropType.Integer     ),
			("scintilla.search.local:outline_alpha",                   AlephThemePropType.Integer     ),
			("scintilla.search.local:under_text",                      AlephThemePropType.Boolean     ),

			("scintilla.search.global:color",                          AlephThemePropType.Color       ),
			("scintilla.search.global:alpha",                          AlephThemePropType.Integer     ),
			("scintilla.search.global:outline_alpha",                  AlephThemePropType.Integer     ),
			("scintilla.search.global:under_text",                     AlephThemePropType.Boolean     ),
		};

		public static readonly (string, Uri)[] RESOURCES =
		{
			("folder_all.png",       new Uri("pack://application:,,,/AlephNote;component/Resources/folder_all.png")       ),
			("folder_any.png",       new Uri("pack://application:,,,/AlephNote;component/Resources/folder_any.png")       ),
			("folder_none.png",      new Uri("pack://application:,,,/AlephNote;component/Resources/folder_none.png")      ),
			("folder_root.png",      new Uri("pack://application:,,,/AlephNote;component/Resources/folder_root.png")      ),

			("IconGreen.ico",        new Uri("pack://application:,,,/AlephNote;component/Resources/IconGreen.ico")        ),
			("IconRed.ico",          new Uri("pack://application:,,,/AlephNote;component/Resources/IconRed.ico")          ),
			("IconSync.ico",         new Uri("pack://application:,,,/AlephNote;component/Resources/IconSync.ico")         ),
			("IconYellow.ico",       new Uri("pack://application:,,,/AlephNote;component/Resources/IconYellow.ico")       ),
			
			("lock.png",             new Uri("pack://application:,,,/AlephNote;component/Resources/lock.png")             ),
			("lock_open.png",        new Uri("pack://application:,,,/AlephNote;component/Resources/lock_open.png")        ),
			("lock_small.png",       new Uri("pack://application:,,,/AlephNote;component/Resources/lock_small.png")       ),

			("plus.png",             new Uri("pack://application:,,,/AlephNote;component/Resources/plus.png")             ),
			("refresh.png",          new Uri("pack://application:,,,/AlephNote;component/Resources/refresh.png")          ),
			("star.png",             new Uri("pack://application:,,,/AlephNote;component/Resources/star.png")             ),
			("tag.png",              new Uri("pack://application:,,,/AlephNote;component/Resources/tag.png")              ),

			("margin_check_mix.png", new Uri("pack://application:,,,/AlephNote;component/Resources/margin_check_mix.png") ),
			("margin_check_off.png", new Uri("pack://application:,,,/AlephNote;component/Resources/margin_check_off.png") ),
			("margin_check_on.png",  new Uri("pack://application:,,,/AlephNote;component/Resources/margin_check_on.png")  ),

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
