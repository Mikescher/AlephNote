using System;
using System.Security.Cryptography;

namespace AlephNote.PluginInterface.Util
{
	public static class PBKDF2
	{
		public enum HMACType { SHA1, SHA256, SHA384, SHA512 };

		public static HMAC GetMac(HMACType func, byte[] key)
		{
			switch (func)
			{
				case HMACType.SHA1:
					return new HMACSHA1(key);
				case HMACType.SHA256:
					return new HMACSHA256(key);
				case HMACType.SHA384:
					return new HMACSHA384(key);
				case HMACType.SHA512:
					return new HMACSHA512(key);
				default:
					throw new ArgumentOutOfRangeException(nameof(func), func, null);
			}
		}

		public static byte[] GenerateDerivedKey(int keySize, byte[] password, byte[] salt, int iterations, HMACType func)
		{
			using (var mac = GetMac(func, password))
			{
				var hLen = mac.HashSize / 8;
				var l = (keySize + hLen - 1) / hLen;
				var outBytes = new byte[l * hLen];
				byte[] p = new byte[salt.Length + 4];
				Array.Copy(salt, 0, p, 0, salt.Length);

				for (int i = 1; i <= l; i++)
				{
					p[p.Length - 4] = (byte)((uint)i >> 24);
					p[p.Length - 3] = (byte)((uint)i >> 16);
					p[p.Length - 2] = (byte)((uint)i >> 8);
					p[p.Length - 1] = (byte)((uint)i >> 0);

					byte[] state = new HMACSHA512(password).ComputeHash(p);

					Array.Copy(state, 0, outBytes, (i - 1) * hLen, state.Length);

					for (int count = 1; count != iterations; count++)
					{
						state = new HMACSHA512(password).ComputeHash(state);

						for (int j = 0; j != state.Length; j++)
						{
							outBytes[(i - 1) * hLen + j] ^= state[j];
						}
					}
				}

				byte[] output = new byte[keySize];
				Buffer.BlockCopy(outBytes, 0, output, 0, keySize);
				return output;
			}
		}
	}
}
