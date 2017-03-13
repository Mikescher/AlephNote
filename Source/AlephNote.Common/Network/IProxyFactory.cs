using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AlephNote.Common.Network
{
	public interface IProxyFactory
	{
		IWebProxy Build();
		IWebProxy Build(string host, int port);
		IWebProxy Build(string host, int port, string username, string password);
	}
}
