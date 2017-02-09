using MSHC.Lang.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CommonNote.Plugins.SimpleNote
{
	/// <summary>
	/// https://simperium.com/docs/reference/http/
	/// </summary>
	static class SimpleNoteAPI
	{
		private const string API_KEY = "6ebfbdf6bfa8423e85d8733f6b6bbc25";
		private const string APP_ID = "chalk-bump-f49";

		private static readonly DateTimeOffset TIMESTAMP_ORIGIN = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0));

#pragma warning disable 0649
		// ReSharper disable All
		public class APIResultAuthorize { public string username, access_token, userid; }
		public class APIResultIndex { public string current, mark; public List<APIResultIndexObj> index = new List<APIResultIndexObj>(); }
		public class APIResultIndexObj { public string id; public int v; }
		public class APIResultNoteData { public List<string> tags = new List<string>(); public bool deleted; public string shareURL, content, publishURL; public List<string> systemTags = new List<string>(); public double modificationDate, creationDate; }
		// ReSharper restore All
#pragma warning restore 0649

		private static WebClient CreateClient(IWebProxy proxy, string authToken)
		{
			var web = new WebClient();
			if (proxy != null) web.Proxy = proxy;
			web.Headers["User-Agent"] = "CommonNote/1.0.0.0";
			web.Headers["X-Simperium-API-Key"] = API_KEY;
			if (authToken != null) web.Headers["X-Simperium-Token"] = authToken;
			return web;
		}

		public static APIResultAuthorize Authenticate(IWebProxy proxy, string userName, string password)
		{
			var uri = new Uri(string.Format("https://auth.simperium.com/1/{0}/authorize/", APP_ID));
			var content = string.Format("{{\"username\":\"{0}\", \"password\":\"{1}\"}}", userName, password);

			var value = CreateClient(proxy, null).UploadString(uri, content);

			return JsonConvert.DeserializeObject<APIResultAuthorize>(value);
		}

		public static APIResultIndex ListBuckets(IWebProxy proxy, string authToken)
		{
			var web = CreateClient(proxy, authToken);

			var value = web.DownloadString(string.Format("https://api.simperium.com/1/{0}/note/index", APP_ID));
			APIResultIndex idx = JsonConvert.DeserializeObject<APIResultIndex>(value);

			while (!string.IsNullOrWhiteSpace(idx.mark))
			{
				var value2 = web.DownloadString(string.Format("https://api.simperium.com/1/{0}/note/index?mark={1}", APP_ID, idx.mark));
				APIResultIndex idx2 = JsonConvert.DeserializeObject<APIResultIndex>(value2);

				idx.current = idx2.current;
				idx.mark = idx2.mark;
				idx.index.AddRange(idx2.index);
			}

			return idx;
		}

		public static SimpleNote GetNoteData(IWebProxy proxy, string authToken, string nodeID, int? version = null)
		{
			var web = CreateClient(proxy, authToken);

			var url = string.Format("https://api.simperium.com/1/{0}/note/i/{1}", APP_ID, nodeID);
			if (version != null) url += "/v/" + version;

			var value = web.DownloadString(url);
			var r = JsonConvert.DeserializeObject<APIResultNoteData>(value);

			return GetNoteFromQuery(r, web, nodeID);
		}

		public static SimpleNote UploadNote(IWebProxy proxy, string authToken, SimpleNote note)
		{
			var web = CreateClient(proxy, authToken);

			note.Deleted = false;
			note.CreationDate = DateTimeOffset.Now;
			note.ModificationDate = DateTimeOffset.Now;

			var uri = new Uri(string.Format("https://api.simperium.com/1/{0}/note/i/{1}?response=1", APP_ID, note.ID));

			APIResultNoteData data = new APIResultNoteData
			{
				tags = note.Tags.ToList(),
				deleted = false,
				shareURL = note.ShareURL,
				publishURL = note.PublishURL,
				systemTags = note.SystemTags,
				content = note.Content,
				creationDate = ConvertToEpochDate(note.CreationDate),
				modificationDate = ConvertToEpochDate(note.ModificationDate),
			};

			var rawdata = JsonConvert.SerializeObject(data);
			var value = web.UploadString(uri, rawdata);
			var r = JsonConvert.DeserializeObject<APIResultNoteData>(value);

			return GetNoteFromQuery(r, web, note.ID);
		}

		public static SimpleNote ChangeNote(IWebProxy proxy, string authToken, SimpleNote note)
		{
			var web = CreateClient(proxy, authToken);

			if (note.Deleted) throw new Exception("Cannot update an already deleted note");
			if (note.ID == "") throw new Exception("Cannot change a not uploaded note");
			note.ModificationDate = DateTimeOffset.Now;

			var uri = new Uri(string.Format("https://api.simperium.com/1/{0}/note/i/{1}?response=1", APP_ID, note.ID));

			APIResultNoteData data = new APIResultNoteData
			{
				tags = note.Tags.ToList(),
				deleted = false,
				shareURL = note.ShareURL,
				publishURL = note.PublishURL,
				systemTags = note.SystemTags,
				content = note.Content,
				creationDate = ConvertToEpochDate(note.CreationDate),
				modificationDate = ConvertToEpochDate(note.ModificationDate),
			};

			var rawdata = JsonConvert.SerializeObject(data);
			var value = web.UploadString(uri, rawdata);
			var r = JsonConvert.DeserializeObject<APIResultNoteData>(value);

			return GetNoteFromQuery(r, web, note.ID);
		}

		public static void DeleteNote(IWebProxy proxy, string authToken, SimpleNote note)
		{
			var web = CreateClient(proxy, authToken);

			if (note.ID == "") throw new Exception("Cannot delete a not uploaded note");
			note.ModificationDate = DateTimeOffset.Now;

			var uri = new Uri(string.Format("https://api.simperium.com/1/{0}/note/i/{1}", APP_ID, note.ID));
			
			web.UploadString(uri, "DELETE", string.Empty);
		}

		private static SimpleNote GetNoteFromQuery(APIResultNoteData r, WebClient c, string id)
		{
			var n = new SimpleNote(id)
			{
				Deleted = r.deleted,
				ShareURL = r.shareURL,
				PublishURL = r.publishURL,
				SystemTags = r.systemTags,
				Content = r.content,
				ModificationDate = ConvertFromEpochDate(r.modificationDate),
				CreationDate = ConvertFromEpochDate(r.creationDate),
				Version = int.Parse(c.ResponseHeaders["X-Simperium-Version"]),
			};

			n.Tags.Synchronize(r.tags);

			return n;
		}

		private static DateTimeOffset ConvertFromEpochDate(double seconds)
		{
			return TIMESTAMP_ORIGIN.AddSeconds(seconds);
		}

		private static double ConvertToEpochDate(DateTimeOffset offset)
		{
			return TimeZoneInfo.ConvertTimeToUtc(offset.DateTime, TimeZoneInfo.Local).ToUniversalTime().Subtract(TIMESTAMP_ORIGIN.DateTime).TotalSeconds;
		}
	}
}
