
using System.ComponentModel;

namespace AlephNote.Settings
{
	public enum SortingMode
	{
		[Description("None")]
		None,

		[Description("Title")]
		ByName,

		[Description("Creation date")]
		ByCreationDate,

		[Description("Last modified date")]
		ByModificationDate,
	}
}
