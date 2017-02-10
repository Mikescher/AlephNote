
using MSHC.WPF.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Linq;

namespace CommonNote.PluginInterface
{
	public interface INote
	{
		event EventHandler OnChanged;

		XElement Serialize();
		void Deserialize(XElement input);

		string GetUniqueName();
		bool EqualID(INote clonenote);

		void SetDirty();
		void SetLocalDirty();
		void ResetLocalDirty();
		void ResetRemoteDirty();

		void OnAfterUpload(INote clonenote);
		void ApplyUpdatedData(INote other);

		ObservableCollection<string> Tags { get; }
		string Text { get; set; }
		string Title { get; set; }
		bool IsLocalSaved { get; set; }
		bool IsRemoteSaved { get; set; }
		DateTimeOffset ModificationDate { get; set; }

		INote Clone();
		IDisposable SuppressDirtyChanges();
		void TriggerOnChanged();
	}

	public abstract class BasicNote : ObservableObject,  INote
	{
		private int _dirtySupressor = 0;
		private bool _ignoreChangeEvent = false;

		private bool _isLocalSaved = false;
		public bool IsLocalSaved { get { return _isLocalSaved; } set { _isLocalSaved = value; OnPropertyChanged(); } }

		private bool _isRemoteSaved = false;
		public bool IsRemoteSaved { get { return _isRemoteSaved; } set { _isRemoteSaved = value; OnPropertyChanged(); } }

		public event EventHandler OnChanged;

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

		protected BasicNote()
		{
			PropertyChanged += Changed;
			Tags.CollectionChanged += TagsChanged;
		}

		private void Changed(object sender, PropertyChangedEventArgs e)
		{
			if (_ignoreChangeEvent) return;

			if (e.PropertyName == "Text" || e.PropertyName == "Title" || e.PropertyName == "ModificationDate")
			{
				SetDirty();

				if (e.PropertyName != "ModificationDate")
				{
					_ignoreChangeEvent = true;
					ModificationDate = DateTimeOffset.Now;
					_ignoreChangeEvent = false;
				}

				if (OnChanged != null) OnChanged(this, new EventArgs());
			}
		}

		private void TagsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			SetDirty();
			if (OnChanged != null) OnChanged(this, new EventArgs());
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
		public abstract DateTimeOffset ModificationDate { get; set; }

		protected abstract BasicNote CreateClone();
		public INote Clone()
		{
			var n = CreateClone();
			n._isLocalSaved = _isLocalSaved;
			n._isRemoteSaved = _isRemoteSaved;
			return n;
		}

		public IDisposable SuppressDirtyChanges()
		{
			return new NoteDirtyBlocker(this);
		}

		public void TriggerOnChanged()
		{
			OnExplicitPropertyChanged("Text");
			OnExplicitPropertyChanged("Title");
			OnExplicitPropertyChanged("Tags");
			OnExplicitPropertyChanged("ModificationDate");
		}

		public abstract void ApplyUpdatedData(INote other);
	}
}
