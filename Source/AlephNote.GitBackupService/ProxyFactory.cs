using System.Net;
using AlephNote.Common.Network;

namespace AlephNote.GitBackupService;

public class ProxyFactory: IProxyFactory
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