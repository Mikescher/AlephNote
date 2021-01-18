using System;

namespace AlephNote.Common.AlephXMLSerialization
{
	public class AlephXMLFieldAttribute : Attribute
	{
		public bool Encrypted { get; set; }

		public bool ReconnectRepo { get; set; } = false;
		public bool RefreshNotesViewTemplate { get; set; } = false;
		public bool RefreshNotesControlView { get; set; } = false;
		public bool RefreshNotesTheme { get; set; } = false;
		public string XMLName { get; set; } = null; // [null] == use property name
		public bool IsAdvanced { get; set; } = false;

		public AlephXMLFieldAttribute()
		{
			Encrypted = false;
		}
	}
}
