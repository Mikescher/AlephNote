using AlephNote.Common.Repository;
using AlephNote.Common.Util;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using MSHC.Lang.Collections;

namespace AlephNote.WPF.Windows
{
	/// <summary>
	/// Interaction logic for ConflictWindow.xaml
	/// </summary>
	public partial class ConflictWindow : Window
	{
		private readonly NoteRepository _repo;
		private readonly string _noteID;
		private readonly Tuple<string, string, List<string>, DirectoryPath> _dataLeft;
		private readonly Tuple<string, string, List<string>, DirectoryPath> _dataRight;

		private ConflictWindow(NoteRepository repo, string noteid, Tuple<string, string, List<string>, DirectoryPath> left, Tuple<string, string, List<string>, DirectoryPath> right)
		{
			InitializeComponent();

			_repo      = repo;
			_noteID    = noteid;
			_dataLeft  = left;
			_dataRight = right;
		}

		public static void Show(NoteRepository repo, Window owner, string noteid, Tuple<string, string, List<string>, DirectoryPath> left, Tuple<string, string, List<string>, DirectoryPath> right)
		{
			var vm = new ConflictWindowViewmodel();
			
			vm.Text1 = left.Item1;
			vm.Text2 = right.Item1;

			vm.Title1 = left.Item2;
			vm.Title2 = right.Item2;

			vm.Tags1 = left.Item3;
			vm.Tags2 = right.Item3;

			vm.Path1 = left.Item4.StrSerialize();
			vm.Path2 = right.Item4.StrSerialize();

			var win = new ConflictWindow(repo, noteid, left, right) { DataContext = vm };

			win.Show();
		}

		private void Button_TakeLeft_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var note = _repo.FindNoteByID(_noteID);
				if (note == null)
				{
					LoggerSingleton.Inst.Warn("ConflictWindow", "Note not found", $"Could not update note {_noteID}, because it no longer exists in the repository");
					Close();
					return;
				}

				if (note.Text != _dataLeft.Item1) note.Text = _dataLeft.Item1;
				if (note.Title != _dataLeft.Item2) note.Title = _dataLeft.Item2;
				if (!note.Tags.UnorderedCollectionEquals(_dataLeft.Item3)) note.Tags.Synchronize(_dataLeft.Item3);
				if (note.Path != _dataLeft.Item4) note.Path = _dataLeft.Item4;

				note.SetRemoteDirty("Data updated in ConflictWindow [TakeLeft]");
				_repo.SyncNow();

				Close();
			}
			catch (Exception ex)
			{
				LoggerSingleton.Inst.Error("ConflictWindow", "Error in conflict resolution", ex);
			}
		}

		private void Button_TakeRight_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var note = _repo.FindNoteByID(_noteID);
				if (note == null)
				{
					LoggerSingleton.Inst.Warn("ConflictWindow", "Note not found", $"Could not update note {_noteID}, because it no longer exists in the repository");
					Close();
					return;
				}

				if (note.Text != _dataRight.Item1) note.Text = _dataRight.Item1;
				if (note.Title != _dataRight.Item2) note.Title = _dataRight.Item2;
				if (!note.Tags.UnorderedCollectionEquals(_dataRight.Item3)) note.Tags.Synchronize(_dataRight.Item3);
				if (note.Path != _dataRight.Item4) note.Path = _dataRight.Item4;

				note.SetRemoteDirty("Data updated in ConflictWindow [Take Right]");
				_repo.SyncNow();

				Close();
			}
			catch (Exception ex)
			{
				LoggerSingleton.Inst.Error("ConflictWindow", "Error in conflict resolution", ex);
			}
		}

		private void Button_TakeLeftWithConflict_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var note = _repo.FindNoteByID(_noteID);
				if (note == null)
				{
					LoggerSingleton.Inst.Warn("ConflictWindow", "Note not found", $"Could not update note {_noteID}, because it no longer exists in the repository");
					Close();
					return;
				}

				if (note.Text != _dataLeft.Item1) note.Text = _dataLeft.Item1;
				if (note.Title != _dataLeft.Item2) note.Title = _dataLeft.Item2;
				if (!note.Tags.UnorderedCollectionEquals(_dataLeft.Item3)) note.Tags.Synchronize(_dataLeft.Item3);
				if (note.Path != _dataLeft.Item4) note.Path = _dataLeft.Item4;

				var conflict = _repo.CreateNewNote(_dataRight.Item4);
				conflict.Title = string.Format("{0}_conflict_manual-{1:yyyy-MM-dd_HH:mm:ss}", _dataRight.Item2, DateTime.Now);
				conflict.Text = _dataRight.Item1;
				conflict.Tags.Synchronize(_dataRight.Item3);
				conflict.IsConflictNote = true;
				_repo.SaveNote(conflict);

				note.SetRemoteDirty("Data updated in ConflictWindow [Take Left With Conflict]");
				_repo.SyncNow();

				Close();
			}
			catch (Exception ex)
			{
				LoggerSingleton.Inst.Error("ConflictWindow", "Error in conflict resolution", ex);
			}
		}

		private void Button_TakeRightWithConflict_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var note = _repo.FindNoteByID(_noteID);
				if (note == null)
				{
					LoggerSingleton.Inst.Warn("ConflictWindow", "Note not found", $"Could not update note {_noteID}, because it no longer exists in the repository");
					Close();
					return;
				}

				if (note.Text != _dataRight.Item1) note.Text = _dataRight.Item1;
				if (note.Title != _dataRight.Item2) note.Title = _dataRight.Item2;
				if (!note.Tags.UnorderedCollectionEquals(_dataRight.Item3)) note.Tags.Synchronize(_dataRight.Item3);
				if (note.Path != _dataRight.Item4) note.Path = _dataRight.Item4;

				var conflict = _repo.CreateNewNote(_dataLeft.Item4);
				conflict.Title = string.Format("{0}_conflict_manual-{1:yyyy-MM-dd_HH:mm:ss}", _dataLeft.Item2, DateTime.Now);
				conflict.Text = _dataLeft.Item1;
				conflict.Tags.Synchronize(_dataLeft.Item3);
				conflict.IsConflictNote = true;
				_repo.SaveNote(conflict);

				note.SetRemoteDirty("Data updated in ConflictWindow [Take Right With Conflict]");
				_repo.SyncNow();

				Close();
			}
			catch (Exception ex)
			{
				LoggerSingleton.Inst.Error("ConflictWindow", "Error in conflict resolution", ex);
			}
		}
	}
}
