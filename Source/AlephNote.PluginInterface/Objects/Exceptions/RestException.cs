using System;

namespace AlephNote.PluginInterface.Exceptions
{
	public class RestException : RemoteException
	{
		public readonly string ShortMessage;

		public RestException(string shortmessage, string message) : base(message) { ShortMessage = shortmessage; }
		public RestException(string shortmessage, string message, Exception inner) : base(message, inner) { ShortMessage = shortmessage; }

		public RestException(string message) : base(message) { ShortMessage = message; }
		public RestException(string message, Exception inner) : base(message, inner) { ShortMessage = message; }
	}
}
