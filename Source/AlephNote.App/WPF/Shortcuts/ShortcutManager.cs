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
	public static class ShortcutManager
	{
		[Flags]
		public enum ActionModifier { None=0, Disabled=1, AccessControl=2, DebugOnly=4, FromSnippet=8}

		private class AlephAction
		{
			private readonly Func<AlephAction, MainWindow, bool> _run;
			public readonly string Description;
			public readonly ActionModifier Modifier;

			public AlephAction(Func<AlephAction, MainWindow, bool> e, string d, ActionModifier m) { _run = e; Description = d; Modifier = m; }

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
			AddCommand("NewNote",                          vm => vm.CreateNewNoteCommand,                    "Create a new note",                                        ActionModifier.AccessControl); 
			AddCommand("NewNoteFromClipboard",             vm => vm.CreateNewNoteFromClipboardCommand,       "Create a new note from current clipboard content",         ActionModifier.AccessControl);
			AddCommand("NewNoteFromTextFile",              vm => vm.CreateNewNoteFromTextfileCommand,        "Create a new note from a file",                            ActionModifier.AccessControl);
			AddCommand("ExportNote",                       vm => vm.ExportCommand,                           "Export the current note");
			AddCommand("DeleteNote",                       vm => vm.DeleteCommand,                           "Delete the current note",                                  ActionModifier.AccessControl);
			AddCommand("DeleteFolder",                     vm => vm.DeleteFolderCommand,                     "Delete the current selected folder",                       ActionModifier.AccessControl);
			AddCommand("AddSubFolder",                     vm => vm.AddSubFolderCommand,                     "Add a new sub folder under the currently selected folder", ActionModifier.AccessControl);
			AddCommand("AddFolder",                        vm => vm.AddFolderCommand,                        "Add a new folder",                                         ActionModifier.AccessControl);
			AddCommand("RenameFolder",                     vm => vm.RenameFolderCommand,                     "Rename currently selected folder",                         ActionModifier.AccessControl);
			AddCommand("ChangeNotePath",                   vm => vm.ChangePathCommand,                       "Change the path of the currently selected note",           ActionModifier.AccessControl);
			AddCommand("DuplicateNote",                    vm => vm.DuplicateNoteCommand,                    "Create a new note as a copy of the current note",          ActionModifier.AccessControl);
			AddCommand("PinUnpinNote",                     vm => vm.PinUnpinNoteCommand,                     "Pin the note to the top (or un-pin the note)",             ActionModifier.AccessControl);
			AddCommand("LockUnlockNote",                   vm => vm.LockUnlockNoteCommand,                   "Lock/Unlock the note (prevent editing)",                   ActionModifier.AccessControl);

			AddCommand("InsertHyperlink",                  vm => vm.InsertHyperlinkCommand,                  "Insert a Hyperlink",                                       ActionModifier.AccessControl);
			AddCommand("InsertFilelink",                   vm => vm.InsertFilelinkCommand,                   "Insert a link to a local file",                            ActionModifier.AccessControl);
			AddCommand("InsertNotelink",                   vm => vm.InsertNotelinkCommand,                   "Insert a link to another note",                            ActionModifier.AccessControl);
			AddCommand("InsertMaillink",                   vm => vm.InsertMaillinkCommand,                   "Insert a clickable mail address",                          ActionModifier.AccessControl);

			AddCommand("SaveAndSync",                      vm => vm.SaveAndSyncCommand,                      "Save current note and synchronize");
			AddCommand("Resync",                           vm => vm.ResyncCommand,                           "Start synchronization with remote");
			AddCommand("FullResync",                       vm => vm.FullResyncCommand,                       "Delete local data and do a full resync",                   ActionModifier.AccessControl);

			AddCommand("DocumentSearch",                   vm => vm.DocumentSearchCommand,                   "Search in current note");
			AddCommand("DocumentSearchNext",               vm => vm.DocumentContinueSearchCommand,           "Find next occurence of search");
			AddCommand("CloseDocumentSearch",              vm => vm.CloseDocumentSearchCommand,              "Close inline search panel");

			AddCommand("ShowSettings",                     vm => vm.SettingsCommand,                         "Show settings window");
			AddCommand("ShowMainWindow",                   vm => vm.ShowMainWindowCommand,                   "Bring the window into the foreground");
			AddCommand("ShowAbout",                        vm => vm.ShowAboutCommand,                        "Show the about window");
			AddCommand("ShowLog",                          vm => vm.ShowLogCommand,                          "Show the log window");
			AddCommand("AppExit",                          vm => vm.ExitCommand,                             "Close the application");
			AddCommand("AppRestart",                       vm => vm.RestartCommand,                          "Restart the application");
			AddCommand("AppHide",                          vm => vm.HideCommand,                             "Hide main window");
			AddCommand("FocusEditor",                      vm => vm.FocusScintillaCommand,                   "Select the note editor");
			AddCommand("FocusNotesList",                   vm => vm.FocusNotesListCommand,                   "Select the note list");
			AddCommand("FocusGlobalSearch",                vm => vm.FocusGlobalSearchCommand,                "Select the global search field");
			AddCommand("FocusFolderList",                  vm => vm.FocusFolderCommand,                      "Select the folder list");

			AddCommand("FocusPrevNote",                    vm => vm.FocusPrevNoteCommand,                    "Select the previous note");
			AddCommand("FocusNextNote",                    vm => vm.FocusNextNoteCommand,                    "Select the next note");
			AddCommand("FocusPrevDirectoryAny",            vm => vm.FocusPrevDirectoryAnyCommand,            "Select the previous directory");
			AddCommand("FocusNextDirectoryAny",            vm => vm.FocusNextDirectoryAnyCommand,            "Select the next directory");
			AddCommand("FocusPrevDirectory",               vm => vm.FocusPrevDirectoryCommand,               "Select the previous directory (on the same level)");
			AddCommand("FocusNextDirectory",               vm => vm.FocusNextDirectoryCommand,               "Select the next directory (on the same level)");
			AddCommand("FocusDirectoryLevelDown",          vm => vm.FocusDirectoryLevelDownCommand,          "Go one directory level down");
			AddCommand("FocusDirectoryLevelUp",            vm => vm.FocusDirectoryLevelUpCommand,            "Go one directory level up");

			AddCommand("CheckForUpdates",                  vm => vm.ManuallyCheckForUpdatesCommand,          "Manually check for new updates");

			AddCommand("ToggleAlwaysOnTop",                vm => vm.SettingAlwaysOnTopCommand,               "Toggle the option 'Always on top'");
			AddCommand("ToggleLineNumbers",                vm => vm.SettingLineNumbersCommand,               "Toggle the option 'Show line numbers'"); 
			AddCommand("ToggleWordWrap",                   vm => vm.SettingsWordWrapCommand,                 "Toggle the option 'Word Wrap'");
			AddCommand("RotateTheme",                      vm => vm.SettingsRotateThemeCommand,              "Change the current theme to the next available");
			AddCommand("ToggleReadonly",                   vm => vm.SettingReadonlyModeCommand,              "Toggle the option 'Readonly mode'");
			
			AddCommand("SetPreviewStyleSimple",            vm => vm.SetPreviewStyleSimpleCommand,            "Set the note preview style to 'Simple one line'");
			AddCommand("SetPreviewStyleExtended",          vm => vm.SetPreviewStyleExtendedCommand,          "Set the note preview style to 'One line with date'");
			AddCommand("SetPreviewStyleSingleLinePreview", vm => vm.SetPreviewStyleSingleLinePreviewCommand, "Set the note preview style to 'Title and first line'");
			AddCommand("SetPreviewStyleFullPreview",       vm => vm.SetPreviewStyleFullPreviewCommand,       "Set the note preview style to 'Multiple lines with preview'");
			
			AddCommand("SetNoteSortingNone",               vm => vm.SetNoteSortingNoneCommand,               "Set the note sorting to 'None'");
			AddCommand("SetNoteSortingByName",             vm => vm.SetNoteSortingByNameCommand,             "Set the note sorting to 'Title'");
			AddCommand("SetNoteSortingByCreationDate",     vm => vm.SetNoteSortingByCreationDateCommand,     "Set the note sorting to 'Creation date'");
			AddCommand("SetNoteSortingByModificationDate", vm => vm.SetNoteSortingByModificationDateCommand, "Set the note sorting to 'Last modified date'");
			
			AddCommand("RotateNoteProvider",               vm => vm.RotateNoteProviderCommand,               "Switch to the next note provider in the list");
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

		public static void AddCommand(string k, Func<MainWindowViewmodel, ICommand> a, string desc, ActionModifier mod = ActionModifier.None)
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

			_actions.Add(k, new AlephAction(Exec, desc, mod));
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

			_actions.Add(snippetactionkey, new AlephAction(Exec, $"Inserts the snippet '{displayname}'", ActionModifier.FromSnippet | ActionModifier.AccessControl));
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
					result.Add(new ObservableShortcutConfig(a.Key, a.Value.Description, AlephKey.None, AlephModifierKeys.None, AlephShortcutScope.Window));
				}
			}

			return result;

		}
	}
}
