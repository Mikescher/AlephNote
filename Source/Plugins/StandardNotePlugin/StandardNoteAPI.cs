using System.Diagnostics;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlephNote.PluginInterface.Exceptions;
using AlephNote.PluginInterface.Util;

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

		public class APIAuthParams { public string version, pw_salt; public PasswordAlg pw_alg; public PasswordFunc pw_func; public int pw_cost, pw_key_size; }
		public class APIResultUser { public Guid uuid; public string email; }
		public class APIRequestUser { public string email, password; }
		public class APIResultAuthorize { public APIResultUser user; public string token; public byte[] masterkey, masterauthkey; public string version; }
		public class APIBodyItem { public Guid uuid; public string content_type, content, enc_item_key, auth_hash; public DateTimeOffset created_at; public bool deleted; }
		public class APIResultItem { public Guid uuid; public string content_type, content, enc_item_key, auth_hash; public DateTimeOffset created_at, updated_at; public bool deleted; }
		public class APIBodySync { public int limit; public List<APIBodyItem> items; public string sync_token, cursor_token; }
		public class APIResultErrorItem { public APIResultItem item; public APISyncResultError error; }
		public class APISyncResultError { public string tag; }
		public class APIResultSync { public List<APIResultItem> retrieved_items, saved_items; public List<APIResultErrorItem> unsaved; public string sync_token, cursor_token; }
		public class APIBadRequest { public APIError error; }
		public class APIError { public string message; public int status; }
		public class SyncResultTag { public Guid uuid; public string title; public bool deleted; public string enc_item_key, item_key; }
		public class SyncResult { public List<StandardFileNote> retrieved_notes, saved_notes, conflict_notes, error_notes, deleted_notes; public List<SyncResultTag> retrieved_tags, saved_tags, unsaved_tags, deleted_tags; }
		public class APIResultContentRef { public Guid uuid; public string content_type; }
		public class ContentNote { public string title, text; public List<APIResultContentRef> references; }
		public class ContentTag { public string title; public List<APIResultContentRef> references; }
		// ReSharper restore All
#pragma warning restore 0649

		public static IAlephLogger Logger;

		public static APIResultAuthorize Authenticate(ISimpleJsonRest web, string mail, string password, IAlephLogger logger)
		{
			var apiparams = web.Get<APIAuthParams>("auth/params", "email=" + mail);

			if (apiparams.version == "001") return Authenticate001(web, apiparams, mail, password, logger);
			if (apiparams.version == "002") return Authenticate002(web, apiparams, mail, password, logger);

			throw new Exception("Unsupported auth API version: " + apiparams.version);
		}

		private static APIResultAuthorize Authenticate001(ISimpleJsonRest web, APIAuthParams apiparams, string mail, string password, IAlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AuthParams[version:1, pw_func:{apiparams.pw_func}, pw_alg:{apiparams.pw_alg}, pw_cost:{apiparams.pw_cost}, pw_key_size:{apiparams.pw_key_size}]");

				if (apiparams.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unsupported pw_func: " + apiparams.pw_func);

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

				APIResultAuthorize tok;
				try
				{
					tok = web.PostDownload<APIResultAuthorize>("auth/sign_in", "email=" + mail, "password=" + reqpw);
				}
				catch (RestStatuscodeException e1)
				{
					if (e1.StatusCode/100 == 4 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
					{
						var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
						if (req != null) throw new StandardNoteAPIException($"Server returned status {e1.StatusCode}.\nMessage: '{req.error.message}'", e1);
					}

					throw;
				}

				tok.masterkey = mk;
				tok.version = "001";
				return tok;
			}
			catch (StandardNoteAPIException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.", e);
			}
		}

		private static APIResultAuthorize Authenticate002(ISimpleJsonRest web, APIAuthParams apiparams, string mail, string password, IAlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AutParams[version:2, pw_cost:{apiparams.pw_cost}]");

				if (apiparams.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unknown pw_func: " + apiparams.pw_func);

				byte[] bytes = PBKDF2.GenerateDerivedKey(768/8, Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				
				var pw = bytes.Skip(0 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();
				var mk = bytes.Skip(1 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();
				var ak = bytes.Skip(2 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();

				var reqpw = EncodingConverter.ByteToHexBitFiddleUppercase(pw).ToLower();
				APIResultAuthorize tok;
				try
				{
					tok = web.PostTwoWay<APIResultAuthorize>(new APIRequestUser { email = mail, password = reqpw }, "auth/sign_in");
				}
				catch (RestStatuscodeException e1)
				{
					if (e1.StatusCode / 100 == 4 && !string.IsNullOrWhiteSpace(e1.HTTPContent))
					{
						var req = web.ParseJsonOrNull<APIBadRequest>(e1.HTTPContent);
						if (req != null) throw new StandardNoteAPIException($"Server returned status {e1.StatusCode}.\nMessage: '{req.error.message}'", e1);
					}

					throw;
				}

				tok.masterkey = mk;
				tok.masterauthkey = ak;
				tok.version = "002";
				return tok;
			}
			catch (StandardNoteAPIException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.", e);
			}
		}

		public static SyncResult Sync(ISimpleJsonRest web, StandardNoteConnection conn, APIResultAuthorize authToken, StandardNoteConfig cfg, StandardNoteData dat, List<StandardFileNote> allNotes, List<StandardFileNote> notesUpload, List<StandardFileNote> notesDelete, List<StandardFileTag> tagsDelete)
		{
			APIBodySync d = new APIBodySync();
			d.cursor_token = null;
			d.sync_token = string.IsNullOrWhiteSpace(dat.SyncToken) ? null : dat.SyncToken;
			d.limit = 150;
			d.items = new List<APIBodyItem>();

			var allTags = dat.Tags.ToList();

			// Upload new notes
			foreach (var mvNote in notesUpload)
			{
				PrepareForUpload(web, d, mvNote, allTags, authToken, cfg, false);
			}

			// Delete deleted notes
			foreach (var rmNote in notesDelete)
			{
				PrepareForUpload(web, d, rmNote, allTags, authToken, cfg, true);
			}

			// Update references on tags (from changed notes)
			foreach (var upTag in notesUpload.SelectMany(n => n.InternalTags).Concat(notesDelete.SelectMany(n => n.InternalTags)).Except(tagsDelete))
			{
				PrepareForUpload(web, d, upTag, allNotes, authToken, cfg, false);
			}

			// Remove unused tags
			if (cfg.RemEmptyTags)
			{
				foreach (var rmTag in tagsDelete)
				{
					PrepareForUpload(web, d, rmTag, allNotes, authToken, cfg, true);
				}
			}
			
			var result = GetCursorResult(web, dat, d);

			var syncresult = new SyncResult();

			syncresult.retrieved_tags = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Where(p => !p.deleted)
				.Select(n => CreateTag(web, n, authToken))
				.ToList();

			syncresult.deleted_tags = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Where(p => p.deleted)
				.Select(n => CreateTag(web, n, authToken))
				.ToList();

			syncresult.saved_tags = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Select(n => CreateTag(web, n, authToken))
				.ToList();

			syncresult.unsaved_tags = result
				.unsaved
				.Where(p => p.item.content_type.ToLower() == "tag")
				.Select(n => CreateTag(web, n.item, authToken))
				.ToList();

			dat.UpdateTags(syncresult.retrieved_tags, syncresult.saved_tags, syncresult.unsaved_tags, syncresult.deleted_tags);

			syncresult.retrieved_notes = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Where(p => !p.deleted)
				.Select(n => CreateNote(web, conn, n, authToken, cfg, dat))
				.ToList();

			syncresult.deleted_notes = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Where(p => p.deleted)
				.Select(n => CreateNote(web, conn, n, authToken, cfg, dat))
				.ToList();

			syncresult.saved_notes = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Select(n => CreateNote(web, conn, n, authToken, cfg, dat))
				.ToList();

			syncresult.conflict_notes = result
				.unsaved
				.Where(p => p.item.content_type.ToLower() == "note")
				.Where(p => p.error.tag == "sync_conflict")
				.Select(n => CreateNote(web, conn, n.item, authToken, cfg, dat))
				.ToList();

			syncresult.error_notes = result
				.unsaved
				.Where(p => p.item.content_type.ToLower() == "note")
				.Where(p => p.error.tag != "sync_conflict")
				.Select(n => CreateNote(web, conn, n.item, authToken, cfg, dat))
				.ToList();

			return syncresult;
		}

		private static APIResultSync GetCursorResult(ISimpleJsonRest web, StandardNoteData dat, APIBodySync d)
		{
			var masterResult = new APIResultSync
			{
				retrieved_items = new List<APIResultItem>(),
				unsaved = new List<APIResultErrorItem>(),
				saved_items = new List<APIResultItem>()
			};

			for (;;)
			{
				var result = web.PostTwoWay<APIResultSync>(d, "items/sync");
				dat.SyncToken = result.sync_token.Trim();

				masterResult.sync_token = result.sync_token;
				masterResult.unsaved.AddRange(result.unsaved);
				masterResult.cursor_token = result.cursor_token;
				masterResult.retrieved_items.AddRange(result.retrieved_items);
				masterResult.saved_items.AddRange(result.saved_items);

				if (result.cursor_token == null) return masterResult;

				d.cursor_token = result.cursor_token;
				d.items.Clear();
			}
		}

		private static void PrepareForUpload(ISimpleJsonRest web, APIBodySync body, StandardFileNote note, List<StandardFileTag> tags, APIResultAuthorize token, StandardNoteConfig cfg, bool delete)
		{
			var jsnContent = new ContentNote
			{
				title = note.Title,
				text = note.Text,
				references = new List<APIResultContentRef>(),
			};

			foreach (var itertag in note.InternalTags.ToList())
			{
				var itag = itertag;

				if (itag.UUID == null)
				{
					var newTag = tags.FirstOrDefault(e => e.Title == itag.Title);
					if (newTag == null)
					{
						newTag = new StandardFileTag(Guid.NewGuid(), itag.Title);
						tags.Add(newTag);
					}

					note.UpgradeTag(itag, newTag);
					itag = newTag;
				}

				Debug.Assert(itag.UUID != null, "itag.UUID != null");
				jsnContent.references.Add(new APIResultContentRef { content_type = "Tag", uuid = itag.UUID.Value });
			}

			var cdNote = StandardNoteCrypt.EncryptContent(token.version, web.SerializeJson(jsnContent), note.ID, token.masterkey, token.masterauthkey);

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

		private static void PrepareForUpload(ISimpleJsonRest web, APIBodySync body, StandardFileTag tag, List<StandardFileNote> allNotes, APIResultAuthorize token, StandardNoteConfig cfg, bool delete)
		{
			var jsnContent = new ContentTag
			{
				title = tag.Title,
				references = allNotes
					.Where(n => n.InternalTags.Any(it => it.UUID == tag.UUID))
					.Select(n => new APIResultContentRef{content_type = "Note", uuid = n.ID})
					.ToList(),
			};

			Debug.Assert(tag.UUID != null, "tag.UUID != null");

			var cdNote = StandardNoteCrypt.EncryptContent(token.version, web.SerializeJson(jsnContent), tag.UUID.Value, token.masterkey, token.masterauthkey);

			body.items.Add(new APIBodyItem
			{
				content_type = "Tag",
				uuid = tag.UUID.Value,
				enc_item_key = cdNote.enc_item_key,
				auth_hash = cdNote.auth_hash,
				content = cdNote.enc_content,
				deleted = delete,
			});
		}

		private static StandardFileNote CreateNote(ISimpleJsonRest web, StandardNoteConnection conn, APIResultItem encNote, APIResultAuthorize authToken, StandardNoteConfig cfg, StandardNoteData dat)
		{
			if (encNote.deleted)
			{
				var nd = new StandardFileNote(encNote.uuid, cfg, conn.HConfig)
				{
					CreationDate = encNote.created_at,
					Text = "",
					Title = "",
					AuthHash = encNote.auth_hash,
					ContentVersion = StandardNoteCrypt.GetSchemaVersion(encNote.content),
				};
				nd.ModificationDate = encNote.updated_at;
				return nd;
			}

			ContentNote content;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encNote.content, encNote.enc_item_key, encNote.auth_hash, authToken.masterkey, authToken.masterauthkey);
				content = web.ParseJsonWithoutConverter<ContentNote>(contentJson);
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			var n = new StandardFileNote(encNote.uuid, cfg, conn.HConfig)
			{
				Text = content.text,
				Title = content.title,
				AuthHash = encNote.auth_hash,
				ContentVersion = StandardNoteCrypt.GetSchemaVersion(encNote.content),
			};

			var refTags = new List<StandardFileTag>();
			foreach (var cref in content.references)
			{
				if (cref.content_type == "Note")
				{
					// ignore
				}
				else if (dat.Tags.Any(t => t.UUID == cref.uuid))
				{
					refTags.Add(new StandardFileTag(cref.uuid, dat.Tags.First(t => t.UUID == cref.uuid).Title));
				}
				else
				{
					Logger.Error(StandardNotePlugin.Name, string.Format("Downloaded note contains an unknown reference :{0} ({1})", cref.uuid, cref.content_type));
				}
			}

			n.SetTags(refTags);
			n.SetReferences(content.references);
			n.CreationDate = encNote.created_at;
			n.ModificationDate = encNote.updated_at;

			return n;
		}

		private static SyncResultTag CreateTag(ISimpleJsonRest web, APIResultItem encTag, APIResultAuthorize authToken)
		{
			if (encTag.deleted)
			{
				return new SyncResultTag
				{
					deleted = encTag.deleted,
					title = "",
					uuid = encTag.uuid,
					enc_item_key = encTag.enc_item_key,
					item_key = "",
				};
			}

			ContentTag content;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encTag.content, encTag.enc_item_key, encTag.auth_hash, authToken.masterkey, authToken.masterauthkey);
				content = web.ParseJsonWithoutConverter<ContentTag>(contentJson);
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
			};
		}
	}
}
