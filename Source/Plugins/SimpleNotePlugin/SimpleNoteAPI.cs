using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using AlephNote.PluginInterface.Exceptions;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.SimpleNote
{
	/// <summary>
	/// https://simperium.com/docs/reference/http/
	/// </summary>
	static class SimpleNoteAPI
	{
		private static readonly DateTimeOffset TIMESTAMP_ORIGIN = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

#pragma warning disable 0649
// ReSharper disable All
		public class APIResultAuthorize { public string username, access_token, userid; }
		public class APIResultIndex { public string current, mark; public List<APIResultIndexObj> index = new List<APIResultIndexObj>(); }
		public class APIResultIndexObj { public string id; public int v; }
		public class APIResultNoteData { public List<string> tags = new List<string>(); public bool deleted; public string shareURL, content, publishURL; public List<string> systemTags = new List<string>(); public double modificationDate, creationDate; }
		public class APISendNoteData { public List<string> tags = new List<string>(); public string content; public double modificationDate; public List<string> systemTags = new List<string>(); }
		public class APIDeleteNoteData { public bool deleted; }
		public class APISendAuth { public string username, password; }
		public class APIBadRequest { public string field, message; }

		// ReSharper restore All
#pragma warning restore 0649

		public static IAlephLogger Logger = new AlephDummyLoggger();

		public static APIResultAuthorize Authenticate(ISimpleJsonRest web, string userName, string password)
		{
			try
			{
				return web.PostTwoWay<APIResultAuthorize>(new APISendAuth { username = userName, password = password }, "authorize/");
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		public static APIResultIndex ListBuckets(ISimpleJsonRest web)
		{
			try
			{
				var idx = web.Get<APIResultIndex>("note/index");

				while (!string.IsNullOrWhiteSpace(idx.mark))
				{
					var idx2 = web.Get<APIResultIndex>("note/index", "mark=" + idx.mark);

					//idx.current = idx2.current;
					idx.mark = idx2.mark;
					idx.index.AddRange(idx2.index);
				}

				return idx;
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		public static SimpleNote GetNoteData(ISimpleJsonRest web, string noteID, SimpleNoteConfig cfg, SimpleNoteConnection conn, int? version = null)
		{
			try
			{
				if (version != null)
					return GetNoteFromQuery(web.Get<APIResultNoteData>(string.Format("note/i/{0}/v/{1}", noteID, version)), web, noteID, cfg, conn);
				else
					return GetNoteFromQuery(web.Get<APIResultNoteData>("note/i/" + noteID), web, noteID, cfg, conn);
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		public static SimpleNote UploadNewNote(ISimpleJsonRest web, SimpleNote note, SimpleNoteConfig cfg, SimpleNoteConnection conn)
		{
			note.Deleted = false;
			note.CreationDate = DateTimeOffset.Now;
			note.ModificationDate = DateTimeOffset.Now;
			
			APIResultNoteData data = new APIResultNoteData
			{
				tags = note.Tags.ToList(),
				deleted = false,
				shareURL = note.ShareURL,
				publishURL = note.PublicURL,
				systemTags = note.SystemTags,
				content = note.Content,
				creationDate = ConvertToEpochDate(note.CreationDate),
				modificationDate = ConvertToEpochDate(note.ModificationDate),
			};

			try
			{
				var r = web.PostTwoWay<APIResultNoteData>(data, "note/i/" + note.ID, "response=1");

				return GetNoteFromQuery(r, web, note.ID, cfg, conn);
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		public static SimpleNote ChangeExistingNote(ISimpleJsonRest web, SimpleNote note, SimpleNoteConfig cfg, SimpleNoteConnection conn, out bool updated)
		{
			if (note.Deleted) throw new SimpleNoteAPIException("Cannot update an already deleted note");
			if (note.ID == "") throw new SimpleNoteAPIException("Cannot change a not uploaded note");
			note.ModificationDate = DateTimeOffset.Now;
			
			APISendNoteData data = new APISendNoteData
			{
				tags = note.Tags.ToList(),
				content = note.Content,
				modificationDate = ConvertToEpochDate(note.ModificationDate),
				systemTags = note.SystemTags.ToList(),
			};

			try
			{
				var r = web.PostTwoWay<APIResultNoteData>(data, "note/i/" + note.ID, new[] { 412 }, "response=1");

				if (r == null)
				{
					// Statuscode 412 - Empty change

					updated = false;
					return (SimpleNote)note.Clone();
				}

				updated = true;
				return GetNoteFromQuery(r, web, note.ID, cfg, conn);
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		public static void DeleteNotePermanently(ISimpleJsonRest web, SimpleNote note)
		{
			if (note.ID == "") throw new SimpleNoteAPIException("Cannot delete a not uploaded note");

			try
			{
				note.ModificationDate = DateTimeOffset.Now;
				web.DeleteEmpty("note/i/" + note.ID);
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		public static void DeleteNote(ISimpleJsonRest web, SimpleNote note)
		{
			if (note.ID == "") throw new SimpleNoteAPIException("Cannot delete a not uploaded note");
			note.ModificationDate = DateTimeOffset.Now;
			
			APIDeleteNoteData data = new APIDeleteNoteData
			{
				deleted = true
			};

			try
			{
				web.PostUpload(data, "note/i/" + note.ID, new[] { 412 });
			}
			catch (RestStatuscodeException e1)
			{
				if (e1.StatusCode == 400 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
				{
					var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
					if (req != null) throw new SimpleNoteAPIException($"Server returned status 400.\nField: '{req.field}'.\nMessage: '{req.message}'", e1);
				}

				throw;
			}
		}

		private static SimpleNote GetNoteFromQuery(APIResultNoteData r, ISimpleJsonRest c, string id, SimpleNoteConfig cfg, SimpleNoteConnection conn)
		{
			try
			{
				var n = new SimpleNote(id, cfg, conn.HConfig)
				{
					Deleted = r.deleted,
					ShareURL = r.shareURL,
					PublicURL = r.publishURL,
					SystemTags = r.systemTags,
					Content = r.content,
					ModificationDate = ConvertFromEpochDate(r.modificationDate),
					CreationDate = ConvertFromEpochDate(r.creationDate),
					LocalVersion = int.Parse(c.GetResponseHeader("X-Simperium-Version")),
				};

				n.Tags.Synchronize(r.tags);

				return n;
			}
			catch (Exception e)
			{
				throw new SimpleNoteAPIException("SimpleNote API returned unexpected note data", e);
			}
		}

		private static DateTimeOffset ConvertFromEpochDate(double seconds)
		{
			const double DTO_MAX = 10.0 * 1000 * 1000 * 1000;

			if (seconds <= 0)               { Logger.Warn(SimpleNotePlugin.Name, "ConvertFromEpochDate with invalid value (<=0)",        $"seconds: {seconds}"); return TIMESTAMP_ORIGIN; }
			if (double.IsNaN(seconds))      { Logger.Warn(SimpleNotePlugin.Name, "ConvertFromEpochDate with invalid value (IsNaN)",      $"seconds: {seconds}"); return TIMESTAMP_ORIGIN; }
			if (double.IsInfinity(seconds)) { Logger.Warn(SimpleNotePlugin.Name, "ConvertFromEpochDate with invalid value (IsInfinity)", $"seconds: {seconds}"); return TIMESTAMP_ORIGIN; }
			if (seconds > DTO_MAX)          { Logger.Warn(SimpleNotePlugin.Name, "ConvertFromEpochDate with invalid value (>Max)",       $"seconds: {seconds}"); return TIMESTAMP_ORIGIN; }

			return TIMESTAMP_ORIGIN.AddSeconds(seconds);
		}

		private static double ConvertToEpochDate(DateTimeOffset offset)
		{
			return offset.DateTime.ToUniversalTime().Subtract(TIMESTAMP_ORIGIN.DateTime).TotalSeconds;
		}
	}
}
