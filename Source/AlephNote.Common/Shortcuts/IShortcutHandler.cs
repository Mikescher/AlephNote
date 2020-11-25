using System.Windows.Input;

namespace AlephNote.Common.Shortcuts
{
	public interface IShortcutHandler
	{
		ICommand CreateNewNoteCommand { get; }
		ICommand CreateNewNoteFromClipboardCommand { get; }
		ICommand CreateNewNoteFromTextfileCommand { get; }
		ICommand ExportCommand { get; }
		ICommand DeleteCommand { get; }
		ICommand DeleteFolderCommand { get; }
		ICommand AddSubFolderCommand { get; }
		ICommand AddFolderCommand { get; }
		ICommand RenameFolderCommand { get; }
		ICommand ChangePathCommand { get; }
		ICommand DuplicateNoteCommand { get; }
		ICommand PinUnpinNoteCommand { get; }
		ICommand LockUnlockNoteCommand { get; }
		ICommand InsertHyperlinkCommand { get; }
		ICommand InsertFilelinkCommand { get; }
		ICommand InsertNotelinkCommand { get; }
		ICommand InsertMaillinkCommand { get; }
		ICommand MoveCurrentLineUpCommand { get; }
		ICommand MoveCurrentLineDownCommand { get; }
		ICommand DuplicateCurrentLineCommand { get; }
		ICommand CopyCurrentLineCommand { get; }
		ICommand CutCurrentLineCommand { get; }
		ICommand SaveAndSyncCommand { get; }
		ICommand ResyncCommand { get; }
		ICommand FullResyncCommand { get; }
		ICommand FullUploadCommand { get; }
		ICommand DocumentSearchCommand { get; }
		ICommand DocumentContinueSearchCommand { get; }
		ICommand CloseDocumentSearchCommand { get; }
		ICommand SettingsCommand { get; }
		ICommand ShowMainWindowCommand { get; }
		ICommand ShowAboutCommand { get; }
		ICommand ShowLogCommand { get; }
		ICommand ExitCommand { get; }
		ICommand RestartCommand { get; }
		ICommand HideCommand { get; }
		ICommand AppToggleVisibilityCommand { get; }
		ICommand FocusScintillaCommand { get; }
		ICommand FocusNotesListCommand { get; }
		ICommand FocusGlobalSearchCommand { get; }
		ICommand FocusFolderCommand { get; }
		ICommand FocusPrevNoteCommand { get; }
		ICommand FocusNextNoteCommand { get; }
		ICommand FocusPrevDirectoryAnyCommand { get; }
		ICommand FocusNextDirectoryAnyCommand { get; }
		ICommand FocusPrevDirectoryCommand { get; }
		ICommand FocusNextDirectoryCommand { get; }
		ICommand FocusDirectoryLevelDownCommand { get; }
		ICommand FocusDirectoryLevelUpCommand { get; }
		ICommand CopyAllowLineCommand { get; }
		ICommand CutAllowLineCommand { get; }
		ICommand ManuallyCheckForUpdatesCommand { get; }
		ICommand SettingAlwaysOnTopCommand { get; }
		ICommand SettingLineNumbersCommand { get; }
		ICommand SettingsWordWrapCommand { get; }
		ICommand SettingsRotateThemeCommand { get; }
		ICommand SettingReadonlyModeCommand { get; }
		ICommand SetPreviewStyleSimpleCommand { get; }
		ICommand SetPreviewStyleExtendedCommand { get; }
		ICommand SetPreviewStyleSingleLinePreviewCommand { get; }
		ICommand SetPreviewStyleFullPreviewCommand { get; }
		ICommand SetNoteSortingNoneCommand { get; }
		ICommand SetNoteSortingByNameCommand { get; }
		ICommand SetNoteSortingByCreationDateCommand { get; }
		ICommand SetNoteSortingByModificationDateCommand { get; }
		ICommand RotateNoteProviderCommand { get; }
		ICommand InsertSnippetCommand { get; }
		ICommand FocusTitleCommand { get; }
		ICommand FocusTagsCommand { get; }
		ICommand MoveFolderUpCommand { get; }
		ICommand MoveFolderDownCommand { get; }
	}
}
