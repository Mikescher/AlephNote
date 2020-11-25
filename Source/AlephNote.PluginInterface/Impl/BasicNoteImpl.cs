using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AlephNote.PluginInterface.AppContext;
using AlephNote.PluginInterface.Datatypes;
using AlephNote.PluginInterface.Util;

namespace AlephNote.PluginInterface.Impl
{
	public abstract class BasicNoteImpl : INote
	{
		private int _dirtySupressor = 0;
		
		private class NoteDirtyBlocker : IDisposable
		{
			private readonly BasicNoteImpl note;

			public NoteDirtyBlocker(BasicNoteImpl n)
			{
				note = n;
				note._dirtySupressor++;
			}

			public void Dispose()
			{
				note._dirtySupressor--;
			}
		}

		private bool _isLocalSaved = false;
		public abstract DirectoryPath Path { get; set; }
		public bool IsLocalSaved { get { return _isLocalSaved; } set { _isLocalSaved = value; OnPropertyChanged(); } }

		private bool _isRemoteSaved = false;
		public bool IsRemoteSaved { get { return _isRemoteSaved; } set { _isRemoteSaved = value; OnPropertyChanged(); } }

		private bool _isConflictNote = false;
		public bool IsConflictNote { get { return _isConflictNote; } set { _isConflictNote = value; OnPropertyChanged(); } }
		
		public bool IsUINote { get; set; } = false; // no PropertyChanged - only internal tracker
		
		public abstract XElement Serialize();
		public abstract void Deserialize(XElement input);

		public abstract string UniqueName { get; }
		public abstract TagList Tags { get; }
		public abstract string Text { get; set; }
		public abstract string Title { get; set; }
		public abstract bool IsPinned { get; set; }
		public abstract bool IsLocked { get; set; }
		public abstract DateTimeOffset CreationDate { get; set; }
		public abstract DateTimeOffset ModificationDate { get; }

		protected abstract BasicNoteImpl CreateClone();

		public abstract void ApplyUpdatedData(INote other);
		public abstract void OnAfterUpload(INote clonenote);
		
		public abstract void TriggerOnChanged(bool doNotSendChangeEvents);

		public abstract void UpdateModificationDate(string propSource, bool clearConflictFlag);

		public event NoteChangedEventHandler OnChanged;
		
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void OnExplicitPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected BasicNoteImpl()
		{
			PropertyChanged += Changed;
			Tags.OnChanged += TagsChanged;
		}

		public virtual string DateTooltip
		{
			get
			{
				return $"Modified: {ModificationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}\nCreated: {CreationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
			}
		}

		private void Changed(object sender, PropertyChangedEventArgs e)
		{
			if (_dirtySupressor > 0) return;

			OnInternalChanged(e.PropertyName);
		}

		protected virtual void OnInternalChanged(string propName)
		{
			if (propName == "Text" || propName == "Title" || propName == "Path" || propName == "IsPinned" || propName == "IsLocked")
			{
				SetDirty($"PropertyChanged called for {propName}");
				UpdateModificationDate(propName, true);
				OnChanged?.Invoke(this, new NoteChangedEventArgs(this, propName));
				OnExplicitPropertyChanged(nameof(DateTooltip));
			}
		}

		private void TagsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_dirtySupressor > 0) return;

			SetDirty($"TagList.OnChanged called.\n\nAction: [{e.Action}]\nNewItems: [{(e.NewItems==null?"NULL":string.Join(", ", e.NewItems.OfType<string>()))}]\nOldItems: [{(e.OldItems==null?"NULL":string.Join(", ", e.OldItems.OfType<string>()))}]");
			UpdateModificationDate("Tags", true);
			OnChanged?.Invoke(this, new NoteChangedEventArgs(this, "Tags"));
			OnExplicitPropertyChanged(nameof(DateTooltip));
		}

		public bool EqualID(INote other)
		{
			return (UniqueName == other.UniqueName);
		}

		private void LogDirtyChanged(bool localOld, bool localNew, bool remoteOld, bool remoteNew, string method, string sourceinfo)
		{
			AlephAppContext.Logger.Debug(
				"BasicNoteImpl", 
				$"DirtyChanged for Note {UniqueName} -> {method}()", 
				$"Note-UUID:     {UniqueName}\n" +
				$"Note-Title:    {Title}\n" +
				$"IsLocalSaved:  {localOld} --> {localNew}\n" +
				$"IsRemoteSaved: {remoteOld} --> {remoteNew}\n\n" +
				$"EventSource:\n{sourceinfo ?? "<NULL>"}");
		}

		public void SetDirty(string sourceinfo)
		{
			if (IsUINote && (IsLocalSaved || IsRemoteSaved)) LogDirtyChanged(IsLocalSaved, false, IsRemoteSaved, false, "SetDirty", sourceinfo);
			IsLocalSaved = false;
			IsRemoteSaved = false;
		}

		public void SetLocalDirty(string sourceinfo)
		{
			if (IsUINote && IsLocalSaved) LogDirtyChanged(IsLocalSaved, false, IsRemoteSaved, IsRemoteSaved, "SetLocalDirty", sourceinfo);
			IsLocalSaved = false;
		}

		public void SetRemoteDirty(string sourceinfo)
		{
			if (IsUINote && IsRemoteSaved) LogDirtyChanged(IsLocalSaved, IsLocalSaved, IsRemoteSaved, false, "SetRemoteDirty", sourceinfo);
			IsRemoteSaved = false;
		}

		public void ResetLocalDirty(string sourceinfo)
		{
			if (IsUINote && !IsLocalSaved) LogDirtyChanged(IsLocalSaved, true, IsRemoteSaved, IsRemoteSaved, "ResetLocalDirty", sourceinfo);
			IsLocalSaved = true;
		}

		public void ResetRemoteDirty(string sourceinfo)
		{
			if (IsUINote && !IsRemoteSaved) LogDirtyChanged(IsLocalSaved, IsLocalSaved, IsRemoteSaved, true, "ResetRemoteDirty", sourceinfo);
			IsRemoteSaved = true;
		}

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

		public bool HasTagCaseInsensitive(string tag)
		{
			return Tags.Any(t => tag.ToLower() == t.ToLower());
		}

		public bool HasTagCaseSensitive(string tag)
		{
			return Tags.Any(t => tag == t);
		}

		public override string ToString()
		{
			return $"Note: {Path} :: {Title}";
		}
	}
}
