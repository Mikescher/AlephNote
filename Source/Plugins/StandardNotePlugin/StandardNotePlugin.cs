using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using MSHC.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNotePlugin : BasicRemotePlugin
	{
		public static readonly Version Version = GetInformationalVersion(typeof(StandardNotePlugin).GetTypeInfo().Assembly);
		public const string Name = "StandardNotePlugin";

		public override bool SupportsPinning                   => true;
		public override bool SupportsLocking                   => true;
		public override bool SupportsNativeDirectories         => false;
		public override bool SupportsTags                      => true;
		public override bool SupportsDownloadMultithreading    => false;
		public override bool SupportsNewDownloadMultithreading => false;
		public override bool SupportsUploadMultithreading      => false;

		public const string CURRENT_SCHEMA = "003";

		private AlephLogger _logger;

		public StandardNotePlugin() : base("Standard Notes", Version, Guid.Parse("30d867a4-cbdc-45c5-950a-c119bf2f2845"))
		{
			//
		}

        public override List<UICommand> DebugCommands => base.DebugCommands.Concat(new List<UICommand>
                {
                    new UICommand("Set custom creation dates (if unset) to server creation date", new RelayCommand<INoteRepository>((repo) =>
                    {
                        foreach (var n in repo.EnumerateNotes().Cast<StandardFileNote>())
                        {
                            if (n.NoteCreationDate == null) n.NoteCreationDate = n.ClientUpdatedAt ?? n.RawModificationDate;
                        }
                    })),

                    new UICommand("Set custom modification dates (if unset) to server modification date", new RelayCommand<INoteRepository>((repo) =>
                    {
                        foreach (var n in repo.EnumerateNotes().Cast<StandardFileNote>())
                        {
                            if (n.NoteModificationDate  == null) n.NoteModificationDate  = n.ClientUpdatedAt ?? n.RawModificationDate;
                            if (n.TextModificationDate  == null) n.TextModificationDate  = n.ClientUpdatedAt ?? n.RawModificationDate;
                            if (n.TitleModificationDate == null) n.TitleModificationDate = n.ClientUpdatedAt ?? n.RawModificationDate;
                            if (n.TagsModificationDate  == null) n.TagsModificationDate  = n.ClientUpdatedAt ?? n.RawModificationDate;
                            if (n.PathModificationDate  == null) n.PathModificationDate  = n.ClientUpdatedAt ?? n.RawModificationDate;
                        }
                    })),
                }).ToList();


        public override void Init(AlephLogger logger)
		{
			_logger = logger;
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new StandardNoteConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierarchyEmulationConfig hConfig)
		{
			return new StandardNoteConnection(_logger, proxy, (StandardNoteConfig)config, hConfig);
		}

		public override INote CreateEmptyNote(IRemoteStorageConnection iconn, IRemoteStorageConfiguration cfg)
		{
			var conn = (StandardNoteConnection)iconn;
			return new StandardFileNote(Guid.NewGuid(), (StandardNoteConfig) cfg, conn.HConfig) { ContentVersion = CURRENT_SCHEMA, NoteCreationDate = DateTimeOffset.Now };
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new StandardNoteData();
		}

		protected override IEnumerable<Tuple<string, string>> CreateHelpTexts()
		{
			yield return Tuple.Create("SendEncrypted", 
				"If checked the note content is encrypted locally before being send to the server."+"\n"+
				"No one can read your notes without your password, not even the server administrator.");

			yield return Tuple.Create("RemEmptyTags", 
				"If checked tags that are not linked with any note are deleted on the server.");

			yield return Tuple.Create("ModificationDateSource",
				"This setting specifies how the (locally used) modification date of notes is determined:" + "\n" +
				"" + "\n" +
				"### From Server ###" + "\n" +
				"Directly use the modification date that the StandardNotes server is sending to us." + "\n" +
				"This is the simplest version but sometimes can result in unwanted behavior. It can, for example, happen that StandardNotes resets the modification date on all notes after a failed synchronization so that all notes appear to be changed. And this way changes to the tags or the path also reset the date" + "\n" +
				"" + "\n" +
				"### From Metadata ###" + "\n" +
				"Use the client_updated_at value from the notes metadata." + "\n" +
				"This is a special tag that is read/set from various StandardNotes clients (primarily the official ones), in contrast to the raw modification date of the note (which is also used for internal synchronization purposes) this one has a lower change of being reset from the outside." + "\n" +
				"*But* this value is not under the exclusive control of AlephNote and other clients that sync with your repository can edit it." + "\n" +
				"" + "\n" +
				"### Intelligent ###" + "\n" +
				"Use a AlephNote specific appData value in the note to determine the modification date." + "\n" +
				"This property is managed only by AlephNote and no erroneous sync events can change it." + "\n" +
				"*But* this also means that only changes in and from AlephNote will update the modification date. You should only use this option if you (almost) exclusively use AlephNote with your notes." + "\n" +
				"" + "\n" +
				"### Intelligent (content changes only) ###" + "\n" +
				"This is almost the same as [Intelligent], but we only consider changes to the Note text and the Note title in our algorithm." + "\n" +
				"This means that tag changes and path changes (moving the note) will not update the modification date");

			yield return Tuple.Create("CreateHierarchyTags",
				"This setting automatically creates tags that correspondent with your folder hierarchy." + "\n" +
				"This way the folders extension of StandardNotes (Extended) works with the folder hierarchy in AlephNote." + "\n" +
				"" + "\n" +
				"Be aware that the note title will still contain the full hierarchy and it can result in problems if your folder names contain dots (because those are the separators in StandardNotes)");
		}
	}
}
