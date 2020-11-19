using AlephNote.PluginInterface.Util;
using System;
using System.ComponentModel;
using System.Xml.Linq;
using AlephNote.PluginInterface.Datatypes;

namespace AlephNote.PluginInterface
{
	public interface INote : INotifyPropertyChanged
	{
		event NoteChangedEventHandler OnChanged;

		XElement Serialize();
		void Deserialize(XElement input);

		bool EqualID(INote clonenote);

		void SetDirty(string sourceinfo);
		void SetLocalDirty(string sourceinfo);
		void SetRemoteDirty(string sourceinfo);
		void ResetLocalDirty(string sourceinfo);
		void ResetRemoteDirty(string sourceinfo);

		void OnAfterUpload(INote clonenote);
		void ApplyUpdatedData(INote other);

		string UniqueName { get; }

		TagList Tags { get; }
		string Text { get; set; }
		string Title { get; set; }
		bool IsPinned { get; set; }
		bool IsLocked { get; set; }
		DirectoryPath Path { get; set; }
		
		bool IsLocalSaved { get; }
		bool IsRemoteSaved { get; }
		bool IsConflictNote { get; set; }
		DateTimeOffset CreationDate { get; set; }
		DateTimeOffset ModificationDate { get; }

		string DateTooltip { get; }

		bool IsUINote { get; set; } // "Real" Note that is linked to the UI - not a intermediate copy

		INote Clone();
		IDisposable SuppressDirtyChanges();
		void TriggerOnChanged(bool doNotSendChangeEvents);
		bool HasTagCaseInsensitive(string tag);
		bool HasTagCaseSensitive(string tag);
	}
}
