using System.IO;
using System.Security.Cryptography;

namespace AlephNote.PluginInterface.Util
{
	public static class AESEncryption
	{
		public static byte[] DecryptCBC256(byte[] data, byte[] key, byte[] iv)
		{
			if (iv == null) iv = new byte[16];
			
			using (var rj = Aes.Create())
			{
				rj.Key = key;
				rj.IV = iv;
				rj.Mode = CipherMode.CBC;
				rj.Padding = PaddingMode.PKCS7;

				using (var memoryStream = new MemoryStream(data))
				using (var cryptoStream = new CryptoStream(memoryStream, rj.CreateDecryptor(key, iv), CryptoStreamMode.Read))
				{
					byte[] buffer = new byte[16 * 1024];
					using (MemoryStream ms = new MemoryStream())
					{
						int read;
						while ((read = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
						{
							ms.Write(buffer, 0, read);
						}
						return ms.ToArray();
					}
				}
			}
		}

		public static byte[] EncryptCBC256(byte[] message, byte[] key, byte[] iv)
		{
			if (iv == null) iv = new byte[16];

			using (var rj = Aes.Create())
			{
				rj.Key = key;
				rj.IV = iv;
				rj.Mode = CipherMode.CBC;
				rj.Padding = PaddingMode.PKCS7;

				using (var ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, rj.CreateEncryptor(key, iv), CryptoStreamMode.Write))
					{
						cs.Write(message, 0, message.Length);
						cs.Flush();
					}
					return ms.ToArray();
				}

			}
		}
	}
}
