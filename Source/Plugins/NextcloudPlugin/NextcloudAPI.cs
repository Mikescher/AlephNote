using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Nextcloud
{
	public static class NextcloudAPI
	{
		private static readonly DateTimeOffset TIMESTAMP_ORIGIN = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

#pragma warning disable 0649
// ReSharper disable All
		public class NoteRef { public int id; public int modified; }
		public class ApiCreateNote { public string content, category; }
		public class ApiUpdateNote { public string content, category; }
		public class ApiNoteResult { public int id; public string content, category, title, etag; public int modified; public bool favorite; }
// ReSharper restore All
#pragma warning restore 0649

		public static List<NoteRef> ListNotes(ISimpleJsonRest web)
		{
			return web.Get<List<NoteRef>>("notes", "exclude=title,category,content,favorite");
		}

		public static NextcloudNote UploadNewNote(ISimpleJsonRest web, NextcloudNote note, NextcloudConfig config)
		{
			var data = new ApiCreateNote { content = note.Content, category = CreateCategoryFromPath(note.Path) };
			var result = web.PostTwoWay<ApiNoteResult>(data, "notes");

			var rnote = new NextcloudNote(result.id, note.LocalID, config)
			{
				CreationDate = DateTime.Now,
				RemoteTimestamp = result.modified,
				Content = result.content,
				Path = ExtractPathFromCategory(result.category),
				ModificationDate = ConvertFromEpochDate(result.modified),
				ETag = result.etag,
			};

			if (rnote.Title.ToLower() != result.title.ToLower())
			{
				rnote.Title = result.title;
			}
			return rnote;
		}

		public static NextcloudNote GetNoteData(ISimpleJsonRest web, int id, NextcloudConfig config)
		{
			var result = web.Get<ApiNoteResult>("notes/" + id);

			return new NextcloudNote(result.id, Guid.NewGuid(), config)
			{
				CreationDate = DateTime.Now,
				RemoteTimestamp = result.modified,
				Content = result.content,
				Favorite = result.favorite,
				Path = ExtractPathFromCategory(result.category),
				ModificationDate = ConvertFromEpochDate(result.modified),
				ETag = result.etag,
			};
		}

		public static NextcloudNote ChangeExistingNote(ISimpleJsonRest web, NextcloudNote note, NextcloudConfig config)
		{
			var data = new ApiUpdateNote { content = note.Content, category = CreateCategoryFromPath(note.Path) };
			var result = web.PutTwoWay<ApiNoteResult>(data, "notes/" + note.RemoteID);

			var rnote = new NextcloudNote(result.id, note.LocalID, config)
			{
				CreationDate = DateTime.Now,
				RemoteTimestamp = result.modified,
				Content = result.content,
				Favorite = result.favorite,
				Path = ExtractPathFromCategory(result.category),
				ModificationDate = ConvertFromEpochDate(result.modified),
				ETag = result.etag,
			};

			if (rnote.Title.ToLower() != result.title.ToLower())
			{
				rnote.Title = result.title;
			}
			return rnote;
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

		private static DirectoryPath ExtractPathFromCategory(string category)
		{
			if (string.IsNullOrWhiteSpace(category)) return DirectoryPath.Root();
			return DirectoryPath.Create(category.Split('/'));
		}

		private static string CreateCategoryFromPath(DirectoryPath path)
		{
			return string.Join(@"/", path.Enumerate());
		}
	}
}
