using AlephNote.Common.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
