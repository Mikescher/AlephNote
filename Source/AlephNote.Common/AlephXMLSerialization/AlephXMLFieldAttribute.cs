using System;

namespace AlephNote.Common.AlephXMLSerialization
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
