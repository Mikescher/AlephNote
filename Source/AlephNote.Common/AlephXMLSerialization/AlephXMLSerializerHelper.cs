using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.AlephXMLSerialization
{
	public static class AlephXMLSerializerHelper
	{
		public static string Encrypt(string data, AXMLSerializationSettings opt)
		{
			return ANEncryptionHelper.SimpleEncryptWithPassword(data, AppSettings.ENCRYPTION_KEY, opt);
		}

		public static string Decrypt(string data, AXMLSerializationSettings opt)
		{
			return ANEncryptionHelper.SimpleDecryptWithPassword(data, AppSettings.ENCRYPTION_KEY, opt);
		}
	}
}
