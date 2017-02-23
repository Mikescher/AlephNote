using AlephNote.PluginInterface;
using MSHC.Lang.Extensions;
using MSHC.Math.Encryption;
using MSHC.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://github.com/standardnotes/doc/blob/master/Client%20Development%20Guide.md
	/// http://standardfile.org/#api
	/// </summary>
	public static class StandardNoteAPI
	{
#pragma warning disable 0649
		// ReSharper disable All
		public enum PasswordAlg { sha512, sha256 }
		public enum PasswordFunc { pbkdf2 }

		public class APIAuthParams { public string pw_salt; public PasswordAlg pw_alg; public PasswordFunc pw_func; public int pw_cost, pw_key_size; }
		public class APIResultUser { public Guid uuid; public string email; }
		public class APIResultAuthorize { public APIResultUser user; public string token; public byte[] masterkey; }
		public class APIBodyItem { public Guid uuid; public string content_type, content, enc_item_key, auth_hash; public DateTimeOffset created_at; public bool deleted; }
		public class APIResultItem { public Guid uuid; public string content_type, content, enc_item_key, auth_hash; public DateTimeOffset created_at, updated_at; public bool deleted; }
		public class APIBodySync { public int limit; public List<APIBodyItem> items; public string sync_token, cursor_token; }
		public class APIResultSync { public List<APIResultItem> retrieved_items, saved_items, unsaved; public string sync_token, cursor_token; }
		public class SyncResultTag { public Guid uuid; public string title; public bool deleted; public string enc_item_key, item_key; }
		public class SyncResult { public List<StandardNote> retrieved_notes, saved_notes, unsaved_notes, deleted_notes; public List<SyncResultTag> retrieved_tags, saved_tags, unsaved_tags; }
		public class APIResultContentRef { public Guid uuid; public string content_type; }
		public class ContentNote { public string title, text; public List<APIResultContentRef> references; }
		public class ContentTag { public string title; public List<APIResultContentRef> references; }
		// ReSharper restore All
#pragma warning restore 0649

		private static WebClient CreateClient(IWebProxy proxy, APIResultAuthorize authToken)
		{
			var web = new GZWebClient();
			if (proxy != null) web.Proxy = proxy;
			web.Headers["User-Agent"] = "AlephNote/1.0.0.0";
			if (authToken != null) web.Headers["Authorization"] = "Bearer " + authToken.token;
			return web;
		}

		private static string CreateUri(string host, string path, string parameter = "")
		{
			if (!host.ToLower().StartsWith("http")) host = "http://" + host;

			if (!host.EndsWith("/")) host = host + "/";
			if (path.StartsWith("/")) path = path.Substring(1);

			if (path.EndsWith("/")) path = path.Substring(path.Length);

			if (parameter != "")
				return host + path + "?" + parameter;
			else
				return host + path;
		}

		public static APIResultAuthorize Authenticate(ISimpleJsonRest web, string mail, string password, IAlephLogger logger)
		{
			var apiparams = web.Get<APIAuthParams>("auth/params", "email=" + mail);

			try
			{
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_func: " + apiparams.pw_func);
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_alg: " + apiparams.pw_alg);
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_cost: " + apiparams.pw_cost);
				logger.Debug(StandardNotePlugin.Name, "AutParams.pw_key_size: " + apiparams.pw_key_size);

				if (apiparams.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unknown pw_func: " + apiparams.pw_func);

				byte[] bytes; 

				if (apiparams.pw_alg == PasswordAlg.sha512)
				{
					bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				}
				else if (apiparams.pw_alg == PasswordAlg.sha512)
				{
					bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				}
				else
				{
					throw new Exception("Unknown pw_alg: " + apiparams.pw_alg);
				}

				var pw = bytes.Take(bytes.Length / 2).ToArray();
				var mk = bytes.Skip(bytes.Length / 2).ToArray();

				var reqpw = EncodingConverter.ByteToHexBitFiddleUppercase(pw).ToLower();
				var tok = web.PostDownload<APIResultAuthorize>("auth/sign_in", "email=" + mail, "password=" + reqpw);

				tok.masterkey = mk;
				return tok;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.", e);
			}
		}

		public static SyncResult Sync(ISimpleJsonRest web, APIResultAuthorize authToken, StandardNoteConfig cfg, StandardNoteData dat, List<StandardNote> notesUpload, List<StandardNote> notesDelete, List<StandardFileTag> tagsDelete)
		{
			APIBodySync d = new APIBodySync();
			d.cursor_token = null;
			d.sync_token = dat.SyncToken;
			d.items = new List<APIBodyItem>();

			var allTags = dat.Tags.ToList();

			foreach (var mvNote in notesUpload)
			{
				PrepareForUpload(d, mvNote, allTags, authToken, cfg, false);
			}

			foreach (var rmNote in notesDelete)
			{
				PrepareForUpload(d, rmNote, allTags, authToken, cfg, true);
			}

			//TODO Do i need to update references on tags ??

			if (cfg.RemEmptyTags)
			{
				foreach (var rmTag in tagsDelete)
				{
					PrepareForUpload(d, rmTag, authToken, cfg, true);
				}
			}
			
			var result = web.PostTwoWay<APIResultSync>(d, "items/sync");

			dat.SyncToken = result.sync_token;

			var syncresult = new SyncResult();

			syncresult.retrieved_tags = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Select(n => CreateTag(n, authToken))
				.ToList();

			syncresult.saved_tags = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Select(n => CreateTag(n, authToken))
				.ToList();

			syncresult.unsaved_tags = result
				.unsaved
				.Where(p => p.content_type.ToLower() == "tag")
				.Select(n => CreateTag(n, authToken))
				.ToList();

			dat.UpdateTags(syncresult.retrieved_tags, syncresult.saved_tags, syncresult.unsaved_tags);

			syncresult.retrieved_notes = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Where(p => !p.deleted)
				.Select(n => CreateNote(n, authToken, cfg, dat))
				.ToList();

			syncresult.deleted_notes = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Where(p => p.deleted)
				.Select(n => CreateNote(n, authToken, cfg, dat))
				.ToList();

			syncresult.saved_notes = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Select(n => CreateNote(n, authToken, cfg, dat))
				.ToList();

			syncresult.unsaved_notes = result
				.unsaved
				.Where(p => p.content_type.ToLower() == "note")
				.Select(n => CreateNote(n, authToken, cfg, dat))
				.ToList();

			return syncresult;
		}

		private static void PrepareForUpload(APIBodySync body, StandardNote note, List<StandardFileTag> tags, APIResultAuthorize token, StandardNoteConfig cfg, bool delete)
		{
			var jsnContent = new ContentNote
			{
				title = note.Title,
				text = note.Text
			};

			foreach (var tagTitle in note.Tags)
			{
				if (tags.All(t => t.Title != tagTitle))
				{
					var newtag = new StandardFileTag {EncryptionKey = null, Title = tagTitle, UUID = Guid.NewGuid()};

					PrepareForUpload(body, newtag, token, cfg, false);

					tags.Add(newtag);
				}

				var rt = tags.First(e => e.Title == tagTitle);
				jsnContent.references.Add(new APIResultContentRef { content_type = "Tag", uuid = rt.UUID });
			}

			var cdNote = StandardNoteCrypt.EncryptContent(note.EncryptionKey, JsonConvert.SerializeObject(jsnContent), token.masterkey, cfg.SendEncrypted);

			body.items.Add(new APIBodyItem
			{
				content_type = "Note",
				uuid = note.ID,
				created_at = note.CreationDate,
				enc_item_key = cdNote.enc_item_key,
				auth_hash = cdNote.auth_hash,
				content = cdNote.enc_content,
				deleted = delete,
			});
		}

		private static void PrepareForUpload(APIBodySync body, StandardFileTag tag, APIResultAuthorize token, StandardNoteConfig cfg, bool delete)
		{
			var jsnContent = new ContentTag
			{
				title = tag.Title,
			};
			
			var cdNote = StandardNoteCrypt.EncryptContent(tag.EncryptionKey, JsonConvert.SerializeObject(jsnContent), token.masterkey, cfg.SendEncrypted);

			tag.EncryptionKey = cdNote.item_key;

			body.items.Add(new APIBodyItem
			{
				content_type = "Tag",
				uuid = tag.UUID,
				enc_item_key = cdNote.enc_item_key,
				auth_hash = cdNote.auth_hash,
				content = cdNote.enc_content,
				deleted = delete,
			});
		}

		private static StandardNote CreateNote(APIResultItem encNote, APIResultAuthorize authToken, StandardNoteConfig cfg, StandardNoteData dat)
		{
			ContentNote content;
			string itemKey;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encNote.content, encNote.enc_item_key, encNote.auth_hash, authToken.masterkey, out itemKey);
				content = JsonConvert.DeserializeObject<ContentNote>(contentJson);
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			var n = new StandardNote(encNote.uuid, cfg)
			{
				CreationDate = encNote.created_at,
				ModificationDate = encNote.updated_at,
				EncryptionKey = itemKey,
				Text = content.text,
				Title = content.title,
			};

			n.Tags.Synchronize(content.references.Select(p => dat.Tags.First(t => t.UUID == p.uuid).Title));

			return n;
		}

		private static SyncResultTag CreateTag(APIResultItem encTag, APIResultAuthorize authToken)
		{
			ContentTag content;
			string itemKey;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encTag.content, encTag.enc_item_key, encTag.auth_hash, authToken.masterkey, out itemKey);
				content = JsonConvert.DeserializeObject<ContentTag>(contentJson);
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			return new SyncResultTag
			{
				deleted = encTag.deleted,
				title = content.title,
				uuid = encTag.uuid,
				enc_item_key = encTag.enc_item_key,
				item_key = itemKey,
			};
		}
	}
}
