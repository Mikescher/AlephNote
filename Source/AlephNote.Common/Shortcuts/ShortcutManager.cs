using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using AlephNote.Common.Util;
using AlephNote.PluginInterface.AppContext;
using MSHC.WPF.MVVM;

namespace AlephNote.Common.Shortcuts
{
	using ASS = AlephShortcutScope;

	public static class ShortcutManager
	{
		[Flags]
		public enum ActionModifier { None=0, Disabled=1, AccessControl=2, DebugOnly=4, FromSnippet=8}

		private class AlephAction
		{
			private readonly Func<AlephAction, IShortcutHandlerParent, bool> _run;
			public readonly string Description;
			public readonly ActionModifier Modifier;
			public readonly AlephShortcutScope DefaultScope;

			public AlephAction(AlephShortcutScope s, Func<AlephAction, IShortcutHandlerParent, bool> e, string d, ActionModifier m) { DefaultScope = s; _run = e; Description = d; Modifier = m; }

			public bool Run(IShortcutHandlerParent w)
			{
				if (Modifier.HasFlag(ActionModifier.Disabled)) return false;
				if (Modifier.HasFlag(ActionModifier.DebugOnly) && !AlephAppContext.DebugMode) return false;
				if (Modifier.HasFlag(ActionModifier.AccessControl) && w.Settings.IsReadOnlyMode) return false;

				return _run(this, w);
			}
		}

		private static readonly Dictionary<string, AlephAction> _actions = new Dictionary<string, AlephAction>();

		static ShortcutManager()
		{
			AddCommand(ASS.Window,     "NewNote",                          h => h.CreateNewNoteCommand,                    "Create a new note",                                        ActionModifier.AccessControl); 
			AddCommand(ASS.Window,     "NewNoteFromClipboard",             h => h.CreateNewNoteFromClipboardCommand,       "Create a new note from current clipboard content",         ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "NewNoteFromTextFile",              h => h.CreateNewNoteFromTextfileCommand,        "Create a new note from a file",                            ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "ExportNote",                       h => h.ExportCommand,                           "Export the current note");
			AddCommand(ASS.Window,     "DeleteNote",                       h => h.DeleteCommand,                           "Delete the current note",                                  ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "DeleteFolder",                     h => h.DeleteFolderCommand,                     "Delete the current selected folder",                       ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "AddSubFolder",                     h => h.AddSubFolderCommand,                     "Add a new sub folder under the currently selected folder", ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "AddFolder",                        h => h.AddFolderCommand,                        "Add a new folder",                                         ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "RenameFolder",                     h => h.RenameFolderCommand,                     "Rename currently selected folder",                         ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "ChangeNotePath",                   h => h.ChangePathCommand,                       "Change the path of the currently selected note",           ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "DuplicateNote",                    h => h.DuplicateNoteCommand,                    "Create a new note as a copy of the current note",          ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "PinUnpinNote",                     h => h.PinUnpinNoteCommand,                     "Pin the note to the top (or un-pin the note)",             ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "LockUnlockNote",                   h => h.LockUnlockNoteCommand,                   "Lock/Unlock the note (prevent editing)",                   ActionModifier.AccessControl);

			AddCommand(ASS.NoteEdit,   "InsertHyperlink",                  h => h.InsertHyperlinkCommand,                  "Insert a Hyperlink",                                       ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "InsertFilelink",                   h => h.InsertFilelinkCommand,                   "Insert a link to a local file",                            ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "InsertNotelink",                   h => h.InsertNotelinkCommand,                   "Insert a link to another note",                            ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "InsertMaillink",                   h => h.InsertMaillinkCommand,                   "Insert a clickable mail address",                          ActionModifier.AccessControl);

			AddCommand(ASS.NoteEdit,   "MoveCurrentLineUp",                h => h.MoveCurrentLineUpCommand,                "Move the currently selected line one up",                  ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "MoveCurrentLineDown",              h => h.MoveCurrentLineDownCommand,              "Move the currently selected line one down",                ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "DuplicateCurrentLine",             h => h.DuplicateCurrentLineCommand,             "Duplicate the current line",                               ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "CopyCurrentLine",                  h => h.CopyCurrentLineCommand,                  "Copy the currently selected line to the clipboard",        ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "CutCurrentLine",                   h => h.CutCurrentLineCommand,                   "Cut the currently selected line to the clipboard",         ActionModifier.AccessControl);

			AddCommand(ASS.Window,     "SaveAndSync",                      h => h.SaveAndSyncCommand,                      "Save current note and synchronize");
			AddCommand(ASS.Window,     "Resync",                           h => h.ResyncCommand,                           "Start synchronization with remote");
			AddCommand(ASS.Window,     "FullResync",                       h => h.FullResyncCommand,                       "Delete local data and do a full resync",                   ActionModifier.AccessControl);

			AddCommand(ASS.NoteEdit,   "DocumentSearch",                   h => h.DocumentSearchCommand,                   "Search in current note");
			AddCommand(ASS.NoteEdit,   "DocumentSearchNext",               h => h.DocumentContinueSearchCommand,           "Find next occurence of search");
			AddCommand(ASS.NoteEdit,   "CloseDocumentSearch",              h => h.CloseDocumentSearchCommand,              "Close inline search panel");

			AddCommand(ASS.Window,     "ShowSettings",                     h => h.SettingsCommand,                         "Show settings window");
			AddCommand(ASS.Window,     "ShowMainWindow",                   h => h.ShowMainWindowCommand,                   "Bring the window into the foreground");
			AddCommand(ASS.Window,     "ShowAbout",                        h => h.ShowAboutCommand,                        "Show the about window");
			AddCommand(ASS.Window,     "ShowLog",                          h => h.ShowLogCommand,                          "Show the log window");
			AddCommand(ASS.Window,     "AppExit",                          h => h.ExitCommand,                             "Close the application");
			AddCommand(ASS.Window,     "AppRestart",                       h => h.RestartCommand,                          "Restart the application");
			AddCommand(ASS.Window,     "AppHide",                          h => h.HideCommand,                             "Hide main window");
			AddCommand(ASS.Window,     "AppToggleVisibility",              h => h.AppToggleVisibilityCommand,              "Hide/Show main window (combination of [AppHide] and [ShowMainWindow])");
			AddCommand(ASS.Window,     "FocusEditor",                      h => h.FocusScintillaCommand,                   "Select the note editor");
			AddCommand(ASS.Window,     "FocusNotesList",                   h => h.FocusNotesListCommand,                   "Select the note list");
			AddCommand(ASS.Window,     "FocusGlobalSearch",                h => h.FocusGlobalSearchCommand,                "Select the global search field");
			AddCommand(ASS.Window,     "FocusFolderList",                  h => h.FocusFolderCommand,                      "Select the folder list");

			AddCommand(ASS.NoteEdit,   "FocusPrevNote",                    h => h.FocusPrevNoteCommand,                    "Select the previous note");
			AddCommand(ASS.NoteEdit,   "FocusNextNote",                    h => h.FocusNextNoteCommand,                    "Select the next note");
			AddCommand(ASS.NoteEdit,   "FocusPrevDirectoryAny",            h => h.FocusPrevDirectoryAnyCommand,            "Select the previous directory");
			AddCommand(ASS.NoteEdit,   "FocusNextDirectoryAny",            h => h.FocusNextDirectoryAnyCommand,            "Select the next directory");
			AddCommand(ASS.NoteEdit,   "FocusPrevDirectory",               h => h.FocusPrevDirectoryCommand,               "Select the previous directory (on the same level)");
			AddCommand(ASS.NoteEdit,   "FocusNextDirectory",               h => h.FocusNextDirectoryCommand,               "Select the next directory (on the same level)");
			AddCommand(ASS.NoteEdit,   "FocusDirectoryLevelDown",          h => h.FocusDirectoryLevelDownCommand,          "Go one directory level down");
			AddCommand(ASS.NoteEdit,   "FocusDirectoryLevelUp",            h => h.FocusDirectoryLevelUpCommand,            "Go one directory level up");
			AddCommand(ASS.NoteEdit,   "CopyAllowLine",                    h => h.CopyAllowLineCommand,                    "Copy selection or current line if no text is selected");
			AddCommand(ASS.NoteEdit,   "CutAllowLine",                     h => h.CutAllowLineCommand,                     "Cut selection or current line if no text is selected");

			AddCommand(ASS.Window,     "CheckForUpdates",                  h => h.ManuallyCheckForUpdatesCommand,          "Manually check for new updates");

			AddCommand(ASS.Window,     "ToggleAlwaysOnTop",                h => h.SettingAlwaysOnTopCommand,               "Toggle the option 'Always on top'");
			AddCommand(ASS.Window,     "ToggleLineNumbers",                h => h.SettingLineNumbersCommand,               "Toggle the option 'Show line numbers'"); 
			AddCommand(ASS.Window,     "ToggleWordWrap",                   h => h.SettingsWordWrapCommand,                 "Toggle the option 'Word Wrap'");
			AddCommand(ASS.Window,     "RotateTheme",                      h => h.SettingsRotateThemeCommand,              "Change the current theme to the next available");
			AddCommand(ASS.Window,     "ToggleReadonly",                   h => h.SettingReadonlyModeCommand,              "Toggle the option 'Readonly mode'");
			
			AddCommand(ASS.Window,     "SetPreviewStyleSimple",            h => h.SetPreviewStyleSimpleCommand,            "Set the note preview style to 'Simple one line'");
			AddCommand(ASS.Window,     "SetPreviewStyleExtended",          h => h.SetPreviewStyleExtendedCommand,          "Set the note preview style to 'One line with date'");
			AddCommand(ASS.Window,     "SetPreviewStyleSingleLinePreview", h => h.SetPreviewStyleSingleLinePreviewCommand, "Set the note preview style to 'Title and first line'");
			AddCommand(ASS.Window,     "SetPreviewStyleFullPreview",       h => h.SetPreviewStyleFullPreviewCommand,       "Set the note preview style to 'Multiple lines with preview'");
			
			AddCommand(ASS.Window,     "SetNoteSortingNone",               h => h.SetNoteSortingNoneCommand,               "Set the note sorting to 'None'");
			AddCommand(ASS.Window,     "SetNoteSortingByName",             h => h.SetNoteSortingByNameCommand,             "Set the note sorting to 'Title'");
			AddCommand(ASS.Window,     "SetNoteSortingByCreationDate",     h => h.SetNoteSortingByCreationDateCommand,     "Set the note sorting to 'Creation date'");
			AddCommand(ASS.Window,     "SetNoteSortingByModificationDate", h => h.SetNoteSortingByModificationDateCommand, "Set the note sorting to 'Last modified date'");
			
			AddCommand(ASS.Window,     "RotateNoteProvider",               h => h.RotateNoteProviderCommand,               "Switch to the next note provider in the list");
		}

		public static void Execute(IShortcutHandlerParent mw, string key)
		{
			LoggerSingleton.Inst.Trace("ShortcutManager", $"Execute Action [{key}]");

			if (_actions.TryGetValue(key, out var action))
			{
				action.Run(mw);
			}
		}

		public static string GetGestureStr(IShortcutHandlerParent mw, string key)
		{
			if (_actions.ContainsKey(key))
			{
				if (mw.Settings != null && mw.Settings.Shortcuts.TryGetValue(key, out var shortcut))
				{
					return shortcut.GetGestureStr();
				}
			}
			return string.Empty;
		}

		private static void AddCommand(AlephShortcutScope scope, string k, Func<IShortcutHandler, ICommand> a, string desc, ActionModifier mod = ActionModifier.None)
		{
			bool Exec(AlephAction src, IShortcutHandlerParent w)
			{
				var vm = w.GetShortcutHandler();
				if (vm == null) return false;

				var c = a(vm);
				if (!c.CanExecute(null)) return false;

				c.Execute(null);
				return true;
			}

			_actions.Add(k, new AlephAction(scope, Exec, desc, mod));
		}

		public static bool Contains(string snipkey)
		{
			return _actions.ContainsKey(snipkey);
		}

		public static void UpdateSnippetCommands(IReadOnlyDictionary<string, SnippetDefinition> snippetsData)
		{
			foreach (var a in _actions.ToList())
			{
				if (!a.Key.StartsWith("Snippet::")) continue;
				RemoveSnippetCommand(a.Key);
			}

			foreach (var snip in snippetsData)
			{
				var snipactionkey = "Snippet::" + snip.Key;
				AddSnippetCommand(snipactionkey, snip.Value.Value, snip.Value.DisplayName);
			}
		}

		private static void AddSnippetCommand(string snippetactionkey, string snippetvalue, string displayname)
		{
			bool Exec(AlephAction src, IShortcutHandlerParent w)
			{
				var vm = w.GetShortcutHandler();
				if (vm == null) return false;

				var c = vm.InsertSnippetCommand;
				if (!c.CanExecute(null)) return false;

				c.Execute(snippetvalue);
				return true;
			}

			_actions.Add(snippetactionkey, new AlephAction(AlephShortcutScope.NoteEdit, Exec, $"Inserts the snippet '{displayname}'", ActionModifier.FromSnippet | ActionModifier.AccessControl));
		}

		private static void RemoveSnippetCommand(string snippetactionkey)
		{
			_actions.Remove(snippetactionkey);
		}

		public static ObservableCollectionNoReset<ObservableShortcutConfig> ListObservableShortcuts(AppSettings settings)
		{
			var result = new ObservableCollectionNoReset<ObservableShortcutConfig>();

			foreach (var a in _actions)
			{
				if (settings.Shortcuts.TryGetValue(a.Key, out var def))
				{
					result.Add(new ObservableShortcutConfig(a.Key, a.Value.Description, def.Key, def.Modifiers, def.Scope));
				}
				else
				{
					result.Add(new ObservableShortcutConfig(a.Key, a.Value.Description, AlephKey.None, AlephModifierKeys.None, a.Value.DefaultScope));
				}
			}

			return result;

		}

		public static IEnumerable<Tuple<ObservableShortcutConfig, ObservableShortcutConfig>> ListConflicts(IEnumerable<ObservableShortcutConfig> input)
		{
			var shortcuts = input.Where(s => s.Scope != AlephShortcutScope.None && s.Key != AlephKey.None).ToList();

			foreach (var s1 in shortcuts)
			{
				HashSet<AlephShortcutScope> conflictScopes;
				switch (s1.Scope)
				{
					case AlephShortcutScope.None:
						conflictScopes = new HashSet<AlephShortcutScope>{ AlephShortcutScope.None };
						break;
					case AlephShortcutScope.Window:
						conflictScopes = new HashSet<AlephShortcutScope>{ AlephShortcutScope.Window, AlephShortcutScope.Global };
						break;
					case AlephShortcutScope.NoteList:
						conflictScopes = new HashSet<AlephShortcutScope>{ AlephShortcutScope.NoteList, AlephShortcutScope.Window, AlephShortcutScope.Global };
						break;
					case AlephShortcutScope.FolderList:
						conflictScopes = new HashSet<AlephShortcutScope>{ AlephShortcutScope.FolderList, AlephShortcutScope.Window, AlephShortcutScope.Global };
						break;
					case AlephShortcutScope.NoteEdit:
						conflictScopes = new HashSet<AlephShortcutScope>{ AlephShortcutScope.NoteEdit, AlephShortcutScope.Window, AlephShortcutScope.Global };
						break;
					case AlephShortcutScope.Global:
						conflictScopes = new HashSet<AlephShortcutScope>{ AlephShortcutScope.Global };
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				foreach (var s2 in shortcuts)
				{
					if (s1 == s2) continue;
					if (!conflictScopes.Contains(s2.Scope)) continue;

					if (s1.Key != s2.Key) continue;

					if ((s1.Modifiers & s2.Modifiers) == s1.Modifiers || (s1.Modifiers & s2.Modifiers) == s2.Modifiers)
					{
						yield return Tuple.Create(s1, s2);
					}
				}

			}
		}

		public static KeyValueFlatCustomList<ShortcutDefinition> CreateDefaultShortcutList()
		{
			return new KeyValueFlatCustomList<ShortcutDefinition>(new[]
			{
				Tuple.Create("NewNote",              new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Control, AlephKey.N)),       // v1.6.00
				Tuple.Create("NewNoteFromTextFile",  new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Control, AlephKey.O)),       // v1.6.06
				Tuple.Create("FocusPrevNote",        new ShortcutDefinition(AlephShortcutScope.NoteEdit,   AlephModifierKeys.Alt,     AlephKey.Up)),      // v1.6.19
				Tuple.Create("FocusNextNote",        new ShortcutDefinition(AlephShortcutScope.NoteEdit,   AlephModifierKeys.Alt,     AlephKey.Down)),    // v1.6.19
				Tuple.Create("SaveAndSync",          new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Control, AlephKey.S)),       // v1.6.00
				Tuple.Create("DocumentSearch",       new ShortcutDefinition(AlephShortcutScope.NoteEdit,   AlephModifierKeys.Control, AlephKey.F)),       // v1.6.00
				Tuple.Create("DocumentSearchNext",   new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.None,    AlephKey.F3)),      // v1.6.17
				Tuple.Create("CloseDocumentSearch",  new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.None,    AlephKey.Escape)),  // v1.6.00
				Tuple.Create("DuplicateCurrentLine", new ShortcutDefinition(AlephShortcutScope.NoteEdit,   AlephModifierKeys.Control, AlephKey.D)),       // v1.6.30
				Tuple.Create("DeleteNote",           new ShortcutDefinition(AlephShortcutScope.NoteList,   AlephModifierKeys.None,    AlephKey.Delete)),  // v1.6.00
				Tuple.Create("AppExit",              new ShortcutDefinition(AlephShortcutScope.Window,     AlephModifierKeys.Alt,     AlephKey.F4)),      // v1.6.00
				Tuple.Create("DeleteFolder",         new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None,    AlephKey.Delete)),  // v1.6.04
				Tuple.Create("RenameFolder",         new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None,    AlephKey.F2)),      // v1.6.04
			}, 
			ShortcutDefinition.DEFAULT);
		}
		
		// ReSharper disable InconsistentNaming
		public static KeyValueFlatCustomList<ShortcutDefinition> MigrateShortcutSettings(Version from, Version to, KeyValueFlatCustomList<ShortcutDefinition> shortcuts)
		{
			var v1_6_04 = new Version(1, 6,  4, 0);
			var v1_6_06 = new Version(1, 6,  6, 0);
			var v1_6_17 = new Version(1, 6, 17, 0);
			var v1_6_19 = new Version(1, 6, 19, 0);
			var v1_6_30 = new Version(1, 6, 30, 0);
			
			if (from < v1_6_04)
			{
				if (shortcuts.All(sc => sc.Key != "DeleteFolder"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [DeleteFolder]");
					shortcuts = shortcuts.Concat(Tuple.Create("DeleteFolder", new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None, AlephKey.Delete)));
				}
			}
			if (from < v1_6_04)
			{
				if (shortcuts.All(sc => sc.Key != "RenameFolder"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [RenameFolder]");
					shortcuts = shortcuts.Concat(Tuple.Create("RenameFolder", new ShortcutDefinition(AlephShortcutScope.FolderList, AlephModifierKeys.None, AlephKey.F2)));
				}
			}
			if (from < v1_6_06)
			{
				if (shortcuts.All(sc => sc.Key != "NewNoteFromTextFile"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [NewNoteFromTextFile]");
					shortcuts = shortcuts.Concat(Tuple.Create("NewNoteFromTextFile", new ShortcutDefinition(AlephShortcutScope.Window, AlephModifierKeys.Control, AlephKey.O)));
				}
			}
			if (from < v1_6_17)
			{
				if (shortcuts.All(sc => sc.Key != "DocumentSearchNext"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [DocumentSearchNext]");
					shortcuts = shortcuts.Concat(Tuple.Create("DocumentSearchNext", new ShortcutDefinition(AlephShortcutScope.Window, AlephModifierKeys.None, AlephKey.F3)));
				}
			}
			if (from < v1_6_19)
			{
				if (shortcuts.All(sc => sc.Key != "FocusPrevNote"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [FocusPrevNote]");
					shortcuts = shortcuts.Concat(Tuple.Create("FocusPrevNote", new ShortcutDefinition(AlephShortcutScope.NoteEdit, AlephModifierKeys.Alt, AlephKey.Up)));
				}
				if (shortcuts.All(sc => sc.Key != "FocusNextNote"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [FocusNextNote]");
					shortcuts = shortcuts.Concat(Tuple.Create("FocusNextNote", new ShortcutDefinition(AlephShortcutScope.NoteEdit, AlephModifierKeys.Alt, AlephKey.Down)));
				}
			}
			if (from < v1_6_30)
			{
				if (shortcuts.All(sc => sc.Key != "DuplicateCurrentLine"))
				{
					LoggerSingleton.Inst.Info("AppSettings", "(Migration) Insert shortcut for [DuplicateCurrentLine]");
					shortcuts = shortcuts.Concat(Tuple.Create("DuplicateCurrentLine", new ShortcutDefinition(AlephShortcutScope.NoteEdit, AlephModifierKeys.Control, AlephKey.D)));
				}
			}

			return shortcuts;
		}
	}
}
