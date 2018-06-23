using AlephNote.PluginInterface.Util;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace AlephNote.PluginInterface
{
	public interface INote
	{
		event NoteChangedEventHandler OnChanged;

		XElement Serialize();
		void Deserialize(XElement input);

		bool EqualID(INote clonenote);

		void SetDirty();
		void SetLocalDirty();
		void SetRemoteDirty();
		void ResetLocalDirty();
		void ResetRemoteDirty();

		void OnAfterUpload(INote clonenote);
		void ApplyUpdatedData(INote other);

		string UniqueName { get; }

		ObservableCollection<string> Tags { get; }
		string Text { get; set; }
		string Title { get; set; }
		bool IsPinned { get; set; }
		bool IsLocked { get; set; }
		DirectoryPath Path { get; set; }

		bool IsLocalSaved { get; set; }
		bool IsRemoteSaved { get; set; }
		bool IsConflictNote { get; set; }
		DateTimeOffset CreationDate { get; set; }
		DateTimeOffset ModificationDate { get; set; }

		INote Clone();
		IDisposable SuppressDirtyChanges();
		void TriggerOnChanged(bool doNotSendChangeEvents);
		bool HasTagCaseInsensitive(string tag);
		bool HasTagCaseSensitive(string tag);
	}
}
