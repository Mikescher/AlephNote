using System.Diagnostics;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using AlephNote.PluginInterface.Exceptions;
using MSHC.Lang.Collections;
using MSHC.Math.Encryption;
using MSHC.Network;
using MSHC.Serialization;
using System.Globalization;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	/// <summary>
	/// https://docs.standardnotes.org/specification/sync
	/// https://github.com/standardnotes/docs/blob/main/docs/specification/sync.md
	/// 
	/// https://docs.standardnotes.org/specification/encryption
	/// 
	/// https://docs.standardnotes.org/specification/encryption/003/
	/// https://github.com/standardnotes/docs/blob/main/docs/specification/encryption-003.md
	/// 
	/// https://github.com/standardnotes/docs/blob/main/docs/specification/encryption-004.md
	///
	/// https://github.com/jonhadfield/awesome-standard-notes
	/// 
	/// https://github.com/standardnotes/syncing-server
	/// https://github.com/standardnotes/snjs
	///
	/// </summary>
	public static class StandardNoteAPI
	{
#pragma warning disable 0649
		// ReSharper disable All
		public enum PasswordAlg { sha512, sha256 }
		public enum PasswordFunc { pbkdf2 }

		public class APIRequestUser { public string email, password, api; }
		public class APIRequestSync { public int limit; public List<APIRequestBodyItem> items; public string sync_token, cursor_token, api; }
		public class APIRequestBodyItem { public Guid uuid; public string content_type, content, enc_item_key, auth_hash; public Guid? items_key_id; public DateTimeOffset created_at, updated_at; public bool deleted; }

		public class APIResultAuthParams { public string identifier; public string pw_nonce; public string version; public string pw_salt; public PasswordAlg pw_alg; public PasswordFunc pw_func; public int pw_cost, pw_key_size; }
		public class APIResultAuthorize001 { public APIResultUser user; public string token; public byte[] masterkey, masterauthkey; public string version; }
		public class APIResultAuthorize004 { public APIResultUser user; public APIResultSession session; public APIResultKeyParams key_params; }
		public class APIResultSync { public List<APIResultItem> retrieved_items, saved_items; public List<APIResultConflictItem> conflicts; public string sync_token, cursor_token; }

		public class APIResultUser { public Guid uuid; public string email; }
		public class APIResultItem { public Guid uuid; public string content_type, content, enc_item_key, auth_hash; public Guid? items_key_id; public DateTimeOffset created_at, updated_at; public bool deleted; }
		public class APIResultErrorItem { public APIResultItem item; public APISyncResultError error; }
		public class APIResultConflictItem { public APIResultItem unsaved_item, server_item; public string type; }
		public class APISyncResultError { public string tag; }
		public class APIResultSession { public string access_token, refresh_token; public long access_expiration, refresh_expiration; }
		public class APIResultKeyParams { public string created; public string identifier, origination, pw_nonce, version; }

		public class APIRawBodyItem { public Guid uuid; public string content_type, content; public DateTimeOffset created_at, updated_at; public bool deleted; }

		public class APIBadRequest { public APIError error; }
		public class APIError { public string message; public int status; }
		public class SyncResultTag { public Guid uuid; public string title; public bool deleted; public string enc_item_key, item_key; public DateTimeOffset created_at, updated_at; public string rawappdata; public List<APIResultContentRef> references; }
		public class SyncResultItemsKey { public Guid uuid; public string version; public bool deleted; public string enc_item_key; public byte[] items_key; public DateTimeOffset created_at, updated_at; public bool isdefault; public string rawappdata; public List<APIResultContentRef> references; }
		public class SyncResult { public List<StandardFileNote> retrieved_notes, saved_notes, deleted_notes; public List<(StandardFileNote unsavednote, StandardFileNote servernote, string type)> syncconflict_notes, uuidconflict_notes; public List<SyncResultTag> retrieved_tags, saved_tags, deleted_tags; public List<(SyncResultTag unsavedtag, SyncResultTag servertag, string type)> syncconflict_tags, uuidconflict_tags; public List<SyncResultItemsKey> retrieved_keys, saved_keys, deleted_keys; public List<(SyncResultItemsKey unsavedkey, SyncResultItemsKey serverkey, string type)> syncconflict_keys, uuidconflict_keys; }
		public class APIResultContentRef { public Guid uuid; public string content_type; }
		public class ContentNote { public string title, text; public List<APIResultContentRef> references; public Dictionary<string, Dictionary<string, object>> appData; public bool @protected; public bool hidePreview; }
		public class ContentTag { public string title; public List<APIResultContentRef> references; public Dictionary<string, Dictionary<string, object>> appData; }
		public class ContentItemsKey { public string itemsKey, version; public bool isDefault; public List<APIResultContentRef> references; public Dictionary<string, Dictionary<string, object>> appData; }
		// ReSharper restore All
#pragma warning restore 0649

		private static readonly Tuple<string, string> APPDATA_PINNED          = Tuple.Create("org.standardnotes.sn", "pinned");
		private static readonly Tuple<string, string> APPDATA_LOCKED          = Tuple.Create("org.standardnotes.sn", "locked");
		private static readonly Tuple<string, string> APPDATA_CLIENTUPDATEDAT = Tuple.Create("org.standardnotes.sn", "client_updated_at");
		private static readonly Tuple<string, string> APPDATA_ARCHIVED        = Tuple.Create("org.standardnotes.sn", "archived");

		private static readonly Tuple<string, string> APPDATA_NOTECDATE        = Tuple.Create("com.mikescher.alephnote", "note_modified_at");
		private static readonly Tuple<string, string> APPDATA_NOTEMDATE        = Tuple.Create("com.mikescher.alephnote", "note_created_at");
		private static readonly Tuple<string, string> APPDATA_TEXTMDATE        = Tuple.Create("com.mikescher.alephnote", "text_modified_at");
		private static readonly Tuple<string, string> APPDATA_TITLEMDATE       = Tuple.Create("com.mikescher.alephnote", "title_modified_at");
		private static readonly Tuple<string, string> APPDATA_TAGSMDATE        = Tuple.Create("com.mikescher.alephnote", "tags_modified_at");
		private static readonly Tuple<string, string> APPDATA_PATHMDATE        = Tuple.Create("com.mikescher.alephnote", "path_modified_at");

		public static AlephLogger Logger;

		public static StandardNoteSessionData Authenticate(ISimpleJsonRest web, string mail, string password, AlephLogger logger)
		{
			var apiparams = web.Get<APIResultAuthParams>("auth/params", "email=" + WebUtility.UrlEncode(mail), "api=" + StandardNotePlugin.CURRENT_API_VERSION);

			if (apiparams.version == "001") return Authenticate001(web, apiparams, mail, password, logger);
			if (apiparams.version == "002") return Authenticate002(web, apiparams, mail, password, logger);
			if (apiparams.version == "003") return Authenticate003(web, apiparams, mail, password, logger);
			if (apiparams.version == "004") return Authenticate004(web, apiparams, mail, password, logger);
			if (apiparams.version == "005") throw new StandardNoteAPIException("Unsupported encryption scheme 005 in auth-params");
			if (apiparams.version == "006") throw new StandardNoteAPIException("Unsupported encryption scheme 006 in auth-params");
			if (apiparams.version == "007") throw new StandardNoteAPIException("Unsupported encryption scheme 007 in auth-params");
			if (apiparams.version == "008") throw new StandardNoteAPIException("Unsupported encryption scheme 008 in auth-params");
			if (apiparams.version == "009") throw new StandardNoteAPIException("Unsupported encryption scheme 009 in auth-params");
			if (apiparams.version == "010") throw new StandardNoteAPIException("Unsupported encryption scheme 010 in auth-params");

			throw new Exception("Unsupported auth API version: " + apiparams.version);
		}

		private static StandardNoteSessionData Authenticate001(ISimpleJsonRest web, APIResultAuthParams apiparams, string mail, string uip, AlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AuthParams[version:1, pw_func:{apiparams.pw_func}, pw_alg:{apiparams.pw_alg}, pw_cost:{apiparams.pw_cost}, pw_key_size:{apiparams.pw_key_size}]");

				var (pw, mk, reqpw) = StandardNoteCrypt.CreateAuthData001(apiparams, mail, uip);

				APIResultAuthorize001 tok;
				try
				{
					tok = web.PostDownload<APIResultAuthorize001>("auth/sign_in", "email=" + WebUtility.UrlEncode(mail), "password=" + WebUtility.UrlEncode(reqpw));
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

				return new StandardNoteSessionData
				{
					Version = "001",
					Token = tok.token,
					RootKey_MasterKey = mk,
				};
			}
			catch (StandardNoteAPIException)
			{
				throw;
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.", e);
			}
		}

		private static StandardNoteSessionData Authenticate002(ISimpleJsonRest web, APIResultAuthParams apiparams, string mail, string uip, AlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AutParams[version:2, pw_cost:{apiparams.pw_cost}]");

				var (pw, mk, ak, reqpw) = StandardNoteCrypt.CreateAuthData002(apiparams, uip);

				APIResultAuthorize001 tok;
				try
				{
					tok = web.PostTwoWay<APIResultAuthorize001>(new APIRequestUser { email = mail, password = reqpw }, "auth/sign_in");
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

				return new StandardNoteSessionData
				{
					Version = "002",
					Token = tok.token,
					RootKey_MasterKey = mk,
					RootKey_MasterAuthKey = ak,
				};
			}
			catch (RestException)
			{
				throw;
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
		
		private static StandardNoteSessionData Authenticate003(ISimpleJsonRest web, APIResultAuthParams apiparams, string mail, string uip, AlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AutParams[version:{apiparams.version}, pw_cost:{apiparams.pw_cost}, pw_nonce:{apiparams.pw_nonce}]");

				var (pw, mk, ak, reqpw) = StandardNoteCrypt.CreateAuthData003(apiparams, mail, uip);

				APIResultAuthorize001 tok;
				try
				{
					tok = web.PostTwoWay<APIResultAuthorize001>(new APIRequestUser { email = mail, password = reqpw, api = StandardNotePlugin.CURRENT_API_VERSION }, "auth/sign_in");
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

				return new StandardNoteSessionData
				{
					Version = "003",
					Token = tok.token,
					RootKey_MasterKey = mk,
					RootKey_MasterAuthKey = ak,
				};
			}
			catch (RestException)
			{
				throw;
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

		private static StandardNoteSessionData Authenticate004(ISimpleJsonRest web, APIResultAuthParams apiparams, string mail, string uip, AlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AutParams[version:{apiparams.version}, identifier:{apiparams.identifier}, pw_nonce:{apiparams.pw_nonce}]");

				var (masterKey, serverPassword, reqpw) = StandardNoteCrypt.CreateAuthData004(apiparams, mail, uip);

				try
				{
					var request = new APIRequestUser 
					{ 
						email    = mail,
						api      = StandardNotePlugin.CURRENT_API_VERSION,
						password = reqpw,
					};

					var result = web.PostTwoWay<APIResultAuthorize004>(request, "auth/sign_in");

					return new StandardNoteSessionData
					{
						Version = "004",

						Token        = result.session.access_token,
						RefreshToken = result.session.refresh_token,

						AccessExpiration  = (result.session.access_expiration == 0)  ? (DateTimeOffset?)null : DateTimeOffset.FromUnixTimeMilliseconds(result.session.access_expiration),
						RefreshExpiration = (result.session.refresh_expiration == 0) ? (DateTimeOffset?)null : DateTimeOffset.FromUnixTimeMilliseconds(result.session.refresh_expiration),

						Identifier     = result.key_params.identifier,
						PasswordNonce  = result.key_params.pw_nonce,
						ParamsCreated  = (result.key_params.created == null || result.key_params.created == "" || result.key_params.created == "0") ? (DateTimeOffset?)null : DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(result.key_params.created)),

						AccountEmail    = result.user.email,
						AccountUUID     = result.user.uuid,

						RootKey_MasterKey      = masterKey,
						RootKey_ServerPassword = serverPassword,
					};
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
			}
			catch (RestException)
			{
				throw;
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

		public static SyncResult Sync(ISimpleJsonRest web, StandardNoteConnection conn, StandardNoteConfig cfg, StandardNoteData dat, List<StandardFileNote> allNotes, List<StandardFileNote> notesUpload, List<StandardFileNote> notesDelete, List<StandardFileTag> tagsDelete)
		{
			APIRequestSync d = new APIRequestSync();
			d.api = StandardNotePlugin.CURRENT_API_VERSION;
			d.cursor_token = null;
			d.sync_token = string.IsNullOrWhiteSpace(dat.SyncToken) ? null : dat.SyncToken;
			d.limit = 150;
			d.items = new List<APIRequestBodyItem>();

			var items_raw = new List<APIRawBodyItem>();

			var allTags = dat.Tags.ToList();

			// Upload new notes
			foreach (var mvNote in notesUpload)
			{
				PrepareNoteForUpload(web, d, ref items_raw, mvNote, allTags, dat, cfg, false);
			}

			// Delete deleted notes
			foreach (var rmNote in notesDelete)
			{
				PrepareNoteForUpload(web, d, ref items_raw, rmNote, allTags, dat, cfg, true);
			}

			// Remove unused tags
			if (cfg.RemEmptyTags)
			{
				foreach (var rmTag in tagsDelete)
				{
					PrepareTagForUpload(web, d, ref items_raw, rmTag, dat, true);
				}
			}

			// Update references on tags (from changed notes)
			foreach (var upTag in GetTagsInNeedOfUpdate(dat.Tags, notesUpload, notesDelete))
			{
				if (items_raw.Any(p => p.uuid == upTag.UUID)) continue; // do not double-send deleted tags
				PrepareTagForUpload(web, d, ref items_raw, upTag, dat, false);
			}
			
			Logger.Debug(
				StandardNotePlugin.Name,
				$"Perform sync request ({items_raw.Count} items send)",
				"Sent Items (unencrypted):\n\n" + 
				string.Join("\n", items_raw.Select(i => 
					$"{{\n  content_type = {i.content_type}\n"+
					$"  uuid         = {i.uuid}\n" +
					$"  created_at   = {i.created_at}\n" +
					$"  updated_at   = {i.updated_at}\n" +
					$"  deleted      = {i.deleted}\n" +
					$"  content      =\n" +
					$"{CompactJsonFormatter.FormatJSON(i.content, 2, 1)}\n" +
					$"}}")) );

			var result = GetCursorResult(web, dat, d);

			var syncresult = new SyncResult();

			// ================================================================
			// ============================  KEYS  ============================
			// ================================================================

			syncresult.retrieved_keys = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "sn|itemskey")
				.Where(p => !p.deleted)
				.Select(n => CreateItemsKey(web, n, dat))
				.ToList();

			syncresult.deleted_keys = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "sn|itemskey")
				.Where(p => p.deleted)
				.Select(n => CreateItemsKey(web, n, dat))
				.ToList();

			syncresult.saved_keys = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "sn|itemskey")
				.Select(n => CreateItemsKey(web, n, dat))
				.ToList();

			syncresult.syncconflict_keys = result
				.conflicts
				.Where(p => p.server_item?.content_type?.ToLower() == "sn|itemskey")
				.Where(p => p.type == "sync_conflict")
				.Select(n => ((n.unsaved_item == null) ? null : CreateItemsKey(web, n.unsaved_item, dat), (n.server_item == null) ? null : CreateItemsKey(web, n.server_item, dat), n.type))
				.ToList();

			if (syncresult.syncconflict_keys.Any()) Logger.Warn(StandardNotePlugin.Name, "Sync returned [sync_conflict] for one or more items_keys");

			syncresult.uuidconflict_keys = result
				.conflicts
				.Where(p => p.server_item?.content_type?.ToLower() == "sn|itemskey")
				.Where(p => p.type == "uuid_conflict")
				.Select(n => ((n.unsaved_item == null) ? null : CreateItemsKey(web, n.unsaved_item, dat), (n.server_item == null) ? null : CreateItemsKey(web, n.server_item, dat), n.type))
				.ToList();

			if (syncresult.uuidconflict_keys.Any()) Logger.Error(StandardNotePlugin.Name, "Sync returned [uuid_conflict] for one or more items_keys");

			dat.UpdateKeys(syncresult.retrieved_keys, syncresult.saved_keys, syncresult.syncconflict_keys, syncresult.deleted_keys);

			// ================================================================
			// ============================  TAGS  ============================
			// ================================================================

			syncresult.retrieved_tags = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Where(p => !p.deleted)
				.Select(n => CreateTag(web, n, dat))
				.ToList();

			syncresult.deleted_tags = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Where(p => p.deleted)
				.Select(n => CreateTag(web, n, dat))
				.ToList();

			syncresult.saved_tags = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "tag")
				.Select(n => CreateTag(web, n, dat))
				.ToList();

			syncresult.syncconflict_tags = result
				.conflicts
				.Where(p => p.server_item?.content_type?.ToLower() == "tag")
				.Where(p => p.type == "sync_conflict")
				.Select(n => ((n.unsaved_item==null) ? null : CreateTag(web, n.unsaved_item, dat), (n.server_item == null) ? null : CreateTag(web, n.server_item, dat), n.type))
				.ToList();

			if (syncresult.syncconflict_tags.Any()) Logger.Warn(StandardNotePlugin.Name, "Sync returned [sync_conflict] for one or more tags");

			syncresult.uuidconflict_tags = result
				.conflicts
				.Where(p => p.server_item?.content_type?.ToLower() == "tag")
				.Where(p => p.type == "uuid_conflict")
				.Select(n => ((n.unsaved_item == null) ? null : CreateTag(web, n.unsaved_item, dat), (n.server_item == null) ? null : CreateTag(web, n.server_item, dat), n.type))
				.ToList();

			if (syncresult.uuidconflict_tags.Any()) Logger.Error(StandardNotePlugin.Name, "Sync returned [uuid_conflict] for one or more tags");

			dat.UpdateTags(syncresult.retrieved_tags, syncresult.saved_tags, syncresult.syncconflict_tags, syncresult.deleted_tags);

			// ================================================================
			// ============================  NOTES  ===========================
			// ================================================================

			syncresult.retrieved_notes = result
				.retrieved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Where(p => !p.deleted)
				.Select(n => CreateNote(web, conn, n, dat, cfg))
				.ToList();
			
			syncresult.deleted_notes = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Where(p => p.deleted)
				.Select(n => CreateNote(web, conn, n, dat, cfg))
				.ToList();

			syncresult.saved_notes = result
				.saved_items
				.Where(p => p.content_type.ToLower() == "note")
				.Select(n => CreateNote(web, conn, n, dat, cfg))
				.ToList();

			syncresult.syncconflict_notes = result
				.conflicts
				.Where(p => p.server_item?.content_type?.ToLower() == "note")
				.Where(p => p.type == "sync_conflict")
				.Select(n => ((n.unsaved_item == null) ? null : CreateNote(web, conn, n.unsaved_item, dat, cfg), (n.server_item == null) ? null : CreateNote(web, conn, n.server_item, dat, cfg), n.type))
				.ToList();

			syncresult.uuidconflict_notes = result
				.conflicts
				.Where(p => p.server_item?.content_type?.ToLower() == "note")
				.Where(p => p.type == "uuid_conflict")
				.Select(n => ((n.unsaved_item == null) ? null : CreateNote(web, conn, n.unsaved_item, dat, cfg), (n.server_item == null) ? null : CreateNote(web, conn, n.server_item, dat, cfg), n.type))
				.ToList();

			var items_unknown_conlict = result.conflicts.Where(p => p.type != "sync_conflict" && p.type != "uuid_conflict").ToList();
			if (items_unknown_conlict.Any())
            {
				Logger.Error(StandardNotePlugin.Name, "Unknown conflict type returned from API", string.Join("\n", items_unknown_conlict.Select(p => p.type)));
            }

			var items_unknown_type = result.retrieved_items.Where(p => 
				p.content_type?.ToLower() != "note" && 
				p.content_type?.ToLower() != "tag" &&
				p.content_type?.ToLower() != "sn|itemskey" &&
				p.content_type?.ToLower() != "sn|userpreferences" &&     // ignored by AlephNote
				p.content_type?.ToLower() != "sn|component").ToList();   // ignored by AlephNote
			if (items_unknown_type.Any())
			{
				Logger.Warn(StandardNotePlugin.Name, "Unknown types returned from API", string.Join("\n", items_unknown_type.Select(p => p.content_type)));
			}

			syncresult.retrieved_notes.AddRange(GetMissingNoteUpdates(syncresult.retrieved_tags.Concat(syncresult.saved_tags), dat.Tags, allNotes, syncresult.retrieved_notes));

			return syncresult;
		}

		private static IEnumerable<StandardFileTag> GetTagsInNeedOfUpdate(List<StandardFileTag> _allTags, List<StandardFileNote> notesUpload, List<StandardFileNote> notesDeleted)
		{
			var result = new List<StandardFileTag>();
			var allTags = _allTags.ToList();

			// [1] New Tags in Note
			foreach (var note in notesUpload)
			{
				foreach (var noteTagRef in note.InternalTags)
				{
					if (noteTagRef.UUID == null) continue;

					var realTag = allTags.FirstOrDefault(t => t.UUID == noteTagRef.UUID);

					if (realTag == null) // create new tag
					{
						var addtag = new StandardFileTag(noteTagRef.UUID, noteTagRef.Title, DateTimeOffset.MinValue, DateTimeOffset.MinValue, Enumerable.Repeat(note.ID, 1), string.Empty);
						allTags.Add(addtag);
						result.Add(addtag);
					}
					else
					{
						if (realTag.ContainsReference(note))
						{
							// tag already contains ref - all ok
						}
						else
						{
							// tag does not contain ref - update
							var addtag = new StandardFileTag(realTag.UUID, realTag.Title, realTag.CreationDate, realTag.ModificationDate, realTag.References.Concat(Enumerable.Repeat(note.ID, 1)), realTag.RawAppData);
							ReplaceOrAdd(allTags, realTag, addtag);
							ReplaceOrAdd(result, realTag, addtag);
						}
					}
				}
			}

			// [2] Tags that ref now-deleted notes
			foreach (var note in notesDeleted)
			{
				foreach (var noteTag in note.InternalTags)
				{
					if (noteTag.UUID == null) continue;

					var realTag = allTags.FirstOrDefault(t => t.UUID == noteTag.UUID);

					if (realTag != null && realTag.ContainsReference(note))
					{
						// tag does still refrence note - remove ref
						var addtag = new StandardFileTag(realTag.UUID, realTag.Title, realTag.CreationDate, realTag.ModificationDate, realTag.References.Except(Enumerable.Repeat(note.ID, 1)), realTag.RawAppData);
						ReplaceOrAdd(allTags, realTag, addtag);
						ReplaceOrAdd(result, realTag, addtag);
					}
				}
			}

			// [3] Removed Tags in note
			foreach (var tag in allTags.ToList())
			{
				if (tag.UUID == null) continue;

				foreach (var tagref in tag.References)
				{
					var note = notesUpload.FirstOrDefault(n => tagref == n.ID);
					if (note == null)
					{
						// ref links to a note that was not changed -- I guess its ok (?)
					}
					else
					{
						if (note.ContainsTag(tag.UUID.Value))
						{
							// note contains tags (and tag contains note) -- nothing changed - all ok
						}
						else
						{
							// note no longer contains tag - update tag
							var addtag = new StandardFileTag(tag.UUID, tag.Title, tag.CreationDate, tag.ModificationDate, tag.References.Except(Enumerable.Repeat(note.ID, 1)), tag.RawAppData);
							ReplaceOrAdd(allTags, tag, addtag);
							ReplaceOrAdd(result, tag, addtag);
						}
					}
				}
			}

			return result;
		}

		private static void ReplaceOrAdd(List<StandardFileTag> list, StandardFileTag old, StandardFileTag repl)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i] == old) { list[i] = repl; return; }
			}
			list.Add(repl);
		}

		private static IEnumerable<StandardFileNote> GetMissingNoteUpdates(IEnumerable<SyncResultTag> syncedTags, List<StandardFileTag> allTags, List<StandardFileNote> _oldNotes, List<StandardFileNote> newNotes)
		{
			// update tag ref changes in notes

			List<StandardFileNote> result = new List<StandardFileNote>();
			List<StandardFileNote> oldNotes = _oldNotes.Select(n => n.Clone()).Cast<StandardFileNote>().ToList();

			// [1] added tags
			foreach (var tag in syncedTags)
			{
				foreach (var tagRef in tag.references)
				{
					var oldnote = oldNotes.FirstOrDefault(n => n.ID == tagRef.uuid);
					var newnote = newNotes.FirstOrDefault(n => n.ID == tagRef.uuid);

					if (newnote != null) // note is already in changed-list
					{
						if (newnote.ContainsTag(tag.uuid))
						{
							// tag is in new one - all ok
						}
						else
						{
							// tag was missing, we add it
							// tag was probably added on remote side
							newnote.AddTag(new StandardFileTagRef(tag.uuid, tag.title));
						}
					}
					else if (oldnote != null) // another note was changed that is not in the changed-list
					{
						
						if (oldnote.ContainsTag(tag.uuid))
						{
							// tag is in it - all ok
						}
						else
						{
							// Add tag to note and add note to changed-list
							oldnote.AddTag(new StandardFileTagRef(tag.uuid, tag.title));
							result.Add(oldnote);
						}
					}
					else // tag references non-existant note ???
					{
						Logger.Warn(StandardNotePlugin.Name, $"Reference from tag {tag.uuid} to missing note {tagRef.uuid}");
					}
				}
			}

			// [2] removed tags from notes-in-bucket
			foreach (var note in newNotes)
			{
				foreach (var noteTag in note.InternalTags.ToList())
				{
					if (noteTag.UUID == null) continue; // unsynced

					var fullTag = allTags.FirstOrDefault(t => t.UUID == noteTag.UUID);

					if (fullTag == null)
					{
						// Note references tag that doesn't even exist -- remove it from note
						note.RemoveTag(noteTag);
					}
					else
					{
						if (fullTag.References.Any(r => r == note.ID))
						{
							// all ok, tag is in note and in reference-list of tag
						}
						else
						{
							// tag is _NOT_ in reference list of tag -- remove it from note
							note.RemoveTag(noteTag);
						}
					}
				}
			}

			// [3] removed tags from old-bucket
			foreach (var note in oldNotes.Where(n1 => newNotes.All(n2 => n1.ID != n2.ID)))
			{
				foreach (var noteTag in note.InternalTags.ToList())
				{
					if (noteTag.UUID == null) continue; // unsynced

					var fullTag = allTags.FirstOrDefault(t => t.UUID == noteTag.UUID);

					if (fullTag == null)
					{
						// Note references tag that doesn't even exist -- remove it from note
						note.RemoveTag(noteTag);
						result.Add(note);
					}
					else
					{
						if (fullTag.References.Any(r => r == note.ID))
						{
							// all ok, tag is in note and in reference-list of tag
						}
						else
						{
							// tag is _NOT_ in reference list of tag -- remove it from note
							note.RemoveTag(noteTag);
							result.Add(note);
						}
					}
				}
			}

			return result.DistinctBy(n => n.ID);
		}

		private static APIResultSync GetCursorResult(ISimpleJsonRest web, StandardNoteData dat, APIRequestSync d)
		{
			var masterResult = new APIResultSync
			{
				retrieved_items = new List<APIResultItem>(),
				conflicts = new List<APIResultConflictItem>(),
				saved_items = new List<APIResultItem>()
			};

			for (;;)
			{
				var result = web.PostTwoWay<APIResultSync>(d, "items/sync");
				dat.SyncToken = result.sync_token.Trim();

				masterResult.sync_token = result.sync_token;
				masterResult.cursor_token = result.cursor_token;

				masterResult.conflicts.AddRange(result.conflicts);
				masterResult.retrieved_items.AddRange(result.retrieved_items);
				masterResult.saved_items.AddRange(result.saved_items);

				if (result.cursor_token == null) return masterResult;

				d.cursor_token = result.cursor_token;
				d.items.Clear();
			}
		}

		private static void PrepareNoteForUpload(ISimpleJsonRest web, APIRequestSync body, ref List<APIRawBodyItem> bodyraw, StandardFileNote note, List<StandardFileTag> allTags, StandardNoteData dat, StandardNoteConfig cfg, bool delete)
		{
			var appdata = new Dictionary<string, Dictionary<string, object>>();

            try
			{
				if (!string.IsNullOrWhiteSpace(note.RawAppData)) appdata = web.ParseJsonOrNull<Dictionary<string, Dictionary<string, object>>>(note.RawAppData);
			}
            catch (Exception e)
            {
				Logger.Warn(StandardNotePlugin.Name, "Note contained invalid AppData", $"Note := {note.UniqueName}\nAppData:\n{note.RawAppData}");
				Logger.Error(StandardNotePlugin.Name, "Note contained invalid AppData - will be resetted on upload", e);
			}

			SetAppDataBool(appdata, APPDATA_PINNED,          note.IsPinned);
			SetAppDataBool(appdata, APPDATA_LOCKED,          note.IsLocked);
			SetAppDataBool(appdata, APPDATA_ARCHIVED,        note.IsArchived);

			SetAppDataDTO(appdata, APPDATA_CLIENTUPDATEDAT, note.ModificationDate);

			if (note.NoteCreationDate      != null) SetAppDataDTO(appdata, APPDATA_NOTECDATE,       note.NoteCreationDate.Value);
			if (note.NoteModificationDate  != null) SetAppDataDTO(appdata, APPDATA_NOTEMDATE,       note.NoteModificationDate.Value);
			if (note.TextModificationDate  != null) SetAppDataDTO(appdata, APPDATA_TEXTMDATE,       note.TextModificationDate.Value);
			if (note.TitleModificationDate != null) SetAppDataDTO(appdata, APPDATA_TITLEMDATE,      note.TitleModificationDate.Value);
			if (note.TagsModificationDate  != null) SetAppDataDTO(appdata, APPDATA_TAGSMDATE,       note.TagsModificationDate.Value);
			if (note.PathModificationDate  != null) SetAppDataDTO(appdata, APPDATA_PATHMDATE,       note.PathModificationDate.Value);

			var objContent = new ContentNote
			{
				title = note.InternalTitle,
				text = note.Text.Replace("\r\n", "\n"),
				references = new List<APIResultContentRef>(),
				appData = appdata,
				@protected = note.IsProtected,
				hidePreview = note.IsHidePreview,
			};

			// Set correct tag UUID if tag already exists or create new
			foreach (var itertag in note.InternalTags.ToList())
			{
				var itag = itertag;

				if (itag.UUID == null)
				{
					var newTag = allTags.FirstOrDefault(e => e.Title == itag.Title)?.ToRef();
					if (newTag == null)
					{
						// Tag does not exist - create new
						newTag = new StandardFileTagRef(Guid.NewGuid(), itag.Title);
						allTags.Add(new StandardFileTag(newTag.UUID, newTag.Title, DateTimeOffset.MinValue, DateTimeOffset.MinValue, Enumerable.Repeat(note.ID, 1), string.Empty));
						note.UpgradeTag(itag, newTag);
					} 
					else
                    {
						// Tag exists, only link
						note.UpgradeTag(itag, newTag);
					}
				}
			}

			// Notes no longer have references to their tags (see issue #88)
			//foreach (var itertag in note.InternalTags.ToList())
			//{
			//	Debug.Assert(itertag.UUID != null, "itertag.UUID != null");
			//	jsnContent.references.Add(new APIResultContentRef { content_type = "Tag", uuid = itertag.UUID.Value });
			//}

			var jsonContent = web.SerializeJson(objContent);

			var cryptData = StandardNoteCrypt.EncryptContent(jsonContent, note.ID, dat);

			body.items.Add(new APIRequestBodyItem
			{
				content_type = "Note",
				uuid         = note.ID,
				created_at   = note.CreationDate,
				updated_at   = note.RawModificationDate,
				enc_item_key = cryptData.enc_item_key,
				auth_hash    = cryptData.auth_hash,
				content      = cryptData.enc_content,
				items_key_id = cryptData.items_key_id,
				deleted      = delete,
			});
			bodyraw.Add(new APIRawBodyItem
			{
				content_type = "Note",
				uuid         = note.ID,
				created_at   = note.CreationDate,
				updated_at   = note.RawModificationDate,
				content      = jsonContent,
				deleted      = delete,
			});
		}

		private static void PrepareTagForUpload(ISimpleJsonRest web, APIRequestSync body, ref List<APIRawBodyItem> bodyraw, StandardFileTag tag, StandardNoteData dat, bool delete)
		{
			var appdata = new Dictionary<string, Dictionary<string, object>>();

			try
			{
				if (!string.IsNullOrWhiteSpace(tag.RawAppData)) appdata = web.ParseJsonOrNull<Dictionary<string, Dictionary<string, object>>>(tag.RawAppData);
			}
			catch (Exception e)
			{
				Logger.Warn(StandardNotePlugin.Name, "Tag contained invalid AppData", $"Tag := {tag.UUID}\nAppData:\n{tag.RawAppData}");
				Logger.Error(StandardNotePlugin.Name, "Tag contained invalid AppData - will be resetted on upload", e);
			}

			var objContent = new ContentTag
			{
				title = tag.Title,
				appData = appdata,
				references = tag
					.References
					.Select(n => new APIResultContentRef{content_type = "Note", uuid = n})
					.ToList(),
			};

			Debug.Assert(tag.UUID != null, "tag.UUID != null");
			
			var jsonContent = web.SerializeJson(objContent);

			var cryptData = StandardNoteCrypt.EncryptContent(jsonContent, tag.UUID.Value, dat);

			body.items.Add(new APIRequestBodyItem
			{
				content_type = "Tag",
				uuid         = tag.UUID.Value,
				created_at   = tag.CreationDate,
				updated_at   = tag.ModificationDate,
				enc_item_key = cryptData.enc_item_key,
				items_key_id = cryptData.items_key_id,
				auth_hash    = cryptData.auth_hash,
				content      = cryptData.enc_content,
				deleted      = delete,
			});
			bodyraw.Add(new APIRawBodyItem
			{
				content_type = "Tag",
				uuid         = tag.UUID.Value,
				created_at   = tag.CreationDate,
				updated_at   = tag.ModificationDate,
				content      = jsonContent,
				deleted      = delete,
			});
		}

		private static StandardFileNote CreateNote(ISimpleJsonRest web, StandardNoteConnection conn, APIResultItem encNote, StandardNoteData dat, StandardNoteConfig cfg)
		{
			if (encNote.deleted)
			{
				var nd = new StandardFileNote(encNote.uuid, cfg, conn.HConfig)
				{
					CreationDate = encNote.created_at,
					AuthHash = encNote.auth_hash,
					ContentVersion = StandardNoteCrypt.GetSchemaVersion(encNote.content),
				};
				nd.RawModificationDate = encNote.updated_at;
				return nd;
			}

			ContentNote content;
			string appDataContentString;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encNote.content, encNote.enc_item_key, encNote.items_key_id, encNote.auth_hash, dat);

				Logger.Debug(
					StandardNotePlugin.Name, 
					$"DecryptContent of note {encNote.uuid:B}", 
					$"[content]:\r\n{encNote.content}\r\n"+
					$"[enc_item_key]:\r\n{encNote.enc_item_key}\r\n" +
					$"[auth_hash]:\r\n{encNote.auth_hash}\r\n" +
					$"\r\n\r\n" +
					$"[contentJson]:\r\n{contentJson}\r\n");

				content = web.ParseJsonWithoutConverter<ContentNote>(contentJson);
				appDataContentString = web.ParseJsonAndGetSubJson(contentJson, "appData", string.Empty);
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			var n = new StandardFileNote(encNote.uuid, cfg, conn.HConfig);
			using (n.SuppressDirtyChanges())
			{
				n.Text           = StandardNoteConfig.REX_LINEBREAK.Replace(content.text, Environment.NewLine);
				n.InternalTitle  = content.title;

				n.AuthHash       = encNote.auth_hash;
				n.ContentVersion = StandardNoteCrypt.GetSchemaVersion(encNote.content);

				n.IsPinned       = GetAppDataBool(content.appData, APPDATA_PINNED,   false);
				n.IsLocked       = GetAppDataBool(content.appData, APPDATA_LOCKED,   false);
				n.IsArchived     = GetAppDataBool(content.appData, APPDATA_ARCHIVED, false);
				n.IsProtected    = content.@protected;
				n.IsHidePreview  = content.hidePreview;

				var refTags = new List<StandardFileTagRef>();
				foreach (var cref in content.references)
				{
					if (cref.content_type.ToLower() == "note")
					{
						// ignore
					}
					else if (dat.Tags.Any(t => t.UUID == cref.uuid))
					{
						refTags.Add(new StandardFileTagRef(cref.uuid, dat.Tags.First(t => t.UUID == cref.uuid).Title));
					}
					else if (cref.content_type.ToLower() == "tag")
					{
						Logger.Warn(StandardNotePlugin.Name, $"Reference to missing tag {cref.uuid} in note {encNote.uuid}");
					}
					else
					{
						Logger.Error(StandardNotePlugin.Name, $"Downloaded note contains an unknown reference :{cref.uuid} ({cref.content_type}) in note {encNote.uuid}");
					}
				}

				foreach (var tref in dat.Tags.Where(tag => tag.References.Any(tref => tref == encNote.uuid)))
				{
					refTags.Add(new StandardFileTagRef(tref.UUID, tref.Title));
				}

				refTags = refTags.DistinctBy(t => t.UUID).ToList();

				n.SetTags(refTags);
				n.SetReferences(content.references);

				n.CreationDate          = encNote.created_at;
				n.RawModificationDate   = encNote.updated_at;
				n.ClientUpdatedAt	    = GetAppDataDTO(content.appData, APPDATA_CLIENTUPDATEDAT, null);
				n.NoteCreationDate      = GetAppDataDTO(content.appData, APPDATA_NOTECDATE,       null);
				n.NoteModificationDate  = GetAppDataDTO(content.appData, APPDATA_NOTEMDATE,       null);
				n.TextModificationDate  = GetAppDataDTO(content.appData, APPDATA_TEXTMDATE,       null);
				n.TitleModificationDate = GetAppDataDTO(content.appData, APPDATA_TITLEMDATE,      null);
				n.TagsModificationDate  = GetAppDataDTO(content.appData, APPDATA_TAGSMDATE,       null);
				n.PathModificationDate  = GetAppDataDTO(content.appData, APPDATA_PATHMDATE,       null);

				n.RawAppData = appDataContentString;
			}

			return n;
		}

		private static SyncResultTag CreateTag(ISimpleJsonRest web, APIResultItem encTag, StandardNoteData dat)
		{
			if (encTag.deleted)
			{
				return new SyncResultTag
				{
					deleted      = encTag.deleted,
					created_at   = encTag.created_at,
					updated_at   = encTag.updated_at,
					title        = "",
					uuid         = encTag.uuid,
					enc_item_key = encTag.enc_item_key,
					item_key     = "",
					references   = new List<APIResultContentRef>(),
					rawappdata   = "",
				};
			}

			ContentTag content;
			string appDataContentString;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encTag.content, encTag.enc_item_key, encTag.items_key_id, encTag.auth_hash, dat);

				Logger.Debug(
					StandardNotePlugin.Name,
					$"DecryptContent of tag {encTag.uuid:B}",
					$"[content]:\r\n{encTag.content}\r\n" +
					$"[enc_item_key]:\r\n{encTag.enc_item_key}\r\n" +
					$"[auth_hash]:\r\n{encTag.auth_hash}\r\n" +
					$"\r\n\r\n" +
					$"[contentJson]:\r\n{contentJson}\r\n");

				content = web.ParseJsonWithoutConverter<ContentTag>(contentJson);
				appDataContentString = web.ParseJsonAndGetSubJson(contentJson, "appData", string.Empty);
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			return new SyncResultTag
			{
				deleted      = encTag.deleted,
				created_at   = encTag.created_at,
				updated_at   = encTag.updated_at,
				title        = content.title,
				uuid         = encTag.uuid,
				enc_item_key = encTag.enc_item_key,
				references   = content.references,
				rawappdata   = appDataContentString,
			};
		}

		private static SyncResultItemsKey CreateItemsKey(ISimpleJsonRest web, APIResultItem encKey, StandardNoteData dat)
		{
			if (encKey.deleted)
			{
				return new SyncResultItemsKey
				{
					deleted      = encKey.deleted,
					created_at   = encKey.created_at,
					updated_at   = encKey.updated_at,
					uuid         = encKey.uuid,
					enc_item_key = encKey.enc_item_key,

					items_key    = null,
					version      = string.Empty,
					references   = new List<APIResultContentRef>(),
					rawappdata   = string.Empty,
					isdefault    = false,
				};
			}

			ContentItemsKey content;
			string appDataContentString;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encKey.content, encKey.enc_item_key, encKey.items_key_id, encKey.auth_hash, dat);

				Logger.Debug(
					StandardNotePlugin.Name,
					$"DecryptContent of items_key {encKey.uuid:B}",
					$"[content]:\r\n{encKey.content}\r\n" +
					$"[enc_item_key]:\r\n{encKey.enc_item_key}\r\n" +
					$"[auth_hash]:\r\n{encKey.auth_hash}\r\n" +
					$"\r\n\r\n" +
					$"[contentJson]:\r\n{contentJson}\r\n");

				content = web.ParseJsonWithoutConverter<ContentItemsKey>(contentJson);
				appDataContentString = web.ParseJsonAndGetSubJson(contentJson, "appData", string.Empty);
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			return new SyncResultItemsKey
			{
				deleted      = encKey.deleted,
				created_at   = encKey.created_at,
				updated_at   = encKey.updated_at,
				uuid         = encKey.uuid,
				enc_item_key = encKey.enc_item_key,

				items_key    = EncodingConverter.StringToByteArrayCaseInsensitive(content.itemsKey),
				version      = content.version,
				references   = content.references,
				rawappdata   = appDataContentString,
				isdefault    = content.isDefault,
			};
		}

		private static bool GetAppDataBool(Dictionary<string, Dictionary<string, object>> appData, Tuple<string, string> path, bool defValue)
		{
			if (appData == null) return defValue;

			if (!appData.TryGetValue(path.Item1, out var values)) return defValue;

			if (!values.TryGetValue(path.Item2, out var value)) return defValue;

			if (value is bool bvalue) return bvalue;

			return XElementExtensions.TryParseBool(value?.ToString()) ?? defValue;
		}

		private static void SetAppDataBool(Dictionary<string, Dictionary<string, object>> appData, Tuple<string, string> path, bool value)
		{
			if (!appData.TryGetValue(path.Item1, out var dictNamespace))
				appData[path.Item1] = dictNamespace = new Dictionary<string, object>();

			dictNamespace[path.Item2] = value;
		}

		private static void SetAppDataString(Dictionary<string, Dictionary<string, object>> appData, Tuple<string, string> path, string value)
		{
			if (!appData.TryGetValue(path.Item1, out var dictNamespace))
				appData[path.Item1] = dictNamespace = new Dictionary<string, object>();

			dictNamespace[path.Item2] = value;
		}

		private static DateTimeOffset? GetAppDataDTO(Dictionary<string, Dictionary<string, object>> appData, Tuple<string, string> path, DateTimeOffset? defValue)
		{
			if (appData == null) return defValue;

			if (!appData.TryGetValue(path.Item1, out var values)) return defValue;

			if (!values.TryGetValue(path.Item2, out var value)) return defValue;

			if (value is DateTimeOffset sdto) return sdto;
			if (value is DateTime sdt) return sdt;
			if (value is string svalue && DateTimeOffset.TryParseExact(svalue, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto)) return dto;

			return defValue;
		}

		private static void SetAppDataDTO(Dictionary<string, Dictionary<string, object>> appData, Tuple<string, string> path, DateTimeOffset value)
		{
			if (!appData.TryGetValue(path.Item1, out var dictNamespace))
				appData[path.Item1] = dictNamespace = new Dictionary<string, object>();

			dictNamespace[path.Item2] = value.ToUniversalTime().ToString("O").Replace("+00:00", "Z");
		}
	}
}
