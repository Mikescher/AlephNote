using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MSHC.Lang.Collections;
using MSHC.WPF.MVVM;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace AlephNote.Common.Hierarchy
{
	public interface IHierarchicalWrapperConfig
	{
		bool SearchFilter(INote note);
		IComparer<INote> DisplaySorter();
		bool ShowAllNotesNode { get; }
		bool ShowEmptyPathNode { get; }
	}

	public abstract class HierarchicalBaseWrapper : ObservableObject
	{
		public static readonly DirectoryPath DP_ALLNOTES      = DirectoryPath.Create(".~AlephNote.Wrapper.AllNotes~");
		public static readonly DirectoryPath DP_UNSORTEDNOTES = DirectoryPath.Create(".~AlephNote.Wrapper.UnsortedNotes~");
		public static readonly DirectoryPath DP_ROOTFOLDER    = DirectoryPath.Root();

		public abstract string Header { get; }

		private bool _isSelected = false;
		public bool IsSelected { get { return _isSelected; } set { _isSelected = value; OnPropertyChanged(); } }

		private bool _isExpanded = true;
		public bool IsExpanded { get { return _isExpanded; } set { _isExpanded = value; OnPropertyChanged(); } }

		public int CustomOrder = 0;

		public abstract string IconSourceKey { get; }
		public abstract int Order { get; }
		public abstract bool IsSpecialNode { get; }
		public abstract bool IsSpecialNode_AllNotes { get; }
		public abstract bool IsSpecialNode_UnsortedNotes { get; }
		public abstract bool IsSpecialNode_RootFolder { get; }

		public abstract void Sync(HierarchicalBaseWrapper aother, HierarchicalWrapper_Folder[] parents);
		
		public static bool IsSpecial(DirectoryPath p)
		{
			return p.EqualsWithCase(DP_ALLNOTES) || p.EqualsWithCase(DP_UNSORTEDNOTES) || p.EqualsWithCase(DP_ROOTFOLDER);
		}
		
		public static bool IsSpecialOrRoot(DirectoryPath p)
		{
			return p.IsRoot() || p.EqualsWithCase(DP_ALLNOTES) || p.EqualsWithCase(DP_UNSORTEDNOTES) || p.EqualsWithCase(DP_ROOTFOLDER);
		}
	}

	public class HierarchicalWrapper_AllNotes : HierarchicalWrapper_Folder
	{
		private readonly HierarchicalWrapper_Folder _baseWrapper;

		public override string IconSourceKey => "folder_all.png";
		public override int Order => -2;
		public override bool IsSpecialNode => true;
		public override bool IsSpecialNode_AllNotes => true;
		public override bool IsSpecialNode_UnsortedNotes => false;
		public override bool IsSpecialNode_RootFolder => false;

		public override void Sync(HierarchicalBaseWrapper aother, HierarchicalWrapper_Folder[] parents)
		{
			var other = (HierarchicalWrapper_AllNotes)aother;

			AllSubNotes.SynchronizeCollectionSafe(other.AllSubNotes);
		}

		public override IEnumerable<INote> GetAllSubNotes(bool rec) => _baseWrapper.GetAllSubNotes(true);

		public override DirectoryPath GetNewNotePath() => DirectoryPath.Root();

		public HierarchicalWrapper_AllNotes(HierarchicalWrapper_Folder baseWrapper, IHierarchicalWrapperConfig cfg) 
			: base("All notes", cfg, DP_ALLNOTES, false, false)
		{
			_baseWrapper = baseWrapper;
			IsSelected = true;
		}
	}

	public class HierarchicalWrapper_UnsortedNotes : HierarchicalWrapper_Folder
	{
		private readonly HierarchicalWrapper_Folder _baseWrapper;

		public override string IconSourceKey => "folder_none.png";
		public override int Order => -1;
		public override bool IsSpecialNode => true;
		public override bool IsSpecialNode_AllNotes => false;
		public override bool IsSpecialNode_UnsortedNotes => true;
		public override bool IsSpecialNode_RootFolder => false;

		public override void Sync(HierarchicalBaseWrapper aother, HierarchicalWrapper_Folder[] parents)
		{
			var other = (HierarchicalWrapper_UnsortedNotes)aother;

			AllSubNotes.SynchronizeCollectionSafe(other.AllSubNotes);
		}

		public override IEnumerable<INote> GetAllSubNotes(bool rec) => _baseWrapper.GetAllSubNotes(true).Where(n => n.Path.IsRoot());

		public override DirectoryPath GetNewNotePath() => DirectoryPath.Root();

		public HierarchicalWrapper_UnsortedNotes(HierarchicalWrapper_Folder baseWrapper, IHierarchicalWrapperConfig cfg) 
			: base("Unsorted notes", cfg, DP_UNSORTEDNOTES, false, false)
		{
			_baseWrapper = baseWrapper;
		}
	}

	public class HierarchicalWrapper_Folder : HierarchicalBaseWrapper
	{
		private readonly bool _isRoot;
		private readonly IHierarchicalWrapperConfig _config;
		private string _header;
		public override string Header => _header;

		private DirectoryPath _path;

		public override string IconSourceKey => _path.IsRoot() ? "folder_root.png" : "folder_any.png";
		public override int Order => 0;
		public override bool IsSpecialNode               => false;
		public override bool IsSpecialNode_AllNotes      => false;
		public override bool IsSpecialNode_UnsortedNotes => false;
		public override bool IsSpecialNode_RootFolder    => _path.IsRoot();

		public HierarchicalWrapper_AllNotes      AllNotesViewWrapper;   // [All Notes]
		public HierarchicalWrapper_UnsortedNotes EmptyPathViewWrapper;  // [Unsorted Notes]
		public HierarchicalWrapper_Folder        RootFolderViewWrapper; // [My Notes]

		private ObservableCollection<HierarchicalWrapper_Folder> _subFolders = new ObservableCollection<HierarchicalWrapper_Folder>();
		public ObservableCollection<HierarchicalWrapper_Folder> SubFolder { get { return _subFolders; } }

		public ObservableCollection<INote> AllSubNotes { get; set; } = new ObservableCollection<INote>();
		private List<INote> _directSubNotes = new List<INote>();

		public virtual IEnumerable<INote> GetAllSubNotes(bool rec)
		{
			if (!rec)
				return _directSubNotes
						.Where(_config.SearchFilter)
						.OrderBy(p => p, _config.DisplaySorter());
			else
				return _directSubNotes
						.Concat(SubFolder.Where(sf => !sf.IsSpecialNode)
						.SelectMany(sf => sf.GetAllSubNotes(true)))
						.Where(_config.SearchFilter)
						.OrderBy(p => p, _config.DisplaySorter());
		}

		public bool Permanent = false;

		public HierarchicalWrapper_Folder(string header, IHierarchicalWrapperConfig cfg, DirectoryPath path, bool root, bool perm)
		{
			_isRoot = root;
			_path = path;
			_header = header;
			_config = cfg;
			Permanent = perm;

			if (_isRoot && cfg.ShowAllNotesNode) SubFolder.Add(AllNotesViewWrapper  = new HierarchicalWrapper_AllNotes(this, cfg));
			if (_isRoot && cfg.ShowEmptyPathNode) SubFolder.Add(EmptyPathViewWrapper = new HierarchicalWrapper_UnsortedNotes(this, cfg));
		}

		public HierarchicalWrapper_Folder GetOrCreateFolder(string txt, out bool created)
		{
			foreach (var item in SubFolder)
			{
				if (item.Header.ToLower() == txt.ToLower()) { created = false; return item;}
			}
			var i = new HierarchicalWrapper_Folder(txt, _config, _path.SubDir(txt), false, false);
			SubFolder.Add(i);
			created = true;
			return i;
		}

		public HierarchicalWrapper_Folder GetFolder(string txt)
		{
			foreach (var item in SubFolder) if (item.Header.ToLower() == txt.ToLower()) return item;
			return null;
		}

		public HierarchicalWrapper_Folder GetOrCreateRootFolder()
		{
			if (RootFolderViewWrapper != null) return RootFolderViewWrapper;
			RootFolderViewWrapper = new HierarchicalWrapper_Folder("[My Notes]", _config, DP_ROOTFOLDER, false, false);
			SubFolder.Add(RootFolderViewWrapper);
			return RootFolderViewWrapper;
		}

		public HierarchicalWrapper_Folder GetRootFolder()
		{
			if (RootFolderViewWrapper != null) return RootFolderViewWrapper;
			return null;
		}

		public void TriggerAllSubNotesChanged()
		{
			OnPropertyChanged("DirectSubNotes");
			OnPropertyChanged("AllSubNotes");
		}

		public override void Sync(HierarchicalBaseWrapper aother, HierarchicalWrapper_Folder[] parents)
		{
			var other = (HierarchicalWrapper_Folder)aother;

			Debug.Assert(this.Header.ToLower() == other.Header.ToLower());
			Debug.Assert(this._isRoot == other._isRoot);

			_path = other._path;
			_directSubNotes = other._directSubNotes;
			AllSubNotes.SynchronizeCollectionSafe(other.AllSubNotes);

			var me = this;

			bool FCompare(HierarchicalWrapper_Folder a, HierarchicalWrapper_Folder b)
			{
				return a.Header.ToLower() == b.Header.ToLower();
			}

			HierarchicalWrapper_Folder FCopy(HierarchicalWrapper_Folder src)
			{
				if (src.GetType() == typeof(HierarchicalWrapper_Folder) && src.IsSpecialNode_RootFolder)
					return me.RootFolderViewWrapper = new HierarchicalWrapper_Folder(src.Header, _config, src._path, src._isRoot, src.Permanent)
					{ 
						IsExpanded = src.IsExpanded,
						CustomOrder = src.CustomOrder,
					};

				if (src.GetType() == typeof(HierarchicalWrapper_Folder))
					return new HierarchicalWrapper_Folder(src.Header, _config, src._path, src._isRoot, src.Permanent)
					{
						IsExpanded = src.IsExpanded,
						CustomOrder = src.CustomOrder,
					};

				if (src.GetType() == typeof(HierarchicalWrapper_UnsortedNotes))
					return me.EmptyPathViewWrapper = new HierarchicalWrapper_UnsortedNotes(me, _config)
					{
						IsExpanded = src.IsExpanded,
						CustomOrder = src.CustomOrder,
					};

				if (src.GetType() == typeof(HierarchicalWrapper_AllNotes))
					return me.AllNotesViewWrapper  = new HierarchicalWrapper_AllNotes(me, _config)
					{
						IsExpanded = src.IsExpanded,
						CustomOrder = src.CustomOrder,
					};

				throw new NotSupportedException(src.GetType().ToString());
			}

			SubFolder.SynchronizeCollection(other.SubFolder, FCompare, FCopy);
			Debug.Assert(SubFolder.Count == other.SubFolder.Count);

			for (int i = 0; i < SubFolder.Count; i++) SubFolder[i].Sync(other.SubFolder[i], parents.Concat(new []{this}).ToArray());
		}

		public void CopyPermanentsTo(HierarchicalWrapper_Folder other)
		{
			foreach (var folder1 in SubFolder)
			{
				var folder2 = other.SubFolder.FirstOrDefault(sf => sf._path.EqualsIgnoreCase(folder1._path));
				if (folder2 == null && folder1.Permanent)
				{
					folder2 = other.GetOrCreateFolder(folder1.Header, out _);
					folder2.Permanent = true;
				}

				if (folder2 != null) folder1.CopyPermanentsTo(folder2);
			}
		}

		public void ClearPermanents()
		{
			Permanent = false;
			foreach (var sf in SubFolder) sf.ClearPermanents();
		}

		public void Add(HierarchicalWrapper_Folder elem) { SubFolder.Add(elem); }

		public void Add(INote elem) { _directSubNotes.Add(elem); }

		public void Clear()
		{
			SubFolder.Clear();
			if (_isRoot) SubFolder.Add(new HierarchicalWrapper_AllNotes(this, _config));

			_directSubNotes.Clear();

			AllSubNotes.Clear();
		}

		public override string ToString()
		{
			return $"{{{Header}}}::[{string.Join(", ", _directSubNotes)}]";
		}

		public HierarchicalWrapper_Folder Find(INote note)
		{
			foreach (var item in SubFolder)
			{
				if (item.IsSpecialNode) continue;
				var n = item.Find(note);
				if (n != null) return null;
			}
			foreach (var item in _directSubNotes)
			{
				if (item == note) return this;
			}
			return null;
		}

		public HierarchicalWrapper_Folder Find(DirectoryPath path, bool includeSpec)
		{
			foreach (var item in SubFolder)
			{
				if (!includeSpec && item.IsSpecialNode) continue;
				if (item._path.EqualsIgnoreCase(path)) return item;
				var n = item.Find(path, includeSpec);
				if (n != null) return n;
			}
			return null;
		}

		public DirectoryPath GetInternalPath() => _path;

		public virtual DirectoryPath GetNewNotePath()
		{
			return _path;
		}

		public bool RemoveFind(DirectoryPath folder)
		{
			for (int i = 0; i < SubFolder.Count; i++)
			{
				if (SubFolder[i].GetNewNotePath().EqualsIgnoreCase(folder)) { SubFolder.RemoveAt(i); return true; }
				if (SubFolder[i].RemoveFind(folder)) return true;
			}
			return false;
		}

		public void Sort(bool rec = true)
		{
			SubFolder.SynchronizeCollectionSafe(SubFolder.OrderBy(p => p.Order).ThenBy(p => p.CustomOrder).ThenBy(p => p.Header.ToLower()));
			if (rec) foreach (var sf in SubFolder) sf.Sort();
		}

		public void FinalizeCollection(bool rec)
		{
			AllSubNotes.Synchronize(GetAllSubNotes(rec).ToList());
			foreach (var sf in SubFolder) sf.FinalizeCollection(rec);
		}

		public IEnumerable<DirectoryPath> ListPaths()
		{
			if (!_isRoot) yield return _path;
			foreach (var sf in SubFolder)
			{
				foreach (var p in sf.ListPaths()) yield return p;
			}
		}

		public HierarchyConfigEntry ToHCEntry()
        {
			return new HierarchyConfigEntry
			(
				Header,
				IsExpanded,
				_path,
				SubFolder.OrderBy(p => p.Order).ThenBy(p => p.CustomOrder).ThenBy(p => p.Header.ToLower()).Select(p => p.ToHCEntry()).ToList()
			);
        }
    }
}
