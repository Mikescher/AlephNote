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
using System.Globalization;
using System.Text;

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

		private DateTimeOffset _creationDate; // The "real" CreationDate of the StandardNotes API
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		// The "real" ModificationDate of the StandardNotes API - cannot be chaned by us mere mortals and is used for sync
		// Is also _not_ changed if we do client changes - only updates _after_ an sync
		private DateTimeOffset _rawModificationDate = DateTimeOffset.Now;
		public DateTimeOffset RawModificationDate { get { return _rawModificationDate; } set { _rawModificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset? _clientUpdatedAt; // Additional ModificationDate used by some official StandardNotes clients (not 100% sure why)
		public DateTimeOffset? ClientUpdatedAt { get { return _clientUpdatedAt; } set { _clientUpdatedAt = value; OnPropertyChanged(); } }

        #region Custom fields

        private DateTimeOffset? _noteCreationDate = null; // custom AlephNote field: real date when note was created
		public DateTimeOffset? NoteCreationDate { get { return _noteCreationDate; } set { _noteCreationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset? _noteModificationDate = null; // custom AlephNote field: real date when note was last changed
		public DateTimeOffset? NoteModificationDate { get { return _noteModificationDate; } set { _noteModificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset? _textModificationDate = null; // custom AlephNote field: real date when note-text was last changed
		public DateTimeOffset? TextModificationDate { get { return _textModificationDate; } set { _textModificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset? _titleModificationDate = null; // custom AlephNote field: real date when note-title was last changed
		public DateTimeOffset? TitleModificationDate { get { return _titleModificationDate; } set { _titleModificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset? _tagsModificationDate = null; // custom AlephNote field: real date when note-tags were last changed
		public DateTimeOffset? TagsModificationDate { get { return _tagsModificationDate; } set { _tagsModificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset? _pathModificationDate = null; // custom AlephNote field: real date when note-path was last changed
		public DateTimeOffset? PathModificationDate { get { return _pathModificationDate; } set { _pathModificationDate = value; OnPropertyChanged(); } }

		#endregion

		public override DateTimeOffset ModificationDate 
		{ 
			get 
			{
				if (_config.ModificationDateSource == MDateSource.RawServer)
                {
					return _rawModificationDate;
				}
				else if (_config.ModificationDateSource == MDateSource.Metadata)
				{
					if (_clientUpdatedAt != null) return _clientUpdatedAt.Value;
					return _rawModificationDate;
				}
				else if (_config.ModificationDateSource == MDateSource.Intelligent)
				{
					if (_noteModificationDate != null) return _noteModificationDate.Value;
					if (_clientUpdatedAt != null) return _clientUpdatedAt.Value;
					return _rawModificationDate;
				}
				else if (_config.ModificationDateSource == MDateSource.IntelligentContent)
				{
					if (_textModificationDate != null || _titleModificationDate != null) return Max(_textModificationDate, _titleModificationDate) ?? _rawModificationDate;
					if (_noteModificationDate != null) return _noteModificationDate.Value;
					if (_clientUpdatedAt != null) return _clientUpdatedAt.Value;
					return _rawModificationDate;
				}
				else
                {
					throw new Exception("Invalid value for ModificationDateSource");
                }
			}
		}

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

		private bool _isArchived = false;
		public bool IsArchived { get { return _isArchived; } set { _isArchived = value; OnPropertyChanged(); } }

		private bool _isProtected = false;
		public bool IsProtected { get { return _isProtected; } set { _isProtected = value; OnPropertyChanged(); } }

		private bool _isHidePreview = false;
		public bool IsHidePreview { get { return _isHidePreview; } set { _isHidePreview = value; OnPropertyChanged(); } }

		private string _rawAppData = null;
		public string RawAppData { get { return _rawAppData; } set { _rawAppData = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<StandardFileRef> _internalRef = new ObservableCollection<StandardFileRef>();
		public ObservableCollection<StandardFileRef> InternalReferences { get { return _internalRef; } }

		private bool _ignoreTagsChanged = false;
		private readonly StandardNoteConfig _config;

		public StandardFileNote(Guid uid, StandardNoteConfig cfg, HierarchyEmulationConfig hcfg)
			: base(hcfg)
		{
			_id = uid;
			_config = cfg;
			_creationDate = DateTimeOffset.Now;

			_tags.OnChanged += TagsChanged;
		}

		public override string DateTooltip
        {
			get
			{
				var sb = new StringBuilder();

				if ($"{ModificationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}" == $"{RawModificationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}")
				{
					sb.AppendLine($"Modified: {ModificationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
				}
				else
				{
					// Our mdate and the "official" mdate differ
					// Can happen on re-syncs where StandardNotes updates the modificationtime (even though nothing changed)
					sb.AppendLine($"Modified: {ModificationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss} ({RawModificationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss})");
				}

				if (NoteCreationDate == null || $"{CreationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}" == $"{NoteCreationDate.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}")
				{
					sb.AppendLine($"Created: {CreationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
				}
				else
				{
					// Our mdate and the "official" mdate differ
					// Can happen on re-syncs where StandardNotes updates the modificationtime (even though nothing changed)
					sb.AppendLine($"Created: {NoteCreationDate.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss} ({CreationDate.ToLocalTime():yyyy-MM-dd HH:mm:ss})");
				}


				if (TextModificationDate  != null) sb.AppendLine($"Modified (content): {TextModificationDate.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
				if (TitleModificationDate != null) sb.AppendLine($"Modified (title): {TitleModificationDate.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
				if (TagsModificationDate  != null) sb.AppendLine($"Modified (tags): {TagsModificationDate.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
				if (PathModificationDate  != null) sb.AppendLine($"Modified (path): {PathModificationDate.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}");

				return sb.ToString().TrimEnd();
			}
        }

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Tags", _internalTags.Select(t => t.Serialize()).Cast<object>().ToArray()),
				new XElement("Text", XHelper.ConvertToC80Base64(_text)),
				new XElement("Title", _internaltitle),
				new XElement("__RealModificationDate", XHelper.ToString(ModificationDate)),
				new XElement("ModificationDate", XHelper.ToString(RawModificationDate)), // is RawModificationDate but xml tag is still <ModificationDate> for compatibiility
				CreateNullableDateTimeXElem("NoteCreationDate",      NoteCreationDate),
				CreateNullableDateTimeXElem("NoteModificationDate",  NoteModificationDate),
				CreateNullableDateTimeXElem("TextModificationDate",  TextModificationDate),
				CreateNullableDateTimeXElem("TitleModificationDate", TitleModificationDate),
				CreateNullableDateTimeXElem("TagsModificationDate",  TagsModificationDate),
				CreateNullableDateTimeXElem("PathModificationDate",  PathModificationDate),
				CreateNullableDateTimeXElem("ClientUpdatedAt",       ClientUpdatedAt),
				new XElement("CreationDate", XHelper.ToString(_creationDate)),
				new XElement("ContentVersion", _contentVersion),
				new XElement("AuthHash", _authHash),
				new XElement("InternalReferences", _internalRef.Select(ir => new XElement("Ref", new XAttribute("Type", ir.Type), new XAttribute("UUID", ir.UUID.ToString("P").ToUpper())))),
				new XElement("IsPinned", _isPinned),
				new XElement("IsLocked", _isLocked),
				new XElement("IsArchived", _isArchived),
				new XElement("IsProtected", _isProtected),
				new XElement("IsHidePreview", _isHidePreview),
				new XElement("RawAppData", _rawAppData),
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
				_text                  = XHelper.GetChildBase64String(input, "Text");
				_internaltitle         = XHelper.GetChildValueString(input, "Title");
				_creationDate          = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
				_rawModificationDate   = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
				_noteCreationDate      = GetChildValueNullableDateTimeOffset(input, "NoteCreationDate");
				_noteModificationDate  = GetChildValueNullableDateTimeOffset(input, "NoteModificationDate");
				_textModificationDate  = GetChildValueNullableDateTimeOffset(input, "TextModificationDate");
				_titleModificationDate = GetChildValueNullableDateTimeOffset(input, "TitleModificationDate");
				_tagsModificationDate  = GetChildValueNullableDateTimeOffset(input, "TagsModificationDate");
				_pathModificationDate  = GetChildValueNullableDateTimeOffset(input, "PathModificationDate");
				_clientUpdatedAt       = GetChildValueNullableDateTimeOffset(input, "ClientUpdatedAt");
				_contentVersion        = XHelper.GetChildValueStringOrDefault(input, "ContentVersion", "?");
				_authHash              = XHelper.GetChildValueStringOrDefault(input, "AuthHash", "?");
				_isPinned              = XHelper.GetChildValue(input, "IsPinned",      false);
				_isLocked              = XHelper.GetChildValue(input, "IsLocked",      false);
				_isArchived            = XHelper.GetChildValue(input, "IsArchived",    false);
				_isProtected           = XHelper.GetChildValue(input, "IsProtected",   false);
				_isHidePreview         = XHelper.GetChildValue(input, "IsHidePreview", false);
				_rawAppData            = XHelper.GetChildValue(input, "RawAppData", "");

				var intref = XHelper.GetChildOrNull(input, "InternalReferences")?.Elements("Ref").Select(x => new StandardFileRef {UUID = XHelper.GetAttributeGuid(x, "UUID"), Type = XHelper.GetAttributeString(x, "Type")}).ToList();
				if (intref != null) _internalRef.Synchronize(intref);

				AddPathToInternalTags();
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

			AddPathToInternalTags();

			if (!_internalTags.Where(p => !IsPathInternalTag(p)).Select(t => t.Title).UnorderedCollectionEquals(Tags.Select(t=>t)))
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
			AddPathToInternalTags();

			try
			{
				_ignoreTagsChanged = true;
				_tags.Synchronize(_internalTags.Where(p => !IsPathInternalTag(p)).Select(it => it.Title));
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
				_rawModificationDate   = other.RawModificationDate;
				_noteCreationDate      = other.NoteCreationDate;
				_noteModificationDate  = other.NoteModificationDate;
				_textModificationDate  = other.TextModificationDate;
				_titleModificationDate = other.TitleModificationDate;
				_tagsModificationDate  = other.TagsModificationDate;
				_pathModificationDate  = other.PathModificationDate;
				_clientUpdatedAt       = other.ClientUpdatedAt;
				_creationDate          = other.CreationDate;

				_internalTags          = other._internalTags;
				ResyncTags();

				_internalRef.Synchronize(other._internalRef);

				_isPinned              = other.IsPinned;
				_isLocked              = other.IsLocked;
				_isArchived            = other.IsArchived;
				_isProtected           = other.IsProtected;
				_isHidePreview         = other.IsHidePreview;

				_rawAppData            = other.RawAppData;
			}
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (StandardFileNote)iother;

			using (SuppressDirtyChanges())
			{
				_rawModificationDate   = other.RawModificationDate;
				_noteModificationDate  = other.NoteModificationDate;
				_textModificationDate  = other.TextModificationDate;
				_titleModificationDate = other.TitleModificationDate;
				_tagsModificationDate  = other.TagsModificationDate;
				_pathModificationDate  = other.PathModificationDate;
				_clientUpdatedAt       = other.ClientUpdatedAt;
				_creationDate          = other.CreationDate;

				_internalTags          = other._internalTags.ToList();
				ResyncTags();

				_text                  = other.Text;
				_internaltitle         = other.InternalTitle;
				_contentVersion        = other.ContentVersion;
				_authHash              = other.AuthHash;

				_internalRef.Synchronize(other._internalRef);

				_isPinned              = other.IsPinned;
				_isLocked              = other.IsLocked;
				_isArchived			   = other.IsArchived;
				_isProtected		   = other.IsProtected;
				_isHidePreview		   = other.IsHidePreview;

				_rawAppData            = other.RawAppData;
			}
		}

		public bool NoteDataEquals(StandardFileNote other)
		{
			if (_id                    != other._id)                    return false;

			if (_rawModificationDate   != other._rawModificationDate)   return false;
			if (_noteCreationDate      != other._noteCreationDate)      return false;
			if (_noteModificationDate  != other._noteModificationDate)  return false;
			if (_textModificationDate  != other._textModificationDate)  return false;
			if (_titleModificationDate != other._titleModificationDate) return false;
			if (_tagsModificationDate  != other._tagsModificationDate)  return false;
			if (_pathModificationDate  != other._pathModificationDate)  return false;
			if (_clientUpdatedAt       != other._clientUpdatedAt)       return false;
			if (_creationDate          != other._creationDate)          return false;

			if (!new HashSet<StandardFileTagRef>(_internalTags).SetEquals(other._internalTags)) return false;

			if (_text          != other._text)          return false;
			if (_internaltitle != other._internaltitle) return false;

			if (_isPinned      != other._isPinned)      return false;
			if (_isLocked      != other._isLocked)      return false;
			if (_isArchived    != other._isArchived	)   return false;
			if (_isProtected   != other._isProtected)   return false;
			if (_isHidePreview != other._isHidePreview) return false;

			if (_rawAppData    != other._rawAppData)    return false;

			return true;
		}

		public override void UpdateModificationDate(string propSource, bool clearConflictFlag)
		{
			var dtnow = DateTimeOffset.Now;

			//RawModificationDate  = dtnow; // must only be updated from server (as sync result)
			ClientUpdatedAt      = dtnow;
			NoteModificationDate = dtnow;

			if (propSource == "Title") TitleModificationDate = dtnow;
			if (propSource == "Text")  TextModificationDate  = dtnow;
			if (propSource == "Tags")  TagsModificationDate  = dtnow;
			if (propSource == "Path")  PathModificationDate  = dtnow;

			OnExplicitPropertyChanged(nameof(ModificationDate));
			OnExplicitPropertyChanged(nameof(DateTooltip));

			if (clearConflictFlag && IsConflictNote) IsConflictNote = false;
		}

		protected override BasicNoteImpl CreateClone()
		{
			var n = new StandardFileNote(_id, _config, _hConfig);

			using (n.SuppressDirtyChanges())
			{
				n._internalTags          = _internalTags.ToList();
				n.ResyncTags();

				n._text                  = _text;
				n._internaltitle         = _internaltitle;

				n._rawModificationDate	 = _rawModificationDate;
				n._noteCreationDate      = _noteCreationDate;
				n._noteModificationDate	 = _noteModificationDate;
				n._textModificationDate	 = _textModificationDate;
				n._titleModificationDate = _titleModificationDate;
				n._tagsModificationDate	 = _tagsModificationDate;
				n._pathModificationDate	 = _pathModificationDate;
				n._clientUpdatedAt		 = _clientUpdatedAt;
				n._creationDate			 = _creationDate;

				n._contentVersion        = _contentVersion;
				n._authHash              = _authHash;

				n._internalRef.Synchronize(_internalRef);

				n._isPinned              = _isPinned;
				n._isLocked              = _isLocked;
				n._isArchived            = _isArchived;
				n._isProtected           = _isProtected;
				n._isHidePreview         = _isHidePreview;

				n._rawAppData            = _rawAppData;

				return n;
			}
		}

		public bool ContainsTag(Guid tagID) => _internalTags.Any(t => t.UUID == tagID);

		private void AddPathToInternalTags()
		{
			if (!_config.CreateHierarchyTags) return;
			if (!_hConfig.EmulateSubfolders) return;

			var tag = "[Notes]";
			if (!_internalTags.Any(p => p.Title == tag)) _internalTags.Insert(0, new StandardFileTagRef(null, tag));
			int i = 1;
            foreach (var comp in Path.Enumerate())
            {
				tag += "."+comp;
				if (!_internalTags.Any(p => p.Title == tag)) _internalTags.Insert(i, new StandardFileTagRef(null, tag));
				i++;
			}

		}

		private bool IsPathInternalTag(StandardFileTagRef tref)
        {
			return tref != null && tref.Title.StartsWith("[Notes].") || tref.Title == "[Notes]";
		}

		public XElement CreateNullableDateTimeXElem(string name, DateTimeOffset? value) //TODO mig to CSharpUtils
        {
			if (value != null) 
				return new XElement(name, value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture), new XAttribute("null", "false"));
			else
				return new XElement(name, string.Empty, new XAttribute("null", "true"));
		}

		private DateTimeOffset? GetChildValueNullableDateTimeOffset(XElement parent, string childName) //TODO mig to CSharpUtils
		{
			var child = parent.Elements(childName).FirstOrDefault();
			if (child == null) return null;

			if (child.Attribute("null").Value == "true") return null;

			return DateTimeOffset.Parse(child.Value, CultureInfo.InvariantCulture);
		}

		private DateTimeOffset? Max(DateTimeOffset? da, DateTimeOffset? db) //TODO mig to CSharpUtils
		{
			if (da == null || db == null) return da ?? db;

			return (da.Value > db.Value) ? da.Value : da.Value;
		}
	}
}
