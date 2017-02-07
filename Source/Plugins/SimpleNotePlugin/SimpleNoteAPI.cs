using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace CommonNote.Plugins.SimpleNote
{
	/// <summary>
	/// https://simperium.com/docs/reference/http/
	/// </summary>
	class SimpleNoteAPI
	{
		private const string API_KEY = "6ebfbdf6bfa8423e85d8733f6b6bbc25";
		private const string APP_ID = "chalk-bump-f49";

		private static readonly DateTimeOffset TIMESTAMP_ORIGIN = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0));

		public class APIResultAuthorize { public string username, access_token, userid; }
		public class APIResultIndex { public string current, mark; public List<APIResultIndexObj> index = new List<APIResultIndexObj>(); }
		public class APIResultIndexObj { public string id; public int v; }
		public class APIResultNoteData { public List<string> tags = new List<string>(); public bool deleted; public string shareURL, content, publishURL; public List<string> systemTags = new List<string>(); public double modificationDate, creationDate; }

		private readonly WebClient web;

		public SimpleNoteAPI(IWebProxy proxy)
		{
			web = new WebClient();
			if (proxy != null) web.Proxy = proxy;
			web.Headers["User-Agent"] = "CommonNote/1.0.0.0";
			web.Headers["X-Simperium-API-Key"] = API_KEY;
		}

		public APIResultAuthorize Authenticate(string userName, string password)
		{
			var uri = new Uri($"https://auth.simperium.com/1/{APP_ID}/authorize/");
			var content = $"{{\"username\":\"{userName}\", \"password\":\"{password}\"}}";

			var value = web.UploadString(uri, content);

			return JsonConvert.DeserializeObject<APIResultAuthorize>(value);
		}

		public APIResultIndex ListBuckets(string authToken)
		{
			web.Headers["X-Simperium-Token"] = authToken;

			var value = web.DownloadString($"https://api.simperium.com/1/{APP_ID}/note/index");
			APIResultIndex idx = JsonConvert.DeserializeObject<APIResultIndex>(value);

			while (!string.IsNullOrWhiteSpace(idx.mark))
			{
				var value2 = web.DownloadString($"https://api.simperium.com/1/{APP_ID}/note/index?mark={idx.mark}");
				APIResultIndex idx2 = JsonConvert.DeserializeObject<APIResultIndex>(value2);

				idx.current = idx2.current;
				idx.mark = idx2.mark;
				idx.index.AddRange(idx2.index);
			}

			return idx;
		}

		public SimpleNote GetNoteData(string authToken, string nodeID, int? version = null)
		{
			web.Headers["X-Simperium-Token"] = authToken;

			var url = $"https://api.simperium.com/1/{APP_ID}/note/i/{nodeID}";
			if (version != null) url += "/v/" + version;

			var value = web.DownloadString(url);
			var r = JsonConvert.DeserializeObject<APIResultNoteData>(value);

			return GetNoteFromQuery(r, web);
		}

		public SimpleNote UploadNote(string authToken, SimpleNote note)
		{
			note.Deleted = false;
			note.ID = Guid.NewGuid().ToString("N").ToUpper();
			note.CreationDate = DateTimeOffset.Now;
			note.ModificationDate = DateTimeOffset.Now;

			var uri = new Uri($"https://api.simperium.com/1/{APP_ID}/note/i/{note.ID}?response=1");

			APIResultNoteData data = new APIResultNoteData
			{
				tags = note.Tags,
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

			return GetNoteFromQuery(r, web);
		}

		public SimpleNote ChangeNote(string authToken, SimpleNote note)
		{
			if (note.Deleted) throw new Exception("Cannot update an already deleted note");
			if (note.ID == "") throw new Exception("Cannot change a not uploaded note");
			note.ModificationDate = DateTimeOffset.Now;

			var uri = new Uri($"https://api.simperium.com/1/{APP_ID}/note/i/{note.ID}?response=1");

			APIResultNoteData data = new APIResultNoteData
			{
				tags = note.Tags,
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

			return GetNoteFromQuery(r, web);
		}

		public void DeleteNote(string authToken, SimpleNote note)
		{
			if (note.ID == "") throw new Exception("Cannot delete a not uploaded note");
			note.ModificationDate = DateTimeOffset.Now;

			var uri = new Uri($"https://api.simperium.com/1/{APP_ID}/note/i/{note.ID}");
			
			web.UploadString(uri, "DELETE", string.Empty);
		}

		private SimpleNote GetNoteFromQuery(APIResultNoteData r, WebClient c)
		{
			return new SimpleNote
			{
				Tags = r.tags,
				Deleted = r.deleted,
				ShareURL = r.shareURL,
				PublishURL = r.publishURL,
				SystemTags = r.systemTags,
				Content = r.content,
				ModificationDate = ConvertFromEpochDate(r.modificationDate),
				CreationDate = ConvertFromEpochDate(r.creationDate),
				Version = int.Parse(c.ResponseHeaders["X-Simperium-Version"]),
			};
		}

		private DateTimeOffset ConvertFromEpochDate(double seconds)
		{
			return TIMESTAMP_ORIGIN.AddSeconds(seconds);
		}

		private double ConvertToEpochDate(DateTimeOffset offset)
		{
			return TimeZoneInfo.ConvertTimeToUtc(offset.DateTime, TimeZoneInfo.Local).ToUniversalTime().Subtract(TIMESTAMP_ORIGIN.DateTime).TotalSeconds;
		}
	}
}
