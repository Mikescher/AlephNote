using System;

namespace AlephNote.PluginInterface
{
	public delegate void NoteChangedEventHandler(object sender, NoteChangedEventArgs e);

	public class NoteChangedEventArgs : EventArgs
	{
		public readonly string PropertyName;
		public readonly INote Note;

		public NoteChangedEventArgs(INote note, string propertyName)
		{
			PropertyName = propertyName;
			Note = note;
		}
	} 
}
