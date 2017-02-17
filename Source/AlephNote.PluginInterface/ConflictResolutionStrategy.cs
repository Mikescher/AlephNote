using System.ComponentModel;

namespace AlephNote.PluginInterface
{
	public enum ConflictResolutionStrategy
	{
		[Description("Use client version, override server")]
		UseClientVersion,

		[Description("Use server version, override client")]
		UseServerVersion,

		[Description("Use client version, create conflict note")]
		UseClientCreateConflictFile,

		[Description("Use server version, create conflict note")]
		UseServerCreateConflictFile,
	}
}
