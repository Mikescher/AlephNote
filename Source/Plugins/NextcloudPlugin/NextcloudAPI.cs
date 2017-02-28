using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;

namespace AlephNote.Plugins.Nextcloud
{
	public static class NextcloudAPI
	{
		private static readonly DateTimeOffset TIMESTAMP_ORIGIN = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

#pragma warning disable 0649
// ReSharper disable All
		public class NoteRef { public int id; public int modified; }
		public class ApiCreateNote { public string content; }
		public class ApiUpdateNote { public string content; }
		public class ApiNoteResult { public int id; public string content, category; public int modified; public bool favorite; }
// ReSharper restore All
#pragma warning restore 0649

		public static List<NoteRef> ListNotes(ISimpleJsonRest web)
		{
			return web.Get<List<NoteRef>>("notes", "exclude=title,category,content,favorite");
		}

		public static NextcloudNote UploadNewNote(ISimpleJsonRest web, NextcloudNote note, NextcloudConfig config)
		{
			var data = new ApiCreateNote { content = note.Content };
			var result = web.PostTwoWay<ApiNoteResult>(data, "notes");

			return new NextcloudNote(result.id, note.LocalID, config)
			{
				CreationDate = DateTime.Now,
				RemoteTimestamp = result.modified,
				Content = result.content,
				ModificationDate = ConvertFromEpochDate(result.modified),
			};
		}

		public static NextcloudNote GetNoteData(ISimpleJsonRest web, int id, NextcloudConfig config)
		{
			var result = web.Get<ApiNoteResult>("notes/" + id);

			return new NextcloudNote(result.id, Guid.NewGuid(), config)
			{
				CreationDate = DateTime.Now,
				RemoteTimestamp = result.modified,
				Content = result.content,
				ModificationDate = ConvertFromEpochDate(result.modified),
			};
		}

		public static NextcloudNote ChangeExistingNote(ISimpleJsonRest web, NextcloudNote note, NextcloudConfig config)
		{
			var data = new ApiUpdateNote { content = note.Content };
			var result = web.PutTwoWay<ApiNoteResult>(data, "notes/" + note.RemoteID);

			return new NextcloudNote(result.id, note.LocalID, config)
			{
				CreationDate = DateTime.Now,
				RemoteTimestamp = result.modified,
				Content = result.content,
				ModificationDate = ConvertFromEpochDate(result.modified),
			};
		}

		public static void DeleteNote(ISimpleJsonRest web, NextcloudNote note)
		{
			web.DeleteEmpty("notes/" + note.RemoteID);
		}

		private static DateTimeOffset ConvertFromEpochDate(double seconds)
		{
			if (seconds <= 0) return TIMESTAMP_ORIGIN;

			return TIMESTAMP_ORIGIN.AddSeconds(seconds);
		}
	}
}
