using AlephNote.Common.Network;
using System.Net;

namespace AlephNote.Impl
{
	class ProxyFactory : IProxyFactory
	{
		public IWebProxy Build(string host, int port)
		{
			return new WebProxy(host, port);
		}

		public IWebProxy Build(string host, int port, string username, string password)
		{
			return new WebProxy(host, port)
			{
				Credentials = new NetworkCredential(username, password)
			};
		}

		public IWebProxy Build()
		{
			return new WebProxy();
		}
	}
}
