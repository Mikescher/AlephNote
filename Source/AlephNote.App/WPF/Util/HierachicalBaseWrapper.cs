using AlephNote.Common.MVVM;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace AlephNote.WPF.Util
{
	public abstract class HierachicalBaseWrapper : ObservableObject
	{
		public abstract string Header { get; }

		private bool _isSelected = false;
		public bool IsSelected { get { return _isSelected; } set { _isSelected = value; OnPropertyChanged(); } }

		private bool _isExpanded = true;
		public bool IsExpanded { get { return _isExpanded; } set { _isExpanded = value; OnPropertyChanged(); } }

		public abstract void Sync(HierachicalBaseWrapper aother, HierachicalFolderWrapper[] parents);
	}

	public class HierachicalRootWrapper : HierachicalFolderWrapper
	{
		private readonly HierachicalFolderWrapper _baseWrapper;

		public override Visibility IsAllVisible    => Visibility.Visible;
		public override Visibility IsNotAllVisible => Visibility.Collapsed;

		public override void Sync(HierachicalBaseWrapper aother, HierachicalFolderWrapper[] parents)
		{
			Debug.Assert(aother is HierachicalRootWrapper);
		}

		public override IEnumerable<INote> AllSubNotes => _baseWrapper.AllSubNotes;

		public HierachicalRootWrapper(HierachicalFolderWrapper baseWrapper) : base("All notes", DirectoryPath.Root(), false)
		{
			_baseWrapper = baseWrapper;
			IsSelected = true;
		}
	}

	public class HierachicalFolderWrapper : HierachicalBaseWrapper
	{
		private readonly bool _isRoot;
		private string _header;
		public override string Header => _header;

		private DirectoryPath _path;

		public virtual Visibility IsAllVisible    => Visibility.Collapsed;
		public virtual Visibility IsNotAllVisible => Visibility.Visible;

		public readonly HierachicalRootWrapper AllNotesWrapper;

		private ObservableCollection<HierachicalFolderWrapper> _subFolders = new ObservableCollection<HierachicalFolderWrapper>();
		public ObservableCollection<HierachicalFolderWrapper> SubFolder { get { return _subFolders; } }

		public List<INote> DirectSubNotes = new List<INote>();

		public virtual IEnumerable<INote> AllSubNotes => DirectSubNotes.Concat(SubFolder.Where(sf => !(sf is HierachicalRootWrapper)).SelectMany(sf => sf.AllSubNotes));

		public HierachicalFolderWrapper(string header, DirectoryPath path, bool root)
		{
			_isRoot = root;
			_path = path;
			_header = header;

			if (_isRoot) SubFolder.Add(AllNotesWrapper = new HierachicalRootWrapper(this));
		}

		public HierachicalFolderWrapper GetOrCreateFolder(string txt)
		{
			foreach (var item in SubFolder)
			{
				if (item.Header.ToLower() == txt.ToLower()) return item;
			}
			var i = new HierachicalFolderWrapper(txt, _path.SubDir(txt), false);
			SubFolder.Add(i);
			return i;
		}

		protected void TriggerAllSubNotesChanged()
		{
			OnPropertyChanged("DirectSubNotes");
			OnPropertyChanged("AllSubNotes");
		}

		public override void Sync(HierachicalBaseWrapper aother, HierachicalFolderWrapper[] parents)
		{
			var other = (HierachicalFolderWrapper)aother;

			Debug.Assert(this.Header.ToLower() == other.Header.ToLower());
			Debug.Assert(this._isRoot == other._isRoot);

			_path = other._path;

			DirectSubNotes.Synchronize(other.DirectSubNotes, out var changed);
			if (changed)
			{
				TriggerAllSubNotesChanged();
				foreach (var p in parents)
				{
					p.TriggerAllSubNotesChanged();
					if (p is HierachicalRootWrapper r) r.AllNotesWrapper.TriggerAllSubNotesChanged();
				}
			}

			bool FCompare(HierachicalFolderWrapper a, HierachicalFolderWrapper b) => a.Header.ToLower() == b.Header.ToLower();
			HierachicalFolderWrapper FCopy(HierachicalFolderWrapper a) => new HierachicalFolderWrapper(a.Header, a._path, a._isRoot);
			SubFolder.SynchronizeCollection(other.SubFolder, FCompare, FCopy);
			Debug.Assert(SubFolder.Count == other.SubFolder.Count);

			for (int i = 0; i < SubFolder.Count; i++) SubFolder[i].Sync(other.SubFolder[i], parents.Concat(new []{this}).ToArray());
		}

		public void Add(HierachicalFolderWrapper elem) { SubFolder.Add(elem); }
		public void Add(INote elem) { DirectSubNotes.Add(elem); }

		public void Clear()
		{
			SubFolder.Clear();
			if (_isRoot) SubFolder.Add(new HierachicalRootWrapper(this));

			DirectSubNotes.Clear();
		}

		public override string ToString()
		{
			return $"{{{Header}}}::[{string.Join(", ", DirectSubNotes)}]";
		}

		public HierachicalFolderWrapper Find(INote note)
		{
			foreach (var item in SubFolder)
			{
				if (item is HierachicalRootWrapper) continue;
				var n = item.Find(note);
				if (n != null) return null;
			}
			foreach (var item in DirectSubNotes)
			{
				if (item == note) return this;
			}
			return null;
		}

		public HierachicalFolderWrapper Find(DirectoryPath path)
		{
			foreach (var item in SubFolder)
			{
				if (item is HierachicalRootWrapper) continue;
				if (item._path.EqualsIgnoreCase(path)) return item;
				var n = item.Find(path);
				if (n != null) return n;
			}
			return null;
		}

		public DirectoryPath GetNewNotePath()
		{
			return _path;
		}
	}
}
