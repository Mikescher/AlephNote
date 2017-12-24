using AlephNote.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using AlephNote.Common.MVVM;
using AlephNote.Common.Settings;
using AlephNote.Common.Settings.Types;

namespace AlephNote.WPF.Shortcuts
{
	public static class ShortcutManager
	{
		private enum ActionType { Internal=1, FromSnippet=2 }

		private class AlephAction
		{
			public readonly ActionType AType;
			public readonly Action<MainWindow> Run;
			public readonly string Description;

			public AlephAction(ActionType t, Action<MainWindow> e, string d) { AType = t; Run = e; Description = d; }
		}

		private static Dictionary<string, AlephAction> _actions = new Dictionary<string, AlephAction>();

		static ShortcutManager()
		{
			AddCommand("NewNote",              vm => vm.CreateNewNoteCommand,              "Create new note");
			AddCommand("NewNoteFromClipboard", vm => vm.CreateNewNoteFromClipboardCommand, "Create new note from current clipboard content");
			AddCommand("ExportNote",           vm => vm.ExportCommand,                     "Export the current note");
			AddCommand("DeleteNote",           vm => vm.DeleteCommand,                     "Delete the current note");
			AddCommand("DeleteFolder",         vm => vm.DeleteFolderCommand,               "Delete the current selected folder");
			AddCommand("AddSubFolder",         vm => vm.AddSubFolderCommand,               "Add a new sub folder under the currently selected folder");
			AddCommand("AddFolder",            vm => vm.AddFolderCommand,                  "Add a new folder");
			AddCommand("RenameFolder",         vm => vm.RenameFolderCommand,               "Rename currently selected folder");

			AddCommand("SaveAndSync",          vm => vm.SaveAndSyncCommand,                "Save current note and synchronize");
			AddCommand("Resync",               vm => vm.ResyncCommand,                     "Start synchronization with remote");
			AddCommand("FullResync",           vm => vm.FullResyncCommand,                 "Delete local data and do a full resync");

			AddCommand("DocumentSearch",       vm => vm.DocumentSearchCommand,             "Search in current note");
			AddCommand("CloseDocumentSearch",  vm => vm.CloseDocumentSearchCommand,        "Close inline search panel");

			AddCommand("ShowSettings",         vm => vm.SettingsCommand,                   "Show settings window");
			AddCommand("ShowMainWindow",       vm => vm.ShowMainWindowCommand,             "Bring the window into the foreground");
			AddCommand("ShowAbout",            vm => vm.ShowAboutCommand,                  "Show the about window");
			AddCommand("ShowLog",              vm => vm.ShowLogCommand,                    "Show the log window");
			AddCommand("AppExit",              vm => vm.ExitCommand,                       "Close the application");

			AddCommand("CheckForUpdates",      vm => vm.ManuallyCheckForUpdatesCommand,    "Manually check for new updates");

			AddCommand("ToggleAlwaysOnTop",    vm => vm.SettingAlwaysOnTopCommand,         "Change the option 'Always on top'");
			AddCommand("ToggleLineNumbers",    vm => vm.SettingLineNumbersCommand,         "Change the option 'Show line numbers'");
			AddCommand("ToggleWordWrap",       vm => vm.SettingsWordWrapCommand,           "Change the option 'Word Wrap'");
		}

		public static void Execute(MainWindow mw, string key)
		{
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

		public static void AddAction(string k, Action<MainWindow> a, string desc)
		{
			_actions.Add(k, new AlephAction(ActionType.Internal, a, desc));
		}

		public static void AddCommand(string k, Func<MainWindowViewmodel, ICommand> a, string desc)
		{
			Action<MainWindow> ac = (w) =>
			{
				var vm = w.VM;
				if (vm != null)
				{
					var c = a(vm);
					if (c.CanExecute(null)) c.Execute(null);
				}
			};

			_actions.Add(k, new AlephAction(ActionType.Internal, ac, desc));
		}

		public static bool Contains(string snipkey)
		{
			return _actions.ContainsKey(snipkey);
		}

		public static void AddSnippetCommand(string snippetactionkey, string snippetvalue, string displayname)
		{
			Action<MainWindow> ac = (w) =>
			{
				var vm = w.VM;
				if (vm != null)
				{
					var c = vm.InsertSnippetCommand;
					if (c.CanExecute(snippetvalue)) c.Execute(snippetvalue);
				}
			};

			_actions.Add(snippetactionkey, new AlephAction(ActionType.FromSnippet, ac, $"Inserts the snippet '{displayname}'"));
		}

		public static ObservableCollectionNoReset<ObservableShortcutConfig> ListObservableShortcuts(AppSettings settings)
		{
			var result = new ObservableCollectionNoReset<ObservableShortcutConfig>();

			foreach (var a in _actions)
			{
				if (settings.Shortcuts.TryGetValue(a.Key, out var def))
				{
					result.Add(new ObservableShortcutConfig((int) a.Value.AType, a.Key, a.Value.Description, def.Key, def.Modifiers, def.Scope));
				}
				else
				{
					result.Add(new ObservableShortcutConfig((int)a.Value.AType, a.Key, a.Value.Description, AlephKey.None, AlephModifierKeys.None, AlephShortcutScope.Window));
				}
			}

			return result;

		}
	}
}
