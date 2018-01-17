using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using AlephNote.PluginInterface.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace AlephNote.Plugins.SimpleNote
{
	public class SimpleNotePlugin : BasicRemotePlugin
	{
		public static readonly Version Version = GetInformationalVersion(typeof(SimpleNotePlugin).GetTypeInfo().Assembly);
		public const string Name = "SimpleNotePlugin";

		private IAlephLogger _logger;

		public SimpleNotePlugin() : base("Simplenote", Version, Guid.Parse("4c73e687-3803-4078-9bf0-554aaafc0873"))
		{
			//
		}

		public override void Init(IAlephLogger logger)
		{
			_logger = logger;
			SimpleNoteAPI.Logger = logger;
		}

		public override IRemoteStorageConfiguration CreateEmptyRemoteStorageConfiguration()
		{
			return new SimpleNoteConfig();
		}

		public override IRemoteStorageConnection CreateRemoteStorageConnection(IWebProxy proxy, IRemoteStorageConfiguration config, HierachyEmulationConfig hConfig)
		{
			return new SimpleNoteConnection(_logger, proxy, (SimpleNoteConfig)config, hConfig);
		}

		public override INote CreateEmptyNote(IRemoteStorageConnection iconn, IRemoteStorageConfiguration cfg)
		{
			var conn = (SimpleNoteConnection)iconn;
			return new SimpleNote(Guid.NewGuid().ToString("N").ToUpper(), (SimpleNoteConfig)cfg, conn.HConfig);
		}

		public override IRemoteStorageSyncPersistance CreateEmptyRemoteSyncData()
		{
			return new SimpleNoteData();
		}

		public override IEnumerable<Tuple<string, string>> CreateHelpTexts()
		{
			yield return Tuple.Create("PermanentlyDeleteNotes", "SimpleNote can either 'really' delete notes on the server or only mark them as 'deleted'.\nIf this option is checked locally deleted notes are permanently deleted on the server.");
			yield return Tuple.Create("BlankLineBelowTitle", "SimpleNote does not really have the concept of a 'note title'.\nNormally the first line is used as a title/preview.\n\nAlephNote also uses the first line as an title,\nfor better formatting when the note is viewed plain (e.g. in SimpleNote web app) we insert a blank line between title and content by default.");
		}

		public override bool HasNativeDirectorySupport()
		{
			return false;
		}
	}
}
