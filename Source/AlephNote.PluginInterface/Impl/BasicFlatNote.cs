using System;
using System.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicFlatNote : BasicNoteImpl
	{
		public override string Title { get { return GetTitle(); } set { SetInternalTitle(GetPath(), value); OnPropertyChanged(); } }

		public override DirectoryPath Path { get { return GetPath(); } set { SetInternalTitle(value, GetTitle()); OnPropertyChanged(); } }
		
		protected readonly HierachyEmulationConfig _hConfig;

		private Tuple<string, DirectoryPath, string> _pathCache = null;

		protected BasicFlatNote(HierachyEmulationConfig hcfg) : base()
		{
			_hConfig = hcfg;
		}

		protected override void OnInternalChanged(string propName)
		{
			if (propName == "InternalTitle")
			{
				OnExplicitPropertyChanged("Title");
				OnExplicitPropertyChanged("Path");
			}

			base.OnInternalChanged(propName);
		}
		
		public abstract string InternalTitle { get; set; }
		
		public override void TriggerOnChanged(bool doNotSendChangeEvents)
		{
			if (doNotSendChangeEvents)
			{
				using (SuppressDirtyChanges())
				{
					OnExplicitPropertyChanged("Text");
					OnExplicitPropertyChanged("Title");
					OnExplicitPropertyChanged("Path");
					OnExplicitPropertyChanged("Tags");
					Tags.CallOnCollectionChanged();
					OnExplicitPropertyChanged("ModificationDate");
					OnExplicitPropertyChanged("IsPinned");
					OnExplicitPropertyChanged("IsLocked");
				}
			}
			else
			{
				OnExplicitPropertyChanged("Text");
				OnExplicitPropertyChanged("Title");
				OnExplicitPropertyChanged("Path");
				OnExplicitPropertyChanged("Tags");
				Tags.CallOnCollectionChanged();
				OnExplicitPropertyChanged("ModificationDate");
				OnExplicitPropertyChanged("IsPinned");
				OnExplicitPropertyChanged("IsLocked");
			}
		}
		
		private void SetInternalTitle(DirectoryPath p, string t)
		{
			if (!_hConfig.EmulateSubfolders)
			{
				InternalTitle = t;
				return;
			}
			
			InternalTitle = _hConfig.EscapeStringListForRemote(p.Enumerate().Concat(new[] { t }));
		}

		private string GetTitle()
		{
			if (!_hConfig.EmulateSubfolders)
			{
				return InternalTitle;
			}

			if (_pathCache != null && _pathCache.Item1 == InternalTitle) return _pathCache.Item3;

			var plist = _hConfig.UnescapeStringFromRemote(InternalTitle).ToList();
			var t = plist.Last();
			plist.RemoveAt(plist.Count - 1);
			var p = DirectoryPath.Create(plist);

			_pathCache = Tuple.Create(InternalTitle, p, t);

			return t;
		}

		private DirectoryPath GetPath()
		{
			if (!_hConfig.EmulateSubfolders)
			{
				return DirectoryPath.Root();
			}

			if (_pathCache != null && _pathCache.Item1 == InternalTitle) return _pathCache.Item2;

			var plist = _hConfig.UnescapeStringFromRemote(InternalTitle).ToList();
			var t = plist.Last();
			plist.RemoveAt(plist.Count - 1);
			var p = DirectoryPath.Create(plist);

			_pathCache = Tuple.Create(InternalTitle, p, t);

			return p;
		}

	}
}
