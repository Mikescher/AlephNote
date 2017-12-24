using System.Collections.Specialized;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardFileNote : BasicFlatNote
	{
		public class StandardFileRef { public Guid UUID; public string Type; }

		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private string _internaltitle = "";
		public override string InternalTitle { get { return _internaltitle; } set { _internaltitle = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private string _contentVersion = "";
		public string ContentVersion { get { return _contentVersion; } set { _contentVersion = value; OnPropertyChanged(); } }

		private string _authHash = "";
		public string AuthHash { get { return _authHash; } set { _authHash = value; OnPropertyChanged(); } }

		private List<StandardFileTag> _internalTags = new List<StandardFileTag>();
		public List<StandardFileTag> InternalTags { get { return _internalTags; } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public override ObservableCollection<string> Tags { get { return _tags; } }

		private readonly ObservableCollection<StandardFileRef> _internalRef = new ObservableCollection<StandardFileRef>();
		public ObservableCollection<StandardFileRef> InternalReferences { get { return _internalRef; } }

		private bool _ignoreTagsChanged = false;
		private readonly StandardNoteConfig _config;

		public StandardFileNote(Guid uid, StandardNoteConfig cfg, HierachyEmulationConfig hcfg)
			: base(hcfg)
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
				new XElement("Text", XHelper.ConvertToC80Base64(_text)),
				new XElement("Title", _internaltitle),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", _creationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("ContentVersion", _contentVersion),
				new XElement("AuthHash", _authHash),
				new XElement("InternalReferences", _internalRef.Select(ir => new XElement("Ref", new XAttribute("Type", ir.Type), new XAttribute("UUID", ir.UUID.ToString("P").ToUpper())))),
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
				_text = XHelper.GetChildBase64String(input, "Text");
				_internaltitle = XHelper.GetChildValueString(input, "Title");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_contentVersion = XHelper.GetChildValueStringOrDefault(input, "ContentVersion", "?");
				_authHash = XHelper.GetChildValueStringOrDefault(input, "AuthHash", "?");

				var intref = XHelper.GetChildOrNull(input, "InternalReferences")?.Elements("Ref").Select(x => new StandardFileRef {UUID = XHelper.GetAttributeGuid(x, "UUID"), Type = XHelper.GetAttributeString(x, "Type")}).ToList();
				if (intref != null) _internalRef.Synchronize(intref);
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

		public void SetReferences(List<StandardNoteAPI.APIResultContentRef> refs)
		{
			_internalRef.Synchronize(refs.Select(r => new StandardFileRef{UUID = r.uuid, Type = r.content_type}));
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
				_internalRef.Synchronize(other._internalRef);
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
				_internaltitle = other.Title;
				_contentVersion = other.ContentVersion;
				_authHash = other.AuthHash;
				_internalRef.Synchronize(other._internalRef);
			}
		}

		public bool EqualsIgnoreModificationdate(StandardFileNote other)
		{
			if (_id != other._id) return false;
			if (_creationDate != other._creationDate) return false;
			if (!new HashSet<StandardFileTag>(_internalTags).SetEquals(other._internalTags)) return false;
			if (_text != other._text) return false;
			if (_internaltitle != other._internaltitle) return false;

			return true;
		}

		protected override BasicFlatNote CreateClone()
		{
			var n               = new StandardFileNote(_id, _config, _hConfig);

			n._internalTags     = _internalTags.ToList();
			n.ResyncTags();
			n._text             = _text;
			n._internaltitle    = _internaltitle;
			n._creationDate     = _creationDate;
			n._modificationDate = _modificationDate;
			n._contentVersion   = _contentVersion;
			n._authHash         = _authHash;
			n._internalRef.Synchronize(_internalRef);
			return n;
		}
	}
}
