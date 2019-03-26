using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AlephNote.WPF.Windows;
using MSHC.WPF.MVVM;

namespace AlephNote.WPF.Controls.NotesView
{
	public static class NotesViewControlCommon
	{
		public static ContextMenu GetContextMenuNote(FrameworkElement anchor)
		{
			var pin = MainWindow.Instance?.VM?.Repository?.SupportsPinning ?? false;
			var lck = MainWindow.Instance?.VM?.Repository?.SupportsLocking ?? false;

			var cms = new ContextMenu();

			         cms.Items.Add(new AutoActionMenuItem{ Header="Export",        AlephAction="ExportNote",      ParentAnchor=anchor});
			         cms.Items.Add(new AutoActionMenuItem{ Header="Duplicate",     AlephAction="DuplicateNote",   ParentAnchor=anchor});
			if (pin) cms.Items.Add(new AutoActionMenuItem{ Header="Pin / Unpin",   AlephAction="PinUnpinNote",    ParentAnchor=anchor});
			if (lck) cms.Items.Add(new AutoActionMenuItem{ Header="Lock / Unlock", AlephAction="LockUnlockNote",  ParentAnchor=anchor});
			         cms.Items.Add(new Separator());
			         cms.Items.Add(CreateAddTagItem(anchor));
			         cms.Items.Add(CreateRemTagItem(anchor));
			         cms.Items.Add(new Separator());
			         cms.Items.Add(new AutoActionMenuItem{ Header="Delete",        AlephAction="DeleteNote",      ParentAnchor=anchor});

			return cms;
		}

		public static ContextMenu GetContextMenuEmpty(FrameworkElement anchor)
		{
			return new ContextMenu
			{
				Items =
				{
					new AutoActionMenuItem{ Header="New Note",                  AlephAction="NewNote",              ParentAnchor=anchor },
					new AutoActionMenuItem{ Header="New Note (from clipboard)", AlephAction="NewNoteFromClipboard", ParentAnchor=anchor },
					new AutoActionMenuItem{ Header="New Note (from text file)", AlephAction="NewNoteFromTextFile",  ParentAnchor=anchor },
				}
			};
		}

		private static MenuItem CreateAddTagItem(FrameworkElement anchor)
		{
			var mw = GetParent(anchor) ?? MainWindow.Instance;

			var parent = new MenuItem { Header = "Add tag" };
			if (mw == null) return parent;
			foreach (var tag in mw.VM.Repository.EnumerateAllTags().Distinct().OrderBy(p => p.ToLower()))
			{
				parent.Items.Add(new MenuItem
				{
					Header = tag,
					Command = mw.VM.AddTagCommand,
					CommandParameter = tag,
				});
			}
			return parent;
		}

		private static MenuItem CreateRemTagItem(FrameworkElement anchor)
		{
			var mw = GetParent(anchor) ?? MainWindow.Instance;

			var parent = new MenuItem { Header = "Remove tag" };
			if (mw == null) return parent;
			foreach (var tag in mw.VM.Repository.EnumerateAllTags().Distinct().OrderBy(p => p.ToLower()))
			{
				parent.Items.Add(new MenuItem
				{
					Header = tag,
					Command = mw.VM.RemoveTagCommand,
					CommandParameter = tag,
				});
			}
			return parent;
		}

		private static MainWindow GetParent(FrameworkElement o)
		{
			if (o == null) return null;

			if (o is MainWindow mw) return mw;

			if (o.Parent is FrameworkElement fe) return GetParent(fe);

			return null;
		}
	}
}
