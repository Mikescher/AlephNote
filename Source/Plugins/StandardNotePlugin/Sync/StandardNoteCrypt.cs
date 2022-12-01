using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AlephNote.PluginInterface.Util;
using MSHC.Math.Encryption;

// ReSharper disable InconsistentNaming
namespace AlephNote.Plugins.StandardNote
{
	static class StandardNoteCrypt
	{
		private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();

		public class EncryptResult { public string enc_item_key, auth_hash, enc_content; public Guid? items_key_id; }

		public static string DecryptContent(string encContent, string encItemKey, Guid? itemsKeyID, string authHash, StandardNoteData dat)
		{
			if (encContent.StartsWith("000")) return DecryptContent000(encContent);
			if (encContent.StartsWith("001")) return DecryptContent001(encContent, encItemKey, authHash, dat);
			if (encContent.StartsWith("002")) return DecryptContent002(encContent, encItemKey, dat);
			if (encContent.StartsWith("003")) return DecryptContent003(encContent, encItemKey, dat);
			if (encContent.StartsWith("004")) return DecryptContent004(encContent, encItemKey, itemsKeyID, dat);
			if (encContent.StartsWith("005")) throw new StandardNoteAPIException("Unsupported encryption scheme 005 in note content");
			if (encContent.StartsWith("006")) throw new StandardNoteAPIException("Unsupported encryption scheme 006 in note content");
			if (encContent.StartsWith("007")) throw new StandardNoteAPIException("Unsupported encryption scheme 007 in note content");
			if (encContent.StartsWith("008")) throw new StandardNoteAPIException("Unsupported encryption scheme 008 in note content");
			if (encContent.StartsWith("009")) throw new StandardNoteAPIException("Unsupported encryption scheme 009 in note content");
			if (encContent.StartsWith("010")) throw new StandardNoteAPIException("Unsupported encryption scheme 010 in note content");

			throw new StandardNoteAPIException("Unsupported encryption scheme ? in note content");
		}

		private static string DecryptContent000(string encContent)
		{
			StandardNoteAPI.Logger.Trace(StandardNotePlugin.Name, "Decrypt content with schema [000]", encContent);

			return Encoding.UTF8.GetString(Convert.FromBase64String(encContent));
		}

		private static string DecryptContent001(string encContent, string encItemKey, string authHash, StandardNoteData dat)
		{
			StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, "Decrypt content with schema [001]", 
				("encContent", encContent),
				("encItemKey", encItemKey),
				("authHash", authHash));

			byte[] masterkey;

			if (dat.SessionData.Version == "001" || dat.SessionData.Version == "002" || dat.SessionData.Version == "003")
			{
				masterkey = dat.SessionData.RootKey_MasterKey;

				StandardNoteAPI.Logger.Trace(StandardNotePlugin.Name, "Use masterkey from session");
			}
			else
			{
				var itemskey = dat.ItemsKeys.FirstOrDefault(p => p.Version == "001");
				if (itemskey == null) throw new StandardNoteAPIException($"Could not decrypt item (Key for 002 not found)");

				StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, $"Found itemskey: {itemskey.UUID}",
					("itemskey.IsDefault", itemskey.IsDefault.ToString()),
					("itemskey.Version", itemskey.Version),
					("itemskey.Key", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.Key)),
					("itemskey.AuthKey", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.AuthKey)));

				masterkey = itemskey.Key;
			}

			var itemKey = EncodingConverter.StringToByteArrayCaseInsensitive(Encoding.ASCII.GetString(AESEncryption.DecryptCBC256(Convert.FromBase64String(encItemKey), masterkey, new byte[16])));

			var ek = itemKey.Take(itemKey.Length / 2).ToArray();
			var ak = itemKey.Skip(itemKey.Length / 2).ToArray();

			var realHash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(encContent), ak));
			
			if (authHash == null) throw new ArgumentNullException(nameof(authHash));
			if (realHash.ToLower() != authHash.ToLower()) throw new StandardNoteAPIException("Decrypting content failed - hash mismatch");

			var c = AESEncryption.DecryptCBC256(Convert.FromBase64String(encContent.Substring(3)), ek, null);

			return Encoding.UTF8.GetString(c);
		}

		private static string DecryptContent002(string encContent, string encItemKey, StandardNoteData dat)
		{
			StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, "Decrypt content with schema [002]",
				("encContent", encContent),
				("encItemKey", encItemKey));

			byte[] masterMK;
			byte[] masterAK;

			if (dat.SessionData.Version == "002" || dat.SessionData.Version == "003")
			{
				masterMK = dat.SessionData.RootKey_MasterKey;
				masterAK = dat.SessionData.RootKey_MasterAuthKey;

				StandardNoteAPI.Logger.Trace(StandardNotePlugin.Name, "Use key/authkey from session");
			}
			else
			{
				var itemskey = dat.ItemsKeys.FirstOrDefault(p => p.Version == "002");
				if (itemskey == null) throw new StandardNoteAPIException($"Could not decrypt item (Key for 002 not found)");

				StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, $"Found itemskey: {itemskey.UUID}",
					("itemskey.IsDefault", itemskey.IsDefault.ToString()),
					("itemskey.Version", itemskey.Version),
					("itemskey.Key", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.Key)),
					("itemskey.AuthKey", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.AuthKey)));

				masterMK = itemskey.Key;
				masterAK = itemskey.AuthKey;
			}

			var item_key = Decrypt002(encItemKey, masterMK, masterAK);

			StandardNoteAPI.Logger.Trace(StandardNotePlugin.Name, "item_key decrypted", $"item_key := '{item_key}'");

			var item_ek = item_key.Substring(0, item_key.Length / 2);
			var item_ak = item_key.Substring(item_key.Length / 2, item_key.Length / 2);

			return Decrypt002(encContent, EncodingConverter.StringToByteArrayCaseInsensitive(item_ek), EncodingConverter.StringToByteArrayCaseInsensitive(item_ak));
		}

		private static string DecryptContent003(string encContent, string encItemKey, StandardNoteData dat)
		{
			StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, "Decrypt content with schema [002]",
				("encContent", encContent),
				("encItemKey", encItemKey));

			byte[] masterMK;
			byte[] masterAK;

			if (dat.SessionData.Version == "001" || dat.SessionData.Version == "002" || dat.SessionData.Version == "003")
			{
				masterMK = dat.SessionData.RootKey_MasterKey;
				masterAK = dat.SessionData.RootKey_MasterAuthKey;

				StandardNoteAPI.Logger.Trace(StandardNotePlugin.Name, "Use key/authkey from session");
			}
			else
			{
				var itemskey = dat.ItemsKeys.FirstOrDefault(p => p.Version == "003");
				if (itemskey == null) throw new StandardNoteAPIException($"Could not decrypt item (Key for 002 not found)");

				StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, $"Found itemskey: {itemskey.UUID}",
					("itemskey.IsDefault", itemskey.IsDefault.ToString()),
					("itemskey.Version", itemskey.Version),
					("itemskey.Key", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.Key)),
					("itemskey.AuthKey", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.AuthKey)));

				masterMK = itemskey.Key;
				masterAK = itemskey.AuthKey;
			}

			var item_key = Decrypt003(encItemKey, masterMK, masterAK);

			StandardNoteAPI.Logger.Trace(StandardNotePlugin.Name, "item_key decrypted", $"item_key := '{item_key}'");

			var item_ek = item_key.Substring(0, item_key.Length / 2);
			var item_ak = item_key.Substring(item_key.Length / 2, item_key.Length / 2);

			return Decrypt003(encContent, EncodingConverter.StringToByteArrayCaseInsensitive(item_ek), EncodingConverter.StringToByteArrayCaseInsensitive(item_ak));
		}

		private static string DecryptContent004(string encContent, string encItemKey, Guid? itemsKeyID, StandardNoteData dat)
		{
			StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, "Decrypt content with schema [002]",
				("encContent", encContent),
				("encItemKey", encItemKey),
				("itemsKeyID", itemsKeyID?.ToString() ?? "NULL"));

			var keyOuter = dat.SessionData.RootKey_MasterKey;
			if (itemsKeyID != null)
            {
				var itemskey = dat.ItemsKeys.FirstOrDefault(p => p.UUID == itemsKeyID);
				if (itemskey == null) throw new StandardNoteAPIException($"Could not decrypt item (Key {itemsKeyID} not found)");

				StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, $"Found itemskey: {itemskey.UUID}",
					("itemskey.IsDefault", itemskey.IsDefault.ToString()),
					("itemskey.Version", itemskey.Version),
					("itemskey.Key", EncodingConverter.ByteToHexBitFiddleLowercase(itemskey.Key)));

				keyOuter = itemskey.Key;
			}
			
			var keyInner = Decrypt004(encItemKey, keyOuter);

			return Decrypt004(encContent, EncodingConverter.StringToByteArrayCaseInsensitive(keyInner));
		}

		private static string Encrypt002(string string_to_encrypt, Guid uuid, byte[] encryption_key, byte[] auth_key)
		{
			byte[] IV = new byte[128 / 8];
			RNG.GetBytes(IV);

			var ciphertext = AESEncryption.EncryptCBC256(Encoding.UTF8.GetBytes(string_to_encrypt), encryption_key, IV);

			var string_to_auth = $"002:{uuid:D}:{EncodingConverter.ByteToHexBitFiddleLowercase(IV)}:{Convert.ToBase64String(ciphertext)}";

			var auth_hash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(string_to_auth), auth_key));

			return $"002:{auth_hash}:{uuid:D}:{EncodingConverter.ByteToHexBitFiddleLowercase(IV)}:{Convert.ToBase64String(ciphertext)}";
		}

		private static string Encrypt003(string string_to_encrypt, Guid uuid, byte[] encryption_key, byte[] auth_key)
		{
			byte[] IV = new byte[128 / 8];
			RNG.GetBytes(IV);

			var ciphertext = AESEncryption.EncryptCBC256(Encoding.UTF8.GetBytes(string_to_encrypt), encryption_key, IV);

			var string_to_auth = $"003:{uuid:D}:{EncodingConverter.ByteToHexBitFiddleLowercase(IV)}:{Convert.ToBase64String(ciphertext)}";

			var auth_hash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(string_to_auth), auth_key));

			return $"003:{auth_hash}:{uuid:D}:{EncodingConverter.ByteToHexBitFiddleLowercase(IV)}:{Convert.ToBase64String(ciphertext)}";
		}

		private static string Decrypt002(string string_to_decrypt, byte[] encryption_key, byte[] auth_key)
		{
			var components = string_to_decrypt.Split(':');
			var version = components[0];
			var auth_hash = components[1];
			var uuid = components[2];
			var IV = components[3];
			var ciphertext = components[4];

			if (auth_key != null && auth_key.Length > 0)
			{
				var string_to_auth = $"{version}:{uuid}:{IV}:{ciphertext}";
				var local_auth_hash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(string_to_auth), auth_key));

				if (local_auth_hash.ToUpper() != auth_hash.ToUpper())
				{
					StandardNoteAPI.Logger.Warn(StandardNotePlugin.Name, "AuthHash verification failed",
						$"local_auth_hash := '{local_auth_hash}'\nauth_hash := '{auth_hash}'");

					throw new Exception("Item auth-hash mismatch");
				}
				else
				{
					StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, "AuthHash verified",
						("local_auth_hash", local_auth_hash),
						("auth_hash", auth_hash));
				}
			} 
			else
			{
				StandardNoteAPI.Logger.Warn(StandardNotePlugin.Name, "AuthHash verification skipped (no auth_key)");
			}

			var result = AESEncryption.DecryptCBC256(Convert.FromBase64String(ciphertext), encryption_key, EncodingConverter.StringToByteArrayCaseInsensitive(IV));

			return Encoding.UTF8.GetString(result);
		}

		private static string Decrypt003(string string_to_decrypt, byte[] encryption_key, byte[] auth_key)
		{
			var components = string_to_decrypt.Split(':');
			var version = components[0];
			var auth_hash = components[1];
			var uuid = components[2];
			var IV = components[3];
			var ciphertext = components[4];

			if (auth_key != null && auth_key.Length > 0)
			{
				var string_to_auth = $"{version}:{uuid}:{IV}:{ciphertext}";
				var local_auth_hash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(string_to_auth), auth_key));

				if (local_auth_hash.ToUpper() != auth_hash.ToUpper())
				{
					StandardNoteAPI.Logger.Warn(StandardNotePlugin.Name, "AuthHash verification failed",
						$"local_auth_hash := '{local_auth_hash}'\nauth_hash := '{auth_hash}'");

					throw new Exception("Item auth-hash mismatch");
				}
				else
				{
					StandardNoteAPI.Logger.TraceExt(StandardNotePlugin.Name, "AuthHash verified",
						("local_auth_hash", local_auth_hash),
						("auth_hash", auth_hash));
				}
			}
            else
			{
				StandardNoteAPI.Logger.Warn(StandardNotePlugin.Name, "AuthHash verification skipped (no auth_key)");
			}

			var result = AESEncryption.DecryptCBC256(Convert.FromBase64String(ciphertext), encryption_key, EncodingConverter.StringToByteArrayCaseInsensitive(IV));

			return Encoding.UTF8.GetString(result);
		}

		private static string Decrypt004(string encContent, byte[] key)
		{
			var split = encContent.Split(':');

			var version = split[0];
			var nonce = EncodingConverter.StringToByteArrayCaseInsensitive(split[1]);
			var ciphertext = Convert.FromBase64String(split[2]);
			var authenticated_data = Encoding.UTF8.GetBytes(split[3]);

			if (version != "004") throw new StandardNoteAPIException($"Version must be 004 to decrypt 004 encrypted item (duh.)");

			var plain = ANCrypt.XChaCha20Decrypt(ciphertext, nonce, key, authenticated_data);

			return Encoding.UTF8.GetString(plain);
		}

		public static byte[] AuthSHA256(byte[] content, byte[] ak)
		{
			using (HMACSHA256 hmac = new HMACSHA256(ak))
			{
				return hmac.ComputeHash(content);
			}
		}

		public static string SHA256Hex(string data)
		{
			using(var sha = SHA256.Create())
			{
				var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
				return EncodingConverter.ByteToHexBitFiddleLowercase(hash);
			}
		}

		public static byte[] SHA256Bytes(string data)
		{
			using (var sha = SHA256.Create())
			{
				var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
				return hash;
			}
		}

		public static string RandomSeed(int len)
		{
			byte[] seed = new byte[len];
			RNG.GetBytes(seed);

			return EncodingConverter.ByteToHexBitFiddleLowercase(seed);
		}

		public static byte[] RandomKey(int len)
		{
			byte[] bin = new byte[len];
			RNG.GetBytes(bin);

			return bin;
		}

		public static (string, string) GeneratePKCE()
		{
			var codeVerifier = RandomSeed(256 / 8);

			var cc1 = SHA256Bytes(codeVerifier);
			var cc2 = EncodingConverter.ByteToHexBitFiddleLowercase(cc1);
			var cc3 = Encoding.UTF8.GetBytes(cc2);

			var codeChallenge = Convert.ToBase64String(cc3)
				.Replace('+', '-')
				.Replace('/', '_')
				.Replace("=", "");

			return (codeVerifier, codeChallenge);
		}

		public static EncryptResult EncryptContent(string content, Guid uuid, StandardNoteData dat)
		{
			if (dat.SessionData.Version == "001") return EncryptContent001(content,       dat.SessionData.RootKey_MasterKey);
			if (dat.SessionData.Version == "002") return EncryptContent002(content, uuid, dat.SessionData.RootKey_MasterKey, dat.SessionData.RootKey_MasterAuthKey);
			if (dat.SessionData.Version == "003") return EncryptContent003(content, uuid, dat.SessionData.RootKey_MasterKey, dat.SessionData.RootKey_MasterAuthKey);
			if (dat.SessionData.Version == "004") return EncryptContent004(content, uuid, dat);
			if (dat.SessionData.Version == "005") throw new StandardNoteAPIException("Unsupported encryption scheme 005 in note content");
			if (dat.SessionData.Version == "006") throw new StandardNoteAPIException("Unsupported encryption scheme 006 in note content");
			if (dat.SessionData.Version == "007") throw new StandardNoteAPIException("Unsupported encryption scheme 007 in note content");
			if (dat.SessionData.Version == "008") throw new StandardNoteAPIException("Unsupported encryption scheme 008 in note content");
			if (dat.SessionData.Version == "009") throw new StandardNoteAPIException("Unsupported encryption scheme 009 in note content");
			if (dat.SessionData.Version == "010") throw new StandardNoteAPIException("Unsupported encryption scheme 010 in note content");

			throw new Exception("Unsupported encryption scheme: " + dat.SessionData.Version);
		}

		private static EncryptResult EncryptContent001(string content, byte[] mk)
		{
			byte[] itemKey = new byte[512 / 8];

			RNG.GetBytes(itemKey);

			var ek = itemKey.Take(itemKey.Length / 2).ToArray();
			var ak = itemKey.Skip(itemKey.Length / 2).ToArray();

			string encContent = "001" + Convert.ToBase64String(AESEncryption.EncryptCBC256(Encoding.UTF8.GetBytes(content), ek, null));

			var authHash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(encContent), ak));

			var encItemKey = Convert.ToBase64String(AESEncryption.EncryptCBC256(Encoding.UTF8.GetBytes(EncodingConverter.ByteToHexBitFiddleLowercase(itemKey)), mk, null));

			return new EncryptResult
			{
				enc_item_key = encItemKey,
				enc_content = encContent,
				auth_hash = authHash,
			};
		}

        private static EncryptResult EncryptContent002(string rawContent, Guid uuid, byte[] masterMK, byte[] masterAK)
		{
			byte[] itemKey = new byte[512 / 8];
			RNG.GetBytes(itemKey);

			var item_ek = itemKey.Take(itemKey.Length / 2).ToArray();
			var item_ak = itemKey.Skip(itemKey.Length / 2).ToArray();

			var encContent = Encrypt002(rawContent, uuid, item_ek, item_ak);

			var encItemKey = Encrypt002(EncodingConverter.ByteToHexBitFiddleLowercase(itemKey), uuid, masterMK, masterAK);

			return new EncryptResult
			{
				enc_item_key = encItemKey,
				enc_content = encContent,
				auth_hash = null,
			};
		}

		private static EncryptResult EncryptContent003(string rawContent, Guid uuid, byte[] masterMK, byte[] masterAK)
		{
			byte[] itemKey = new byte[512 / 8];
			RNG.GetBytes(itemKey);

			var item_ek = itemKey.Take(itemKey.Length / 2).ToArray();
			var item_ak = itemKey.Skip(itemKey.Length / 2).ToArray();

			var encContent = Encrypt003(rawContent, uuid, item_ek, item_ak);

			var encItemKey = Encrypt003(EncodingConverter.ByteToHexBitFiddleLowercase(itemKey), uuid, masterMK, masterAK);

			return new EncryptResult
			{
				enc_item_key = encItemKey,
				enc_content = encContent,
				auth_hash = null,
			};
		}

		private static EncryptResult EncryptContent004(string rawContent, Guid uuid, StandardNoteData dat)
		{
			var item_key = RandomSeed(32);
			var authenticated_data = $"{{\"u\":\"{uuid:D}\",\"v\":\"004\"}}";

			var encrypted_content = Encrypt004(rawContent, EncodingConverter.StringToByteArrayCaseInsensitive(item_key), authenticated_data);

			var default_items_key = GetDefaultItemsKey(dat, "004");

			var enc_item_key = Encrypt004(item_key, default_items_key.Key, authenticated_data);

			return new EncryptResult
			{
				enc_item_key = enc_item_key,
				enc_content = encrypted_content,
				auth_hash = null,
				items_key_id = default_items_key.UUID,
			};
		}

		private static string Encrypt004(string content, byte[] key, string assocData)
        {
			var nonce = RandomSeed(24);

			var authenticated_data = Convert.ToBase64String(Encoding.UTF8.GetBytes(assocData));

			var ciphertext = ANCrypt.XChaCha20Encrypt(Encoding.UTF8.GetBytes(content), EncodingConverter.StringToByteArrayCaseInsensitive(nonce), key, Encoding.UTF8.GetBytes(authenticated_data));

			return string.Join(":", "004", nonce, Convert.ToBase64String(ciphertext), authenticated_data);
        }

		private static StandardFileItemsKey GetDefaultItemsKey(StandardNoteData dat, string version)
		{
			if (dat.ItemsKeys.Where(p => p.Version == version).Count() == 0) throw new StandardNoteAPIException("Could not encrypt item, no items_key in repository");

			if (dat.ItemsKeys.Where(p => p.Version == version).Count() == 1) return dat.ItemsKeys.Single(p => p.Version == version);

			var def = dat.ItemsKeys.FirstOrDefault(p => p.Version == version && p.IsDefault);
			if (def != null) return def;

			StandardNoteAPI.Logger.Warn(StandardNotePlugin.Name, "No default key for encryption specified (using latest)", $"Keys in storage: {dat.ItemsKeys.Count}");

			var latest = dat.ItemsKeys.Where(p => p.Version == version).OrderBy(p => p.CreationDate).Last();
			return latest;
		}

		public static string GetSchemaVersion(string strdata)
		{
			if (strdata == null) return "?";

			if (strdata.StartsWith("000")) return "000";
			if (strdata.StartsWith("001")) return "001";
			if (strdata.StartsWith("002")) return "002";
			if (strdata.StartsWith("003")) return "003";
			if (strdata.StartsWith("004")) return "004";
			if (strdata.StartsWith("005")) return "005";
			if (strdata.StartsWith("006")) return "006";
			if (strdata.StartsWith("007")) return "007";
			if (strdata.StartsWith("008")) return "008";
			if (strdata.StartsWith("009")) return "009";
			if (strdata.StartsWith("010")) return "010";

			return "?";
		}

		public static (byte[] pw, byte[] mk, string reqpw) CreateAuthData001(StandardNoteAPI.APIResultAuthParams apiparams, string mail, string uip)
        {

			if (apiparams.pw_func != StandardNoteAPI.PasswordFunc.pbkdf2) throw new Exception("Unsupported pw_func: " + apiparams.pw_func);

			byte[] bytes;

			if (apiparams.pw_alg == StandardNoteAPI.PasswordAlg.sha512)
			{
				bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
			}
			else if (apiparams.pw_alg == StandardNoteAPI.PasswordAlg.sha512)
			{
				bytes = PBKDF2.GenerateDerivedKey(apiparams.pw_key_size / 8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);
			}
			else
			{
				throw new Exception("Unknown pw_alg: " + apiparams.pw_alg);
			}

			var pw = bytes.Take(bytes.Length / 2).ToArray();
			var mk = bytes.Skip(bytes.Length / 2).ToArray();

			var reqpw = EncodingConverter.ByteToHexBitFiddleLowercase(pw);

			return (pw, mk, reqpw);
		}

		public static (byte[] pw, byte[] mk, byte[] ak, string reqpw) CreateAuthData002(StandardNoteAPI.APIResultAuthParams apiparams, string uip)
		{
			if (apiparams.pw_func != StandardNoteAPI.PasswordFunc.pbkdf2) throw new Exception("Unknown pw_func: " + apiparams.pw_func);

			byte[] bytes = PBKDF2.GenerateDerivedKey(768 / 8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(apiparams.pw_salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);

			var pw = bytes.Skip(0 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();
			var mk = bytes.Skip(1 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();
			var ak = bytes.Skip(2 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();

			var reqpw = EncodingConverter.ByteToHexBitFiddleUppercase(pw).ToLower();

			return (pw, mk, ak, reqpw);
		}

		public static (byte[] pw, byte[] mk, byte[] ak, string reqpw) CreateAuthData003(StandardNoteAPI.APIResultAuthParams apiparams, string mail, string uip)
		{
			if (apiparams.pw_cost < 100000) throw new StandardNoteAPIException($"Account pw_cost is too small ({apiparams.pw_cost})");

			var salt = StandardNoteCrypt.SHA256Hex(string.Join(":", mail, "SF", "003", apiparams.pw_cost.ToString(), apiparams.pw_nonce));
			byte[] bytes = PBKDF2.GenerateDerivedKey(768 / 8, Encoding.UTF8.GetBytes(uip), Encoding.UTF8.GetBytes(salt), apiparams.pw_cost, PBKDF2.HMACType.SHA512);

			var pw = bytes.Skip(0 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();
			var mk = bytes.Skip(1 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();
			var ak = bytes.Skip(2 * (bytes.Length / 3)).Take(bytes.Length / 3).ToArray();

			var reqpw = EncodingConverter.ByteToHexBitFiddleUppercase(pw).ToLower();

			return (pw, mk, ak, reqpw);
		}

		public static (byte[] mk, byte[] sp, string reqpw) CreateAuthData004(StandardNoteAPI.APIResultAuthParams apiparams, string mail, string uip)
		{
			var salt = StandardNoteCrypt.SHA256Bytes(string.Join(":", apiparams.identifier, apiparams.pw_nonce)).Take(128 / 8).ToArray();

			var derivedKey = ANCrypt.Argon2(Encoding.UTF8.GetBytes(uip), salt, 5, 64 * 1024, 64);

			var masterKey = derivedKey.Skip(00).Take(32).ToArray();
			var serverPassword = derivedKey.Skip(32).Take(32).ToArray();

			var requestPassword = EncodingConverter.ByteToHexBitFiddleLowercase(serverPassword);

			return (masterKey, serverPassword, requestPassword);
		}
	}
}
