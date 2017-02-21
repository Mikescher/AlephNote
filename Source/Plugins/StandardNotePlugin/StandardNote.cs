using AlephNote.PluginInterface;
using MSHC.Lang.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace AlephNote.Plugins.StandardNote
{
	class StandardNote : BasicNote
	{
		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _title = "";
		public override string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.MinValue;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		private string _encryptionItemKey = "";
		public string EncryptionItemKey { get { return _encryptionItemKey; } set { _encryptionItemKey = value; OnPropertyChanged(); } }

		private readonly StandardNoteConfig _config;

		public StandardNote(Guid uid, StandardNoteConfig cfg)
		{
			_id = uid;
			_config = cfg;
			_creationDate = DateTimeOffset.Now;
		}

		public override XElement Serialize()
		{
			throw new NotImplementedException();
		}

		public override void Deserialize(XElement input)
		{
			throw new NotImplementedException();
		}

		public override string GetUniqueName()
		{
			throw new NotImplementedException();
		}

		public override void OnAfterUpload(INote clonenote)
		{
			throw new NotImplementedException();
		}

		public override void ApplyUpdatedData(INote other)
		{
			throw new NotImplementedException();
		}

		protected override BasicNote CreateClone()
		{
			var n = new StandardNote(_id, _config);
			n._tags.Synchronize(_tags.ToList());
			n._text = _text;
			n._title = _title;
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			return n;
		}
	}
}
