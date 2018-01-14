using AlephNote.Common.Settings;
using AlephNote.PluginInterface.Objects.AXML;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Common.AlephXMLSerialization
{
	public static class AlephXMLSerializerHelper
	{
		public static string Encrypt(string data, AXMLSerializationSettings opt)
		{
			return AESThenHMAC.SimpleEncryptWithPassword(data, AppSettings.ENCRYPTION_KEY, opt);
		}

		public static string Decrypt(string data, AXMLSerializationSettings opt)
		{
			return AESThenHMAC.SimpleDecryptWithPassword(data, AppSettings.ENCRYPTION_KEY, opt);
		}
	}
}
