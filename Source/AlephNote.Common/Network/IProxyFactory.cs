using System.Net;

namespace AlephNote.Common.Network
{
	public interface IProxyFactory
	{
		IWebProxy Build();
		IWebProxy Build(string host, int port);
		IWebProxy Build(string host, int port, string username, string password);
	}
}
