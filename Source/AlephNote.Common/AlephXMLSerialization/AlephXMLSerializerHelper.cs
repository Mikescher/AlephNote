using System;
using System.Text;
using AlephNote.Common.Encryption;
using AlephNote.Common.Settings;

namespace AlephNote.Common.AlephXMLSerialization
{
	public static class AlephXMLSerializerHelper
	{
		public static string Encrypt(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return string.Empty;

			return Convert.ToBase64String(AESThenHMAC.SimpleEncryptWithPassword(Encoding.UTF32.GetBytes(data), AppSettings.ENCRYPTION_KEY));
		}

		public static string Decrypt(string data)
		{
			if (string.IsNullOrWhiteSpace(data)) return string.Empty;

			return Encoding.UTF32.GetString(AESThenHMAC.SimpleDecryptWithPassword(Convert.FromBase64String(data), AppSettings.ENCRYPTION_KEY));
		}
	}
}
