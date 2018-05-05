using System;
using System.Net;

namespace AlephNote.PluginInterface.Exceptions
{
	public class RestException : RemoteException
	{
		public readonly string ShortMessage;

		public readonly bool IsConnectionProblem;

		public RestException(string shortmessage, string message, bool isconnerr) : base(message) { ShortMessage = shortmessage; IsConnectionProblem = isconnerr; }
		public RestException(string shortmessage, string message, Exception inner, bool isconnerr) : base(message, inner) { ShortMessage = shortmessage; IsConnectionProblem = isconnerr; }

		public RestException(string message, bool isconnerr) : base(message) { ShortMessage = message; IsConnectionProblem = isconnerr; }
		public RestException(string message, Exception inner, bool isconnerr) : base(message, inner) { ShortMessage = message; IsConnectionProblem = isconnerr; }
	}
}
