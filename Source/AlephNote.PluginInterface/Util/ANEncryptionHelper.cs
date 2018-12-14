using AlephNote.PluginInterface.Objects.AXML;
using MSHC.Encryption;

namespace AlephNote.PluginInterface.Util
{
	public static class ANEncryptionHelper
	{
		public static string SimpleEncryptWithPassword(string secretMessage, string password, AXMLSerializationSettings opt)
		{
			if ((opt & AXMLSerializationSettings.UseEncryption) == 0) return secretMessage;
			return AESThenHMAC.SimpleEncryptWithPassword(secretMessage, password);
		}
		
		public static string SimpleDecryptWithPassword(string encryptedMessageStr, string password, AXMLSerializationSettings opt)
		{
			if ((opt & AXMLSerializationSettings.UseEncryption) == 0) return encryptedMessageStr;
			return AESThenHMAC.SimpleDecryptWithPassword(encryptedMessageStr, password);
		}
	}
}
