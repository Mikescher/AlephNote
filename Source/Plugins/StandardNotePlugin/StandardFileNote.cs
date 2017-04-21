using System.Collections.Specialized;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardFileNote : BasicNote
	{
		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _title = "";
		public override string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private List<StandardFileTag> _internalTags = new List<StandardFileTag>();
		public List<StandardFileTag> InternalTags { get { return _internalTags; } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		private bool _ignoreTagsChanged = false;
		private readonly StandardNoteConfig _config;

		public StandardFileNote(Guid uid, StandardNoteConfig cfg)
		{
			_id = uid;
			_config = cfg;
			_creationDate = DateTimeOffset.Now;

			_tags.CollectionChanged += TagsChanged;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Tags", _internalTags.Select(t => t.Serialize()).Cast<object>().ToArray()),
				new XElement("Text", Convert.ToBase64String(Encoding.UTF8.GetBytes(_text))),
				new XElement("Title", _title),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", _creationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
			};

			var r = new XElement("standardnote", data);
			r.SetAttributeValue("plugin", StandardNotePlugin.Name);
			r.SetAttributeValue("pluginversion", StandardNotePlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			using (SuppressDirtyChanges())
			{
				_internalTags = XHelper.GetChildOrThrow(input, "Tags").Elements().Select(StandardFileTag.Deserialize).ToList();

				_id = XHelper.GetChildValueGUID(input, "ID");
				_tags.Synchronize(_internalTags.Select(it => it.Title));
				_text = Encoding.UTF8.GetString(Convert.FromBase64String(XHelper.GetChildValueString(input, "Text")));
				_title = XHelper.GetChildValueString(input, "Title");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
			}
		}

		private void TagsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_ignoreTagsChanged) return;

			if (e.NewItems != null)
				foreach (var item in e.NewItems.Cast<string>())
				{
					if (_internalTags.All(it => it.Title != item))
						_internalTags.Add(new StandardFileTag(null, item));
				}

			if (e.OldItems != null)
				foreach (var item in e.OldItems.Cast<string>())
				{
					_internalTags.RemoveAll(it => it.Title == item);
				}
		}

		public void UpgradeTag(StandardFileTag told, StandardFileTag tnew)
		{
			int idx = _internalTags.IndexOf(told);
			_internalTags[idx] = tnew;
		}

		public void SetTags(IEnumerable<StandardFileTag> newtags)
		{
			_internalTags = newtags.ToList();
			ResyncTags();
		}

		private void ResyncTags()
		{
			try
			{
				_ignoreTagsChanged = true;
				_tags.Synchronize(_internalTags.Select(it => it.Title));
			}
			finally
			{
				_ignoreTagsChanged = false;
			}
		}

		public override string GetUniqueName()
		{
			return _id.ToString("N");
		}

		public override void OnAfterUpload(INote iother)
		{
			var other = (StandardFileNote)iother;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate = other.CreationDate;
				_internalTags = other._internalTags;
				ResyncTags();
			}
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (StandardFileNote)iother;

			using (SuppressDirtyChanges())
			{
				_modificationDate = other.ModificationDate;
				_creationDate = other.CreationDate;
				_internalTags = other._internalTags.ToList();
				ResyncTags();
				_text = other.Text;
				_title = other.Title;
			}
		}

		public bool EqualsIgnoreModificationdate(StandardFileNote other)
		{
			if (_id != other._id) return false;
			if (_creationDate != other._creationDate) return false;
			if (!new HashSet<StandardFileTag>(_internalTags).SetEquals(other._internalTags)) return false;
			if (_text != other._text) return false;
			if (_title != other._title) return false;

			return true;
		}

		protected override BasicNote CreateClone()
		{
			var n = new StandardFileNote(_id, _config);
			n._internalTags = _internalTags.ToList();
			n.ResyncTags();
			n._text = _text;
			n._title = _title;
			n._creationDate = _creationDate;
			n._modificationDate = _modificationDate;
			return n;
		}
	}
}
