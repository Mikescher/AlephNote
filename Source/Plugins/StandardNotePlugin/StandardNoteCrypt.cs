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

		public class EncryptResult { public string enc_item_key, auth_hash, enc_content; }

		public static string DecryptContent(string encContent, string encItemKey, string authHash, StandardNoteSessionData sdat)
		{
			if (encContent.StartsWith("000")) return DecryptContent000(encContent);
			if (encContent.StartsWith("001")) return DecryptContent001(encContent, encItemKey, authHash, sdat.RootKey_MasterKey);
			if (encContent.StartsWith("002")) return DecryptContent002(encContent, encItemKey, sdat.RootKey_MasterKey, sdat.RootKey_MasterAuthKey);
			if (encContent.StartsWith("003")) return DecryptContent003(encContent, encItemKey, sdat.RootKey_MasterKey, sdat.RootKey_MasterAuthKey);
			if (encContent.StartsWith("003")) throw new StandardNoteAPIException("Unsupported encryption scheme 004 in note content");
			if (encContent.StartsWith("004")) throw new StandardNoteAPIException("Unsupported encryption scheme 004 in note content"); //TODO
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
			return Encoding.UTF8.GetString(Convert.FromBase64String(encContent));
		}

		private static string DecryptContent001(string encContent, string encItemKey, string authHash, byte[] masterkey)
		{
			var itemKey = EncodingConverter.StringToByteArrayCaseInsensitive(Encoding.ASCII.GetString(AESEncryption.DecryptCBC256(Convert.FromBase64String(encItemKey), masterkey, new byte[16])));

			var ek = itemKey.Take(itemKey.Length / 2).ToArray();
			var ak = itemKey.Skip(itemKey.Length / 2).ToArray();

			var realHash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(encContent), ak));
			
			if (authHash == null) throw new ArgumentNullException(nameof(authHash));
			if (realHash.ToLower() != authHash.ToLower()) throw new StandardNoteAPIException("Decrypting content failed - hash mismatch");

			var c = AESEncryption.DecryptCBC256(Convert.FromBase64String(encContent.Substring(3)), ek, null);

			return Encoding.UTF8.GetString(c);
		}

		private static string DecryptContent002(string encContent, string encItemKey, byte[] masterMK, byte[] masterAK)
		{
			var item_key = Decrypt002(encItemKey, masterMK, masterAK);

			var item_ek = item_key.Substring(0, item_key.Length / 2);
			var item_ak = item_key.Substring(item_key.Length / 2, item_key.Length / 2);

			return Decrypt002(encContent, EncodingConverter.StringToByteArrayCaseInsensitive(item_ek), EncodingConverter.StringToByteArrayCaseInsensitive(item_ak));
		}

		private static string DecryptContent003(string encContent, string encItemKey, byte[] masterMK, byte[] masterAK)
		{
			var item_key = Decrypt003(encItemKey, masterMK, masterAK);

			var item_ek = item_key.Substring(0, item_key.Length / 2);
			var item_ak = item_key.Substring(item_key.Length / 2, item_key.Length / 2);

			return Decrypt003(encContent, EncodingConverter.StringToByteArrayCaseInsensitive(item_ek), EncodingConverter.StringToByteArrayCaseInsensitive(item_ak));
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
				if (local_auth_hash.ToUpper() != auth_hash.ToUpper()) throw new Exception("Item auth-hash mismatch");
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
				if (local_auth_hash.ToUpper() != auth_hash.ToUpper()) throw new Exception("Item auth-hash mismatch");
			}

			var result = AESEncryption.DecryptCBC256(Convert.FromBase64String(ciphertext), encryption_key, EncodingConverter.StringToByteArrayCaseInsensitive(IV));

			return Encoding.UTF8.GetString(result);
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


		public static EncryptResult EncryptContent(string content, Guid uuid, StandardNoteSessionData sdat)
		{
			if (sdat.Version == "001") return EncryptContent001(content,       sdat.RootKey_MasterKey);
			if (sdat.Version == "002") return EncryptContent002(content, uuid, sdat.RootKey_MasterKey, sdat.RootKey_MasterAuthKey);
			if (sdat.Version == "003") return EncryptContent003(content, uuid, sdat.RootKey_MasterKey, sdat.RootKey_MasterAuthKey);
			if (sdat.Version == "004") throw new StandardNoteAPIException("Unsupported encryption scheme 004 in note content");
			if (sdat.Version == "005") throw new StandardNoteAPIException("Unsupported encryption scheme 005 in note content");
			if (sdat.Version == "006") throw new StandardNoteAPIException("Unsupported encryption scheme 006 in note content");
			if (sdat.Version == "007") throw new StandardNoteAPIException("Unsupported encryption scheme 007 in note content");
			if (sdat.Version == "008") throw new StandardNoteAPIException("Unsupported encryption scheme 008 in note content");
			if (sdat.Version == "009") throw new StandardNoteAPIException("Unsupported encryption scheme 009 in note content");
			if (sdat.Version == "010") throw new StandardNoteAPIException("Unsupported encryption scheme 010 in note content");

			throw new Exception("Unsupported encryption scheme: " + sdat.Version);
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
	}
}
