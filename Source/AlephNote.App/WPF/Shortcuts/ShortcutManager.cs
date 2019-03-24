using AlephNote.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Shortcuts
{
	using ASS = AlephShortcutScope;

	public static class ShortcutManager
	{
		[Flags]
		public enum ActionModifier { None=0, Disabled=1, AccessControl=2, DebugOnly=4, FromSnippet=8}

		private class AlephAction
		{
			private readonly Func<AlephAction, MainWindow, bool> _run;
			public readonly string Description;
			public readonly ActionModifier Modifier;
			public readonly AlephShortcutScope DefaultScope;

			public AlephAction(AlephShortcutScope s, Func<AlephAction, MainWindow, bool> e, string d, ActionModifier m) { DefaultScope = s; _run = e; Description = d; Modifier = m; }

			public bool Run(MainWindow w)
			{
				if (Modifier.HasFlag(ActionModifier.Disabled)) return false;
				if (Modifier.HasFlag(ActionModifier.DebugOnly) && !App.DebugMode) return false;
				if (Modifier.HasFlag(ActionModifier.AccessControl) && w.Settings.IsReadOnlyMode) return false;

				return _run(this, w);
			}

		}

		private static readonly Dictionary<string, AlephAction> _actions = new Dictionary<string, AlephAction>();

		static ShortcutManager()
		{
			AddCommand(ASS.Window,     "NewNote",                          vm => vm.CreateNewNoteCommand,                    "Create a new note",                                        ActionModifier.AccessControl); 
			AddCommand(ASS.Window,     "NewNoteFromClipboard",             vm => vm.CreateNewNoteFromClipboardCommand,       "Create a new note from current clipboard content",         ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "NewNoteFromTextFile",              vm => vm.CreateNewNoteFromTextfileCommand,        "Create a new note from a file",                            ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "ExportNote",                       vm => vm.ExportCommand,                           "Export the current note");
			AddCommand(ASS.Window,     "DeleteNote",                       vm => vm.DeleteCommand,                           "Delete the current note",                                  ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "DeleteFolder",                     vm => vm.DeleteFolderCommand,                     "Delete the current selected folder",                       ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "AddSubFolder",                     vm => vm.AddSubFolderCommand,                     "Add a new sub folder under the currently selected folder", ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "AddFolder",                        vm => vm.AddFolderCommand,                        "Add a new folder",                                         ActionModifier.AccessControl);
			AddCommand(ASS.FolderList, "RenameFolder",                     vm => vm.RenameFolderCommand,                     "Rename currently selected folder",                         ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "ChangeNotePath",                   vm => vm.ChangePathCommand,                       "Change the path of the currently selected note",           ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "DuplicateNote",                    vm => vm.DuplicateNoteCommand,                    "Create a new note as a copy of the current note",          ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "PinUnpinNote",                     vm => vm.PinUnpinNoteCommand,                     "Pin the note to the top (or un-pin the note)",             ActionModifier.AccessControl);
			AddCommand(ASS.Window,     "LockUnlockNote",                   vm => vm.LockUnlockNoteCommand,                   "Lock/Unlock the note (prevent editing)",                   ActionModifier.AccessControl);

			AddCommand(ASS.NoteEdit,   "InsertHyperlink",                  vm => vm.InsertHyperlinkCommand,                  "Insert a Hyperlink",                                       ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "InsertFilelink",                   vm => vm.InsertFilelinkCommand,                   "Insert a link to a local file",                            ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "InsertNotelink",                   vm => vm.InsertNotelinkCommand,                   "Insert a link to another note",                            ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "InsertMaillink",                   vm => vm.InsertMaillinkCommand,                   "Insert a clickable mail address",                          ActionModifier.AccessControl);

			AddCommand(ASS.NoteEdit,   "MoveCurrentLineUp",                vm => vm.MoveCurrentLineUpCommand,                "Move the currently selected line one up",                  ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "MoveCurrentLineDown",              vm => vm.MoveCurrentLineDownCommand,              "Move the currently selected line one down",                ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "DuplicateCurrentLine",             vm => vm.DuplicateCurrentLineCommand,             "Duplicate the current line",                               ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "CopyCurrentLine",                  vm => vm.CopyCurrentLineCommand,                  "Copy the currently selected line to the clipboard",        ActionModifier.AccessControl);
			AddCommand(ASS.NoteEdit,   "CutCurrentLine",                   vm => vm.CutCurrentLineCommand,                   "Cut the currently selected line to the clipboard",         ActionModifier.AccessControl);

			AddCommand(ASS.Window,     "SaveAndSync",                      vm => vm.SaveAndSyncCommand,                      "Save current note and synchronize");
			AddCommand(ASS.Window,     "Resync",                           vm => vm.ResyncCommand,                           "Start synchronization with remote");
			AddCommand(ASS.Window,     "FullResync",                       vm => vm.FullResyncCommand,                       "Delete local data and do a full resync",                   ActionModifier.AccessControl);

			AddCommand(ASS.NoteEdit,   "DocumentSearch",                   vm => vm.DocumentSearchCommand,                   "Search in current note");
			AddCommand(ASS.NoteEdit,   "DocumentSearchNext",               vm => vm.DocumentContinueSearchCommand,           "Find next occurence of search");
			AddCommand(ASS.NoteEdit,   "CloseDocumentSearch",              vm => vm.CloseDocumentSearchCommand,              "Close inline search panel");

			AddCommand(ASS.Window,     "ShowSettings",                     vm => vm.SettingsCommand,                         "Show settings window");
			AddCommand(ASS.Window,     "ShowMainWindow",                   vm => vm.ShowMainWindowCommand,                   "Bring the window into the foreground");
			AddCommand(ASS.Window,     "ShowAbout",                        vm => vm.ShowAboutCommand,                        "Show the about window");
			AddCommand(ASS.Window,     "ShowLog",                          vm => vm.ShowLogCommand,                          "Show the log window");
			AddCommand(ASS.Window,     "AppExit",                          vm => vm.ExitCommand,                             "Close the application");
			AddCommand(ASS.Window,     "AppRestart",                       vm => vm.RestartCommand,                          "Restart the application");
			AddCommand(ASS.Window,     "AppHide",                          vm => vm.HideCommand,                             "Hide main window");
			AddCommand(ASS.Window,     "FocusEditor",                      vm => vm.FocusScintillaCommand,                   "Select the note editor");
			AddCommand(ASS.Window,     "FocusNotesList",                   vm => vm.FocusNotesListCommand,                   "Select the note list");
			AddCommand(ASS.Window,     "FocusGlobalSearch",                vm => vm.FocusGlobalSearchCommand,                "Select the global search field");
			AddCommand(ASS.Window,     "FocusFolderList",                  vm => vm.FocusFolderCommand,                      "Select the folder list");

			AddCommand(ASS.NoteEdit,   "FocusPrevNote",                    vm => vm.FocusPrevNoteCommand,                    "Select the previous note");
			AddCommand(ASS.NoteEdit,   "FocusNextNote",                    vm => vm.FocusNextNoteCommand,                    "Select the next note");
			AddCommand(ASS.NoteEdit,   "FocusPrevDirectoryAny",            vm => vm.FocusPrevDirectoryAnyCommand,            "Select the previous directory");
			AddCommand(ASS.NoteEdit,   "FocusNextDirectoryAny",            vm => vm.FocusNextDirectoryAnyCommand,            "Select the next directory");
			AddCommand(ASS.NoteEdit,   "FocusPrevDirectory",               vm => vm.FocusPrevDirectoryCommand,               "Select the previous directory (on the same level)");
			AddCommand(ASS.NoteEdit,   "FocusNextDirectory",               vm => vm.FocusNextDirectoryCommand,               "Select the next directory (on the same level)");
			AddCommand(ASS.NoteEdit,   "FocusDirectoryLevelDown",          vm => vm.FocusDirectoryLevelDownCommand,          "Go one directory level down");
			AddCommand(ASS.NoteEdit,   "FocusDirectoryLevelUp",            vm => vm.FocusDirectoryLevelUpCommand,            "Go one directory level up");
			AddCommand(ASS.NoteEdit,   "CopyAllowLine",                    vm => vm.CopyAllowLineCommand,                    "Copy selection or current line if no text is selected");
			AddCommand(ASS.NoteEdit,   "CutAllowLine",                     vm => vm.CutAllowLineCommand,                     "Cut selection or current line if no text is selected");

			AddCommand(ASS.Window,     "CheckForUpdates",                  vm => vm.ManuallyCheckForUpdatesCommand,          "Manually check for new updates");

			AddCommand(ASS.Window,     "ToggleAlwaysOnTop",                vm => vm.SettingAlwaysOnTopCommand,               "Toggle the option 'Always on top'");
			AddCommand(ASS.Window,     "ToggleLineNumbers",                vm => vm.SettingLineNumbersCommand,               "Toggle the option 'Show line numbers'"); 
			AddCommand(ASS.Window,     "ToggleWordWrap",                   vm => vm.SettingsWordWrapCommand,                 "Toggle the option 'Word Wrap'");
			AddCommand(ASS.Window,     "RotateTheme",                      vm => vm.SettingsRotateThemeCommand,              "Change the current theme to the next available");
			AddCommand(ASS.Window,     "ToggleReadonly",                   vm => vm.SettingReadonlyModeCommand,              "Toggle the option 'Readonly mode'");
			
			AddCommand(ASS.Window,     "SetPreviewStyleSimple",            vm => vm.SetPreviewStyleSimpleCommand,            "Set the note preview style to 'Simple one line'");
			AddCommand(ASS.Window,     "SetPreviewStyleExtended",          vm => vm.SetPreviewStyleExtendedCommand,          "Set the note preview style to 'One line with date'");
			AddCommand(ASS.Window,     "SetPreviewStyleSingleLinePreview", vm => vm.SetPreviewStyleSingleLinePreviewCommand, "Set the note preview style to 'Title and first line'");
			AddCommand(ASS.Window,     "SetPreviewStyleFullPreview",       vm => vm.SetPreviewStyleFullPreviewCommand,       "Set the note preview style to 'Multiple lines with preview'");
			
			AddCommand(ASS.Window,     "SetNoteSortingNone",               vm => vm.SetNoteSortingNoneCommand,               "Set the note sorting to 'None'");
			AddCommand(ASS.Window,     "SetNoteSortingByName",             vm => vm.SetNoteSortingByNameCommand,             "Set the note sorting to 'Title'");
			AddCommand(ASS.Window,     "SetNoteSortingByCreationDate",     vm => vm.SetNoteSortingByCreationDateCommand,     "Set the note sorting to 'Creation date'");
			AddCommand(ASS.Window,     "SetNoteSortingByModificationDate", vm => vm.SetNoteSortingByModificationDateCommand, "Set the note sorting to 'Last modified date'");
			
			AddCommand(ASS.Window,     "RotateNoteProvider",               vm => vm.RotateNoteProviderCommand,               "Switch to the next note provider in the list");
		}

		public static void Execute(MainWindow mw, string key)
		{
			App.Logger.Trace("ShortcutManager", $"Execute Action [{key}]");

			if (_actions.TryGetValue(key, out var action))
			{
				action.Run(mw);
			}
		}

		public static string GetGestureStr(MainWindow mw, string key)
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

		private static void AddCommand(AlephShortcutScope scope, string k, Func<MainWindowViewmodel, ICommand> a, string desc, ActionModifier mod = ActionModifier.None)
		{
			bool Exec(AlephAction src, MainWindow w)
			{
				var vm = w.VM;
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
			bool Exec(AlephAction src, MainWindow w)
			{
				var vm = w.VM;
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
	}
}
