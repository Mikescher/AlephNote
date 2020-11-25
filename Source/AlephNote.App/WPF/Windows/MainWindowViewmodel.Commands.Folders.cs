using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using AlephNote.WPF.Dialogs;
using MSHC.WPF.MVVM;
using AlephNote.WPF.Util;
using AlephNote.Common.Hierarchy;

namespace AlephNote.WPF.Windows
{
	public partial class MainWindowViewmodel
	{
		public ICommand DeleteFolderCommand   => new RelayCommand(DeleteFolder);
		public ICommand AddFolderCommand      => new RelayCommand(AddRootFolder);
		public ICommand AddSubFolderCommand   => new RelayCommand(AddSubFolder);
		public ICommand RenameFolderCommand   => new RelayCommand(RenameFolder);
		public ICommand MoveFolderUpCommand   => new RelayCommand(MoveFolderUp);
		public ICommand MoveFolderDownCommand => new RelayCommand(MoveFolderDown);

		private void DeleteFolder()
		{
			try
			{
				var fp = SelectedFolderPath;

				if (fp == null || HierarchicalBaseWrapper.IsSpecial(fp)) return;

				var delNotes = Repository.Notes.Where(n => n.Path.IsPathOrSubPath(fp)).ToList();

				if (MessageBox.Show(Owner, $"Do you really want to delete this folder together with {delNotes.Count} contained notes?", "Delete folder?", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

				foreach (var note in delNotes)
				{
					Repository.DeleteNote(note, true);
				}

				Owner.NotesViewControl.DeleteFolder(fp);

				SelectedNote = Owner.NotesViewControl.EnumerateVisibleNotes().FirstOrDefault() ?? Repository.Notes.FirstOrDefault();
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Could not delete folder", e);
				ExceptionDialog.Show(Owner, "Could not delete folder", e, string.Empty);
			}
		}

		private void AddSubFolder()
		{
			if (!GenericInputDialog.ShowInputDialog(Owner, "Insert the name for the new subfolder", "Folder name", "", out var foldercomponent)) return;
			if (string.IsNullOrWhiteSpace(foldercomponent)) return;

			var currentPath = SelectedFolderPath;
			if (currentPath == null || HierarchicalBaseWrapper.IsSpecial(currentPath)) currentPath = DirectoryPath.Root();

			var path = currentPath.SubDir(foldercomponent);

			Owner.NotesViewControl.AddFolder(path);
		}

		private void AddRootFolder()
		{
			if (!GenericInputDialog.ShowInputDialog(Owner, "Insert the name for the new folder", "Folder name", "", out var foldercomponent)) return;
			if (string.IsNullOrWhiteSpace(foldercomponent)) return;

			var path = DirectoryPath.Create(Enumerable.Repeat(foldercomponent, 1));

			Owner.NotesViewControl.AddFolder(path);
		}

		private void RenameFolder()
		{
			try
			{
				var oldPath = SelectedFolderPath;
				if (HierarchicalBaseWrapper.IsSpecial(oldPath)) return;

				if (!GenericInputDialog.ShowInputDialog(Owner, "Insert the name for the folder", "Folder name", oldPath.GetLastComponent(), out var newFolderName)) return;

				var newPath = oldPath.ParentPath().SubDir(newFolderName);

				if (newPath.EqualsWithCase(oldPath)) return;

				Owner.NotesViewControl.AddFolder(newPath);

				foreach (INote n in Repository.Notes.ToList())
				{
					if (n.Path.IsPathOrSubPath(oldPath))
					{
						n.Path = n.Path.Replace(oldPath, newPath);
					}
				}

				Owner.NotesViewControl.AddFolder(newPath);
			}
			catch (Exception e)
			{
				App.Logger.Error("Main", "Could not rename folder", e);
				ExceptionDialog.Show(Owner, "Could not rename folder", e, string.Empty);
			}
		}

		private void MoveFolderUp()
		{
			if (Settings.SortHierarchyFoldersByName) return;

			var path = SelectedFolderPath;
			if (HierarchicalBaseWrapper.IsSpecial(path)) return;

			Owner.NotesViewControl.MoveFolder(path, -1);
		}

		private void MoveFolderDown()
		{
			if (Settings.SortHierarchyFoldersByName) return;

			var path = SelectedFolderPath;
			if (HierarchicalBaseWrapper.IsSpecial(path)) return;

			Owner.NotesViewControl.MoveFolder(path, +1);
		}
	}
}
