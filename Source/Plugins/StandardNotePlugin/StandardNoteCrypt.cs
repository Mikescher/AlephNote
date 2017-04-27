using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.StandardNote
{
	static class StandardNoteCrypt
	{
		private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();

		public class EncryptResult { public string item_key, enc_item_key, auth_hash, enc_content; }

		public static string DecryptContent(string encContent, string encItemKey, string authHash, byte[] masterkey)
		{
			if (encContent.StartsWith("001"))
			{
				var itemKey = EncodingConverter.StringToByteArrayCaseInsensitive(Encoding.ASCII.GetString(AESEncryption.DecryptCBC256(Convert.FromBase64String(encItemKey), masterkey, new byte[16])));

				var ek = itemKey.Take(itemKey.Length / 2).ToArray();
				var ak = itemKey.Skip(itemKey.Length / 2).ToArray();

				var realHash = EncodingConverter.ByteToHexBitFiddleLowercase(AuthSHA256(Encoding.UTF8.GetBytes(encContent), ak));

				if (realHash.ToLower() != authHash.ToLower()) throw new StandardNoteAPIException("Decrypting content failed - hash mismatch");

				var c = AESEncryption.DecryptCBC256(Convert.FromBase64String(encContent.Substring(3)), ek, null);

				return Encoding.UTF8.GetString(c);
			}
			else
			{
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
		public static EncryptResult EncryptContent(string content, byte[] mk, bool encrypt)
		{
			byte[] itemKey = new byte[512 / 8];

			RNG.GetBytes(itemKey);

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
