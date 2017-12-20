using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using AlephNote.PluginInterface.Util;
using System.Text;

namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicFlatNote : INotifyPropertyChanged, INote
	{
		private int _dirtySupressor = 0;

		private bool _isLocalSaved = false;
		public bool IsLocalSaved { get { return _isLocalSaved; } set { _isLocalSaved = value; OnPropertyChanged(); } }

		private bool _isRemoteSaved = false;
		public bool IsRemoteSaved { get { return _isRemoteSaved; } set { _isRemoteSaved = value; OnPropertyChanged(); } }

		private bool _isConflictNote = false;
		public bool IsConflictNote { get { return _isConflictNote; } set { _isConflictNote = value; OnPropertyChanged(); } }

		public string Title { get { return GetTitle(); } set { SetInternalTitle(GetPath(), value); OnPropertyChanged(); } }

		public DirectoryPath Path { get { return GetPath(); } set { SetInternalTitle(value, GetTitle()); OnPropertyChanged(); } }

		public event NoteChangedEventHandler OnChanged;

		protected readonly HierachyEmulationConfig _hConfig;

		private Tuple<string, DirectoryPath, string> _pathCache = null;

		private class NoteDirtyBlocker : IDisposable
		{
			private readonly BasicFlatNote note;

			public NoteDirtyBlocker(BasicFlatNote n)
			{
				note = n;
				note._dirtySupressor++;
			}

			public void Dispose()
			{
				note._dirtySupressor--;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnExplicitPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected BasicFlatNote(HierachyEmulationConfig hcfg)
		{
			_hConfig = hcfg;
			PropertyChanged += Changed;
			Tags.CollectionChanged += TagsChanged;
		}

		private void Changed(object sender, PropertyChangedEventArgs e)
		{
			if (_dirtySupressor > 0) return;

			if (e.PropertyName == "InternalTitle")
			{
				OnExplicitPropertyChanged("Title");
				OnExplicitPropertyChanged("Path");
			}

			if (e.PropertyName == "Text" || e.PropertyName == "Title" || e.PropertyName == "Path")
			{
				SetDirty();
				ModificationDate = DateTimeOffset.Now;
				if (IsConflictNote) IsConflictNote = false;
				OnChanged?.Invoke(this, new NoteChangedEventArgs(this, e.PropertyName));
			}
		}

		private void TagsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_dirtySupressor > 0) return;

			SetDirty();
			ModificationDate = DateTimeOffset.Now;
			if (IsConflictNote) IsConflictNote = false;
			OnChanged?.Invoke(this, new NoteChangedEventArgs(this, "Tags"));
		}

		public bool EqualID(INote other)
		{
			return GetUniqueName() == other.GetUniqueName();
		}

		public void SetDirty()
		{
			IsLocalSaved = false;
			IsRemoteSaved = false;
		}

		public void SetLocalDirty()
		{
			IsLocalSaved = false;
		}

		public void SetRemoteDirty()
		{
			IsRemoteSaved = false;
		}

		public void ResetLocalDirty()
		{
			IsLocalSaved = true;
		}

		public void ResetRemoteDirty()
		{
			IsRemoteSaved = true;
		}

		public abstract XElement Serialize();
		public abstract void Deserialize(XElement input);

		public abstract string GetUniqueName();

		public abstract void OnAfterUpload(INote clonenote);

		public abstract ObservableCollection<string> Tags { get; }
		public abstract string Text { get; set; }
		public abstract string InternalTitle { get; set; }
		public abstract DateTimeOffset CreationDate { get; set; }
		public abstract DateTimeOffset ModificationDate { get; set; }

		protected abstract BasicFlatNote CreateClone();

		public INote Clone()
		{
			var n = CreateClone();
			n._isLocalSaved = _isLocalSaved;
			n._isRemoteSaved = _isRemoteSaved;
			n._isConflictNote = _isConflictNote;
			return n;
		}

		public IDisposable SuppressDirtyChanges()
		{
			return new NoteDirtyBlocker(this);
		}

		public void TriggerOnChanged(bool doNotSendChangeEvents)
		{
			if (doNotSendChangeEvents)
			{
				using (SuppressDirtyChanges())
				{
					OnExplicitPropertyChanged("Text");
					OnExplicitPropertyChanged("Title");
					OnExplicitPropertyChanged("Path");
					OnExplicitPropertyChanged("Tags");
				}
			}
			else
			{
				OnExplicitPropertyChanged("Text");
				OnExplicitPropertyChanged("Title");
				OnExplicitPropertyChanged("Path");
				OnExplicitPropertyChanged("Tags");
			}
		}

		public bool HasTagCasInsensitive(string tag)
		{
			return Tags.Any(t => tag.ToLower() == t.ToLower());
		}

		public abstract void ApplyUpdatedData(INote other);

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
