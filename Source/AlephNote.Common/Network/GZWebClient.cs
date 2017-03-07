using System;
using System.Net;

namespace AlephNote
{
	/// <summary>
	/// http://stackoverflow.com/a/4914874/1761622
	/// </summary>
	public class GZWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
			if (request == null) return null;
			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			return request;
		}
	}
}
