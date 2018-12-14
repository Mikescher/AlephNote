using System.Diagnostics;
using AlephNote.PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlephNote.PluginInterface.Exceptions;
using AlephNote.PluginInterface.Util;
using MSHC.Lang.Collections;
using MSHC.Math.Encryption;
using MSHC.Serialization;

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

		public class APIAuthParams { public string version, pw_salt, pw_nonce; public PasswordAlg pw_alg; public PasswordFunc pw_func; public int pw_cost, pw_key_size; }
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
		public class SyncResultTag { public Guid uuid; public string title; public bool deleted; public string enc_item_key, item_key; public List<APIResultContentRef> references; }
		public class SyncResult { public List<StandardFileNote> retrieved_notes, saved_notes, conflict_notes, error_notes, deleted_notes; public List<SyncResultTag> retrieved_tags, saved_tags, unsaved_tags, deleted_tags; }
		public class APIResultContentRef { public Guid uuid; public string content_type; }
		public class ContentNote { public string title, text; public List<APIResultContentRef> references; public Dictionary<string, Dictionary<string, object>> appData; }
		public class ContentTag { public string title; public List<APIResultContentRef> references; }
		// ReSharper restore All
#pragma warning restore 0649

		private static readonly Tuple<string, string> APPDATA_PINNED = Tuple.Create("org.standardnotes.sn", "pinned");
		private static readonly Tuple<string, string> APPDATA_LOCKED = Tuple.Create("org.standardnotes.sn", "locked");

		public static IAlephLogger Logger;

		public static APIResultAuthorize Authenticate(ISimpleJsonRest web, string mail, string password, IAlephLogger logger)
		{
			var apiparams = web.Get<APIAuthParams>("auth/params", "email=" + mail);

			if (apiparams.version == "001") return Authenticate001(web, apiparams, mail, password, logger);
			if (apiparams.version == "002") return Authenticate002(web, apiparams, mail, password, logger);
			if (apiparams.version == "003") return Authenticate003(web, apiparams, mail, password, logger);
			if (apiparams.version == "004") throw new StandardNoteAPIException("Unsupported encryption scheme 004 in auth-params");
			if (apiparams.version == "005") throw new StandardNoteAPIException("Unsupported encryption scheme 005 in auth-params");
			if (apiparams.version == "006") throw new StandardNoteAPIException("Unsupported encryption scheme 006 in auth-params");
			if (apiparams.version == "007") throw new StandardNoteAPIException("Unsupported encryption scheme 007 in auth-params");
			if (apiparams.version == "008") throw new StandardNoteAPIException("Unsupported encryption scheme 008 in auth-params");
			if (apiparams.version == "009") throw new StandardNoteAPIException("Unsupported encryption scheme 009 in auth-params");
			if (apiparams.version == "010") throw new StandardNoteAPIException("Unsupported encryption scheme 010 in auth-params");

			throw new Exception("Unsupported auth API version: " + apiparams.version);
		}

		private static APIResultAuthorize Authenticate001(ISimpleJsonRest web, APIAuthParams apiparams, string mail, string uip, IAlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AuthParams[version:1, pw_func:{apiparams.pw_func}, pw_alg:{apiparams.pw_alg}, pw_cost:{apiparams.pw_cost}, pw_key_size:{apiparams.pw_key_size}]");

				if (apiparams.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unsupported pw_func: " + apiparams.pw_func);

				byte[] bytes;

				if (apiparams.pw_alg == PasswordAlg.sha512)
				{
					bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				}
				else if (apiparams.pw_alg == PasswordAlg.sha512)
				{
					bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
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
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Authentification with StandardNoteAPI failed.", e);
			}
		}

		private static APIResultAuthorize Authenticate002(ISimpleJsonRest web, APIAuthParams apiparams, string mail, string uip, IAlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AutParams[version:2, pw_cost:{apiparams.pw_cost}]");

				if (apiparams.pw_func != PasswordFunc.pbkdf2) throw new Exception("Unknown pw_func: " + apiparams.pw_func);

				byte[] bytes = PBKDF2.GenerateDerivedKey(768/8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				
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
		
		private static APIResultAuthorize Authenticate003(ISimpleJsonRest web, APIAuthParams apiparams, string mail, string uip, IAlephLogger logger)
		{
			try
			{
				logger.Debug(StandardNotePlugin.Name, $"AutParams[version:{apiparams.version}, pw_cost:{apiparams.pw_cost}, pw_nonce:{apiparams.pw_nonce}]");

				if (apiparams.pw_cost < 100000) throw new StandardNoteAPIException($"Account pw_cost is too small ({apiparams.pw_cost})");

				var salt = StandardNoteCrypt.SHA256(string.Join(":", mail, "SF", "003", apiparams.pw_cost.ToString(), apiparams.pw_nonce));
				byte[] bytes = PBKDF2.GenerateDerivedKey(768/8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
				
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
				tok.version = "003";
				return tok;
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
				PrepareNoteForUpload(web, d, mvNote, allTags, authToken, cfg, false);
			}

			// Delete deleted notes
			foreach (var rmNote in notesDelete)
			{
				PrepareNoteForUpload(web, d, rmNote, allTags, authToken, cfg, true);
			}

			// Update references on tags (from changed notes)
			foreach (var upTag in GetTagsInNeedOfUpdate(dat.Tags, notesUpload, notesDelete))
			{
				PrepareTagForUpload(web, d, upTag, authToken, false);
			}

			// Remove unused tags
			if (cfg.RemEmptyTags)
			{
				foreach (var rmTag in tagsDelete)
				{
					PrepareTagForUpload(web, d, rmTag, authToken, true);
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
				foreach (var noteTag in note.InternalTags)
				{
					if (noteTag.UUID == null) continue;

					var realTag = allTags.FirstOrDefault(t => t.UUID == noteTag.UUID);

					if (realTag == null) // create new tag
					{
						var addtag = new StandardFileTag(noteTag.UUID, noteTag.Title, Enumerable.Repeat(note.ID, 1));
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
							var addtag = new StandardFileTag(realTag.UUID, realTag.Title, realTag.References.Concat(Enumerable.Repeat(note.ID, 1)));
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
						var addtag = new StandardFileTag(realTag.UUID, realTag.Title, realTag.References.Except(Enumerable.Repeat(note.ID, 1)));
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
							var addtag = new StandardFileTag(tag.UUID, tag.Title, tag.References.Except(Enumerable.Repeat(note.ID, 1)));
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

		private static void PrepareNoteForUpload(ISimpleJsonRest web, APIBodySync body, StandardFileNote note, List<StandardFileTag> allTags, APIResultAuthorize token, StandardNoteConfig cfg, bool delete)
		{
			var appdata = new Dictionary<string, Dictionary<string, object>>();

			SetAppDataBool(appdata, APPDATA_PINNED, note.IsPinned);
			SetAppDataBool(appdata, APPDATA_LOCKED, note.IsLocked);

			var jsnContent = new ContentNote
			{
				title = note.InternalTitle,
				text = note.Text.Replace("\r\n", "\n"),
				references = new List<APIResultContentRef>(),
				appData = appdata,
			};

			// Set correct tag UUID if tag already exists
			foreach (var itertag in note.InternalTags.ToList())
			{
				var itag = itertag;

				if (itag.UUID == null)
				{
					var newTag = allTags.FirstOrDefault(e => e.Title == itag.Title)?.ToRef();
					if (newTag == null)
					{
						newTag = new StandardFileTagRef(Guid.NewGuid(), itag.Title);
						allTags.Add(new StandardFileTag(newTag.UUID, newTag.Title, Enumerable.Repeat(note.ID, 1)));
					}

					note.UpgradeTag(itag, newTag);
				}
			}

			// Notes no longer have references to their tags (see issue #88)
			//foreach (var itertag in note.InternalTags.ToList())
			//{
			//	Debug.Assert(itertag.UUID != null, "itertag.UUID != null");
			//	jsnContent.references.Add(new APIResultContentRef { content_type = "Tag", uuid = itertag.UUID.Value });
			//}

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

		private static void PrepareTagForUpload(ISimpleJsonRest web, APIBodySync body, StandardFileTag tag, APIResultAuthorize token, bool delete)
		{
			var jsnContent = new ContentTag
			{
				title = tag.Title,
				references = tag
					.References
					.Select(n => new APIResultContentRef{content_type = "Note", uuid = n})
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
					InternalTitle = "",
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

				Logger.Debug(
					StandardNotePlugin.Name, 
					$"DecryptContent of note {encNote.uuid:B}", 
					$"[content]:\r\n{encNote.content}\r\n"+
					$"[enc_item_key]:\r\n{encNote.enc_item_key}\r\n" +
					$"[auth_hash]:\r\n{encNote.auth_hash}\r\n" +
					$"\r\n\r\n" +
					$"[contentJson]:\r\n{contentJson}\r\n");

				content = web.ParseJsonWithoutConverter<ContentNote>(contentJson);
			}
			catch (RestException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StandardNoteAPIException("Cannot decrypt note with local masterkey", e);
			}

			var n = new StandardFileNote(encNote.uuid, cfg, conn.HConfig)
			{
				Text = StandardNoteConfig.REX_LINEBREAK.Replace(content.text, Environment.NewLine),
				InternalTitle = content.title,
				AuthHash = encNote.auth_hash,
				ContentVersion = StandardNoteCrypt.GetSchemaVersion(encNote.content),
				IsPinned = GetAppDataBool(content.appData, APPDATA_PINNED, false),
				IsLocked = GetAppDataBool(content.appData, APPDATA_LOCKED, false),
			};

			var refTags = new List<StandardFileTagRef>();
			foreach (var cref in content.references)
			{
				if (cref.content_type == "Note")
				{
					// ignore
				}
				else if (dat.Tags.Any(t => t.UUID == cref.uuid))
				{
					refTags.Add(new StandardFileTagRef(cref.uuid, dat.Tags.First(t => t.UUID == cref.uuid).Title));
				}
				else if (cref.content_type == "Tag")
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
					references   = new List<APIResultContentRef>(),
				};
			}

			ContentTag content;
			try
			{
				var contentJson = StandardNoteCrypt.DecryptContent(encTag.content, encTag.enc_item_key, encTag.auth_hash, authToken.masterkey, authToken.masterauthkey);

				Logger.Debug(
					StandardNotePlugin.Name,
					$"DecryptContent of tag {encTag.uuid:B}",
					$"[content]:\r\n{encTag.content}\r\n" +
					$"[enc_item_key]:\r\n{encTag.enc_item_key}\r\n" +
					$"[auth_hash]:\r\n{encTag.auth_hash}\r\n" +
					$"\r\n\r\n" +
					$"[contentJson]:\r\n{contentJson}\r\n");

				content = web.ParseJsonWithoutConverter<ContentTag>(contentJson);
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
				title        = content.title,
				uuid         = encTag.uuid,
				enc_item_key = encTag.enc_item_key,
				references   = content.references,
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
	}
}
