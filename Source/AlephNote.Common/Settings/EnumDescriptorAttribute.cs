using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephNote.Common.Settings
{
	[AttributeUsage(AttributeTargets.All)]
	public class EnumDescriptorAttribute : Attribute
	{
		public readonly string Description;

		public EnumDescriptorAttribute(string value)
		{
			Description = value;
		}
	}
}
