using AlephNote.PluginInterface;
using System;

namespace AlephNote.Plugins.StandardNote
{
	public class StandardNoteAPIException : RemoteException
	{
		public StandardNoteAPIException(string message) : base(message) { }
		public StandardNoteAPIException(string message, Exception inner) : base(message, inner) { }
	}
}
