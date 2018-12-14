using AlephNote.PluginInterface;
using MSHC.Lang.Attributes;

namespace AlephNote.Common.Settings.Types
{
	// (!) Werte müssen die gleichen sein wie bei AlephNote.PluginInterface.ConflictResolutionStrategyConfig
	public enum ConflictResolutionStrategyConfig
	{
		[EnumDescriptor("Use client version, override server")]
		UseClientVersion = 1,

		[EnumDescriptor("Use server version, override client")]
		UseServerVersion = 2,

		[EnumDescriptor("Use client version, create conflict note")]
		UseClientCreateConflictFile = 3,

		[EnumDescriptor("Use server version, create conflict note")]
		UseServerCreateConflictFile = 4,

		[EnumDescriptor("Show conflict resolution dialog")]
		ManualMerge = 5,
	}

	public static class ConflictResolutionStrategyHelper
	{
		public static ConflictResolutionStrategy ToInterfaceType(ConflictResolutionStrategyConfig cfg)
		{
			return (ConflictResolutionStrategy)cfg;
		}
	}
}
