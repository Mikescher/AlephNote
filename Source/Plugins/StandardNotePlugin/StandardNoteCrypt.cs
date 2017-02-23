using MSHC.Math.Encryption;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AlephNote.Plugins.StandardNote
{
	static class StandardNoteCrypt
	{
		private static readonly RandomNumberGenerator RNG = new RNGCryptoServiceProvider();

		public class EncryptResult { public string item_key, enc_item_key, auth_hash, enc_content; }

		public static string DecryptContent(string encContent, string encItemKey, string authHash, byte[] masterkey, out string item_key_b64)
		{
			if (encContent.StartsWith("001"))
			{
				var item_key = EncodingConverter.StringToByteArrayCaseInsensitive(Encoding.ASCII.GetString(AESEncryption.DecryptCBC256(Convert.FromBase64String(encItemKey), masterkey, new byte[16])));

				var ek = item_key.Take(item_key.Length / 2).ToArray();
				var ak = item_key.Skip(item_key.Length / 2).ToArray();

				var realHash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(encContent), ak));

				if (realHash.ToLower() != authHash.ToLower()) throw new StandardNoteAPIException("Decrypting content failed - hash mismatch");

				var c = AESEncryption.DecryptCBC256(Convert.FromBase64String(encContent.Substring(3)), ek, null);

				item_key_b64 = Convert.ToBase64String(item_key);

				return Encoding.UTF8.GetString(c);
			}
			else
			{
				if (string.IsNullOrWhiteSpace(encItemKey))
					item_key_b64 = "";
				else
					item_key_b64 = Convert.ToBase64String(EncodingConverter.StringToByteArrayCaseInsensitive(Encoding.ASCII.GetString(AESEncryption.DecryptCBC256(Convert.FromBase64String(encItemKey), masterkey, new byte[16]))));

				return Encoding.UTF8.GetString(Convert.FromBase64String(encContent));
			}
		}

		public static byte[] AuthSHA256(byte[] content, byte[] ak)
		{
			using (HMACSHA256 hmac = new HMACSHA256(ak))
			{
				return hmac.ComputeHash(content);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>[item_key, enc_item_key, auth_hash, content]</returns>
		public static EncryptResult EncryptContent(string encKey, string content, byte[] mk, bool encrypt)
		{
			byte[] itemKey = new byte[512 / 8];

			if (string.IsNullOrWhiteSpace(encKey))
			{
				RNG.GetBytes(itemKey);
			}
			else
			{
				itemKey = Convert.FromBase64String(encKey);
			}

			var ek = itemKey.Take(itemKey.Length / 2).ToArray();
			var ak = itemKey.Skip(itemKey.Length / 2).ToArray();

			string encContent;
			if (encrypt)
				encContent = "001" + Convert.ToBase64String(AESEncryption.EncryptCBC256(Encoding.UTF8.GetBytes(content), ek, null));
			else
				encContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));

			var authHash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(encContent), ak));

			var encItemKey = Convert.ToBase64String(AESEncryption.EncryptCBC256(Encoding.UTF8.GetBytes(EncodingConverter.ByteToHexBitFiddleLowercase(itemKey)), mk, null));

			return new EncryptResult
			{
				item_key = Convert.ToBase64String(itemKey),
				enc_item_key = encItemKey,
				enc_content = encContent,
				auth_hash = authHash,
			};
		}
	}
}
