using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicNote : INotifyPropertyChanged, INote
	{
		private int _dirtySupressor = 0;

		private bool _isLocalSaved = false;
		public bool IsLocalSaved { get { return _isLocalSaved; } set { _isLocalSaved = value; OnPropertyChanged(); } }

		private bool _isRemoteSaved = false;
		public bool IsRemoteSaved { get { return _isRemoteSaved; } set { _isRemoteSaved = value; OnPropertyChanged(); } }

		private bool _isConflictNote = false;
		public bool IsConflictNote { get { return _isConflictNote; } set { _isConflictNote = value; OnPropertyChanged(); } }

		public event NoteChangedEventHandler OnChanged;

		private class NoteDirtyBlocker : IDisposable
		{
			private readonly BasicNote note;

			public NoteDirtyBlocker(BasicNote n)
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
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnExplicitPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected BasicNote()
		{
			PropertyChanged += Changed;
			Tags.CollectionChanged += TagsChanged;
		}

		private void Changed(object sender, PropertyChangedEventArgs e)
		{
			if (_dirtySupressor > 0) return;

			if (e.PropertyName == "Text" || e.PropertyName == "Title")
			{
				SetDirty();

				ModificationDate = DateTimeOffset.Now;

				if (IsConflictNote) IsConflictNote = false;

				if (OnChanged != null) OnChanged(this, new NoteChangedEventArgs(this, e.PropertyName));
			}
		}

		private void TagsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetDirty();

			if (IsConflictNote) IsConflictNote = false;

			if (OnChanged != null) OnChanged(this, new NoteChangedEventArgs(this, "Tags"));
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
		public abstract string Title { get; set; }
		public abstract DateTimeOffset CreationDate { get; set; }
		public abstract DateTimeOffset ModificationDate { get; set; }

		protected abstract BasicNote CreateClone();

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
					OnExplicitPropertyChanged("Tags");
				}
			}
			else
			{
				OnExplicitPropertyChanged("Text");
				OnExplicitPropertyChanged("Title");
				OnExplicitPropertyChanged("Tags");
			}
		}

		public bool HasTagCasInsensitive(string tag)
		{
			return Tags.Any(t => tag.ToLower() == t.ToLower());
		}

		public abstract void ApplyUpdatedData(INote other);
	}
}
