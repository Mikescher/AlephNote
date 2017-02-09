
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

		string GetLocalUniqueName();

		void SetDirty();
		void ResetLocalDirty();
		void ResetRemoteDirty();

		ObservableCollection<string> Tags { get; }
		string Text { get; set; }
		string Title { get; set; }
		bool IsLocalSaved { get; set; }
		bool IsRemoteSaved { get; set; }
		DateTimeOffset ModificationDate { get; set; }
	}

	public abstract class BasicNote : ObservableObject,  INote
	{
		private bool _ignoreChangeEvent = false;

		private bool _isLocalSaved = false;
		public bool IsLocalSaved { get { return _isLocalSaved; } set { _isLocalSaved = value; OnPropertyChanged(); } }

		private bool _isRemoteSaved = false;
		public bool IsRemoteSaved { get { return _isRemoteSaved; } set { _isRemoteSaved = value; OnPropertyChanged(); } }

		public event EventHandler OnChanged;

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

		public void SetDirty()
		{
			IsLocalSaved = false;
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

		public abstract string GetLocalUniqueName();

		public abstract ObservableCollection<string> Tags { get; }
		public abstract string Text { get; set; }
		public abstract string Title { get; set; }
		public abstract DateTimeOffset ModificationDate { get; set; }
	}
}
