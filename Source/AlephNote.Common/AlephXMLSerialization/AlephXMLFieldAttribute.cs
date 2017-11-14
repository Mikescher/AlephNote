using System;

namespace AlephNote.Settings
{
	class AlephXMLFieldAttribute : Attribute
	{
		public bool Encrypted { get; set; }

		public AlephXMLFieldAttribute()
		{
			Encrypted = false;
		}
	}
}
