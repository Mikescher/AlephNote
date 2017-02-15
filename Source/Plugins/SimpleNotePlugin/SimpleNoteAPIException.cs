using AlephNote.PluginInterface;
using System;

namespace AlephNote.Plugins.SimpleNote
{
	public class SimpleNoteAPIException : RemoteException
	{
		public SimpleNoteAPIException(string message) : base(message) { }
		public SimpleNoteAPIException(string message, Exception inner) : base(message, inner) { }
	}
}
