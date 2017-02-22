using MSHC.Math.Encryption;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AlephNote.Plugins.StandardNote
{
	static class StandardNoteCrypt
	{
		public static string DecryptContent(string encContent, string encItemKey, string authHash, byte[] masterkey)
		{
			if (encContent.StartsWith("001"))
			{
				var item_key = EncodingConverter.StringToByteArrayCaseInsensitive(Encoding.ASCII.GetString(AESEncryption.DecryptCBC256(Convert.FromBase64String(encItemKey), masterkey, new byte[16])));

				var ek = item_key.Take(item_key.Length / 2).ToArray();
				var ak = item_key.Skip(item_key.Length / 2).ToArray();

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
	}
}
