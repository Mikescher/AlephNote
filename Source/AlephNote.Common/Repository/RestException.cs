using AlephNote.PluginInterface;
using System;

namespace AlephNote.Repository
{
	public class RestException : RemoteException
	{
		public RestException(string message) : base(message) { }
		public RestException(string message, Exception inner) : base(message, inner) { }
	}
}
