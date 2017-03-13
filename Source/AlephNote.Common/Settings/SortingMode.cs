using AlephNote.Common.Settings;

namespace AlephNote.Settings
{
	public enum SortingMode
	{
		[EnumDescriptor("None")]
		None,

		[EnumDescriptor("Title")]
		ByName,

		[EnumDescriptor("Creation date")]
		ByCreationDate,

		[EnumDescriptor("Last modified date")]
		ByModificationDate,
	}
}
