using System.Security.Cryptography;
using System.Text;

namespace AlephNote.PluginInterface.Util
{
	public static class ChecksumHelper
	{
		public static string MD5(string input)
		{
			using (MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				return EncodingConverter.ByteToHexBitFiddleUppercase(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
			}
		}
	}
}
