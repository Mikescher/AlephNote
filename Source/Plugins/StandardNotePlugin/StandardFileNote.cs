using System.Collections.Specialized;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.AppContext;
using AlephNote.PluginInterface.Datatypes;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using MSHC.Lang.Collections;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardFileNote : BasicFlatNote
	{
		public class StandardFileRef { public Guid UUID; public string Type; }

		private Guid _id;
		public Guid ID { get { return _id; } set { _id = value; OnPropertyChanged(); } }

		private string _text = "";
		public override string Text { get { return _text ?? string.Empty; } set { _text = value; OnPropertyChanged(); } }

		private string _internaltitle = "";
		public override string InternalTitle { get { return _internaltitle ?? string.Empty; } set { _internaltitle = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private string _contentVersion = "";
		public string ContentVersion { get { return _contentVersion; } set { _contentVersion = value; OnPropertyChanged(); } }

		private string _authHash = "";
		public string AuthHash { get { return _authHash; } set { _authHash = value; OnPropertyChanged(); } }

		private List<StandardFileTagRef> _internalTags = new List<StandardFileTagRef>();
		public List<StandardFileTagRef> InternalTags { get { return _internalTags; } }

		private readonly SimpleTagList _tags = new SimpleTagList();
		public override TagList Tags { get { return _tags; } }

		private bool _isPinned = false;
		public override bool IsPinned { get { return _isPinned; } set { _isPinned = value; OnPropertyChanged(); } }

		private bool _isLocked = false;
		public override bool IsLocked { get { return _isLocked; } set { _isLocked = value; OnPropertyChanged(); } }

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

			_tags.OnChanged += TagsChanged;
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Tags", _internalTags.Select(t => t.Serialize()).Cast<object>().ToArray()),
				new XElement("Text", XHelper.ConvertToC80Base64(_text)),
				new XElement("Title", _internaltitle),
				new XElement("ModificationDate", XHelper.ToString(ModificationDate)),
				new XElement("CreationDate", XHelper.ToString(_creationDate)),
				new XElement("ContentVersion", _contentVersion),
				new XElement("AuthHash", _authHash),
				new XElement("InternalReferences", _internalRef.Select(ir => new XElement("Ref", new XAttribute("Type", ir.Type), new XAttribute("UUID", ir.UUID.ToString("P").ToUpper())))),
				new XElement("IsPinned", _isPinned),
				new XElement("IsLocked", _isLocked),
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
				_internalTags = XHelper.GetChildOrThrow(input, "Tags").Elements().Select(StandardFileTagRef.Deserialize).ToList();

				_id = XHelper.GetChildValueGUID(input, "ID");
				ResyncTags();
				_text = XHelper.GetChildBase64String(input, "Text");
				_internaltitle = XHelper.GetChildValueString(input, "Title");
				_creationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_modificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_contentVersion = XHelper.GetChildValueStringOrDefault(input, "ContentVersion", "?");
				_authHash = XHelper.GetChildValueStringOrDefault(input, "AuthHash", "?");
				_isPinned = XHelper.GetChildValue(input, "IsPinned", false);
				_isLocked = XHelper.GetChildValue(input, "IsLocked", false);

				var intref = XHelper.GetChildOrNull(input, "InternalReferences")?.Elements("Ref").Select(x => new StandardFileRef {UUID = XHelper.GetAttributeGuid(x, "UUID"), Type = XHelper.GetAttributeString(x, "Type")}).ToList();
				if (intref != null) _internalRef.Synchronize(intref);
			}
		}

		private void TagsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_ignoreTagsChanged) return;
			
			// Ignore items that are new and old (they were only moved)
			var inew = e.NewItems?.Cast<string>().Except(e.OldItems?.Cast<string>() ?? Enumerable.Empty<string>()).ToList() ?? new List<string>();
			var iold = e.OldItems?.Cast<string>().Except(e.NewItems?.Cast<string>() ?? Enumerable.Empty<string>()).ToList() ?? new List<string>();

			foreach (var item in iold)
			{
				_internalTags.RemoveAll(it => it.Title == item);
			}

			foreach (var item in inew)
			{
				if (_internalTags.All(it => it.Title != item))
					_internalTags.Add(new StandardFileTagRef(null, item));
			}

			if (e.Action == NotifyCollectionChangedAction.Move)
			{
				// keep order
				_internalTags.Sort((a,b) => Tags.IndexOf(a.Title) - Tags.IndexOf(b.Title));
			}

			if (!_internalTags.Select(t => t.Title).UnorderedCollectionEquals(Tags.Select(t=>t)))
			{
				Debugger.Break();
				AlephAppContext.Logger.Error(StandardNotePlugin.Name, "Assertion failed (Invalid Tag state)", 
					$"Action        := {e.Action}\n"+
					$"inew          := [{string.Join(", ", inew.Select(p => $"'{p}'"))}]\n"+
					$"iold          := [{string.Join(", ", iold.Select(p => $"'{p}'"))}]\n"+
					$"_internalTags := [{string.Join(", ", _internalTags.Select(p => $"'{p}'"))}]\n"+
					$"Tags          := [{string.Join(", ", Tags.Select(p => $"'{p}'"))}]");
			}
		}

		public void UpgradeTag(StandardFileTagRef told, StandardFileTagRef tnew)
		{
			int idx = _internalTags.IndexOf(told);
			_internalTags[idx] = tnew;
		}

		public void SetTags(IEnumerable<StandardFileTagRef> newtags)
		{
			_internalTags = newtags.ToList();
			ResyncTags();
		}

		public void AddTag(StandardFileTagRef newtag)
		{
			_internalTags.Add(newtag);
			ResyncTags();
		}

		public void RemoveTag(StandardFileTagRef newtag)
		{
			_internalTags.Remove(newtag);
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

		public override string UniqueName => _id.ToString("N");

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
				_isPinned = other._isPinned;
				_isLocked = other._isLocked;
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
				_internaltitle = other.InternalTitle;
				_contentVersion = other.ContentVersion;
				_authHash = other.AuthHash;
				_internalRef.Synchronize(other._internalRef);
				_isPinned = other._isPinned;
				_isLocked = other._isLocked;
			}
		}

		public bool EqualsIgnoreModificationdate(StandardFileNote other)
		{
			if (_id != other._id) return false;
			if (_creationDate != other._creationDate) return false;
			if (!new HashSet<StandardFileTagRef>(_internalTags).SetEquals(other._internalTags)) return false;
			if (_text != other._text) return false;
			if (_internaltitle != other._internaltitle) return false;
			if (_isPinned != other._isPinned) return false;
			if (_isLocked != other._isLocked) return false;

			return true;
		}

		protected override BasicNoteImpl CreateClone()
		{
			var n = new StandardFileNote(_id, _config, _hConfig);

			using (n.SuppressDirtyChanges())
			{
				n._internalTags     = _internalTags.ToList();
				n.ResyncTags();
				n._text             = _text;
				n._internaltitle    = _internaltitle;
				n._creationDate     = _creationDate;
				n._modificationDate = _modificationDate;
				n._contentVersion   = _contentVersion;
				n._authHash         = _authHash;
				n._internalRef.Synchronize(_internalRef);
				n._isPinned         = _isPinned;
				n._isLocked         = _isLocked;
				
				return n;
			}
		}

		public bool ContainsTag(Guid tagID) => _internalTags.Any(t => t.UUID == tagID);
	}
}
