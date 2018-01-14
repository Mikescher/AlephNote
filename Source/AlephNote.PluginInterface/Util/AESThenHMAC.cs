using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AlephNote.PluginInterface.Objects.AXML;

namespace AlephNote.PluginInterface.Util
{
	/// <summary>
	/// http://stackoverflow.com/a/10366194/1761622
	/// https://gist.github.com/jbtule/4336842#file-aesthenhmac-cs
	/// https://github.com/Mikescher/Passpad
	/// </summary>
	public static class AESThenHMAC
	{
		//Preconfigured Encryption Parameters
		private const int BlockBitSize = 128;
		private const int KeyBitSize = 256;

		//Preconfigured Password Key Derivation Parameters
		private const int SaltBitSize = 64;
		private const int Iterations_v1 = 10000;

		private const int AES_IV_SIZE = 16;
		private const int AES_KEY_SIZE = 32;

		private static readonly byte[] AES_SALT =  // same as PassPad
		{
			0xEF, 0x03, 0x33, 0xC4, 0xEB, 0x4A, 0x06, 0x51,
			0x01, 0x17, 0xF8, 0x2E, 0xB4, 0x28, 0x60, 0x33,
			0x06, 0x1E, 0xBC, 0xF2, 0x38, 0x36, 0x62, 0x27,
			0x24, 0x65, 0x72, 0x06, 0xFE, 0xAD, 0x9C, 0xB6,
		};

		private static byte[] EncodeBytes(byte[] data, string password)
		{
			using (var aes = Aes.Create())
			{
				if (aes == null) throw new Exception("AES instantiation failed");

				var key = HashPassword(password, AES_KEY_SIZE);
				var iv = new byte[AES_IV_SIZE];

				aes.KeySize = KeyBitSize;
				aes.BlockSize = BlockBitSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				aes.Key = key;
				aes.IV = iv;

				var encryptor = aes.CreateEncryptor(key, iv);
				using (var msEncrypt = new MemoryStream())
				{
					using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					using (var swEncrypt = new BinaryWriter(csEncrypt))
					{
						swEncrypt.Write(data);
					}

					return msEncrypt.ToArray();
				}
			}
		}

		private static byte[] DecodeBytes(byte[] data, string password)
		{
			using (var aes = Aes.Create())
			{
				if (aes == null) throw new Exception("AES instantiation failed");

				var key = HashPassword(password, AES_KEY_SIZE);
				var iv = new byte[AES_IV_SIZE];

				aes.KeySize = KeyBitSize;
				aes.BlockSize = BlockBitSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				aes.Key = key;
				aes.IV = iv;

				var decryptor = aes.CreateDecryptor(key, iv);
				using (var msDecrypt = new MemoryStream(data))
				using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				{
					return ReadToEnd(csDecrypt);
				}

			}
		}

		private static byte[] HashPassword(string password, int size)
		{
			using (var rfc2898 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), AES_SALT, 1))
			{
				return rfc2898.GetBytes(size);
			}
		}

		private static byte[] ReadToEnd(Stream stream)
		{
			byte[] buffer = new byte[32768];
			using (MemoryStream ms = new MemoryStream())
			{
				for (; ; )
				{
					int read = stream.Read(buffer, 0, buffer.Length);
					if (read <= 0)
						return ms.ToArray();
					ms.Write(buffer, 0, read);
				}
			}
		}
		
		private static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
		{
			//User Error Checks
			if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "cryptKey");

			if (authKey == null || authKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "authKey");

			if (secretMessage == null || secretMessage.Length < 1)
				throw new ArgumentException("Secret Message Required!", "secretMessage");

			//non-secret payload optional
			nonSecretPayload = nonSecretPayload ?? new byte[] { };

			byte[] cipherText;
			byte[] iv;

			using (var aes = Aes.Create())
			{
				if (aes == null) throw new Exception("AES instantiation failed");

				aes.KeySize = KeyBitSize;
				aes.BlockSize = BlockBitSize;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				//Use random IV
				aes.GenerateIV();
				iv = aes.IV;

				using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
				using (var cipherStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
					using (var binaryWriter = new BinaryWriter(cryptoStream))
					{
						//Encrypt Data
						binaryWriter.Write(secretMessage);
					}

					cipherText = cipherStream.ToArray();
				}

			}

			//Assemble encrypted message and add authentication
			using (var hmac = new HMACSHA256(authKey))
			using (var encryptedStream = new MemoryStream())
			{
				using (var binaryWriter = new BinaryWriter(encryptedStream))
				{
					//Prepend non-secret payload if any
					binaryWriter.Write(nonSecretPayload);
					//Prepend IV
					binaryWriter.Write(iv);
					//Write Ciphertext
					binaryWriter.Write(cipherText);
					binaryWriter.Flush();

					//Authenticate all data
					var tag = hmac.ComputeHash(encryptedStream.ToArray());
					//Postpend tag
					binaryWriter.Write(tag);
				}
				return encryptedStream.ToArray();
			}

		}
		
		private static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
		{

			//Basic Usage Error Checks
			if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), "cryptKey");

			if (authKey == null || authKey.Length != KeyBitSize / 8)
				throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), "authKey");

			if (encryptedMessage == null || encryptedMessage.Length == 0)
				throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

			using (var hmac = new HMACSHA256(authKey))
			{
				var sentTag = new byte[hmac.HashSize / 8];
				//Calculate Tag
				var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
				var ivLength = (BlockBitSize / 8);

				//if message length is to small just return null
				if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
					return null;

				//Grab Sent Tag
				Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

				//Compare Tag with constant time comparison
				var compare = 0;
				for (var i = 0; i < sentTag.Length; i++)
					compare |= sentTag[i] ^ calcTag[i];

				//if message doesn't authenticate return null
				if (compare != 0)
					return null;

				using (var aes = Aes.Create())
				{
					if (aes == null) throw new Exception("AES instantiation failed");

					aes.KeySize = KeyBitSize;
					aes.BlockSize = BlockBitSize;
					aes.Mode = CipherMode.CBC;
					aes.Padding = PaddingMode.PKCS7;

					//Grab IV from message
					var iv = new byte[ivLength];
					Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

					using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
					using (var plainTextStream = new MemoryStream())
					{
						using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
						using (var binaryWriter = new BinaryWriter(decrypterStream))
						{
							//Decrypt Cipher Text from Message
							binaryWriter.Write(
								encryptedMessage,
								nonSecretPayloadLength + iv.Length,
								encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
							);
						}
						//Return Plain Text
						return plainTextStream.ToArray();
					}
				}
			}
		}
		
		public static string SimpleEncryptWithPassword(string secretMessage, string password, AXMLSerializationSettings opt)
		{
			if ((opt & AXMLSerializationSettings.UseEncryption) == 0) return secretMessage;
			if (string.IsNullOrWhiteSpace(secretMessage)) return string.Empty;

			var encbytes = EncodeBytes(Encoding.UTF32.GetBytes(secretMessage), password);

			return ":02:"+Convert.ToBase64String(encbytes);
		}
		
		public static string SimpleDecryptWithPassword(string encryptedMessageStr, string password, AXMLSerializationSettings opt)
		{
			if ((opt & AXMLSerializationSettings.UseEncryption) == 0) return encryptedMessageStr;
			if (string.IsNullOrWhiteSpace(encryptedMessageStr)) return string.Empty;

			if (!encryptedMessageStr.StartsWith(":"))
			{
				// VERSION 1
				var encryptedMessage = Convert.FromBase64String(encryptedMessageStr);

				var cryptSalt = new byte[SaltBitSize / 8];
				var authSalt = new byte[SaltBitSize / 8];

				//Grab Salt from Non-Secret Payload
				Array.Copy(encryptedMessage, 0, cryptSalt, 0, cryptSalt.Length);
				Array.Copy(encryptedMessage, 0 + cryptSalt.Length, authSalt, 0, authSalt.Length);

				byte[] cryptKey;
				byte[] authKey;

				//Generate crypt key
				using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, Iterations_v1))
				{
					cryptKey = generator.GetBytes(KeyBitSize / 8);
				}
				//Generate auth key
				using (var generator = new Rfc2898DeriveBytes(password, authSalt, Iterations_v1))
				{
					authKey = generator.GetBytes(KeyBitSize / 8);
				}

				return Encoding.UTF32.GetString(SimpleDecrypt(encryptedMessage, cryptKey, authKey, cryptSalt.Length + authSalt.Length + 0));
			}
			else if (encryptedMessageStr.StartsWith(":02:"))
			{
				// VERSION 2
				encryptedMessageStr = encryptedMessageStr.Substring(4);

				var encbytes = Convert.FromBase64String(encryptedMessageStr);
				var rawbytes = DecodeBytes(encbytes, password);

				return Encoding.UTF32.GetString(rawbytes);
			}
			else
			{
				throw new ArgumentException("Unknown encryption version: " + encryptedMessageStr, nameof(encryptedMessageStr));
			}

		}

	}
}
