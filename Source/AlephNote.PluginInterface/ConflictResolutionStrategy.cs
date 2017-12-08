namespace AlephNote.PluginInterface
{
	public enum ConflictResolutionStrategy
	{
		// Use client version, override server
		UseClientVersion = 1,

		// Use server version, override client
		UseServerVersion = 2,

		// Use client version, create conflict note
		UseClientCreateConflictFile = 3,

		// Use server version, create conflict note
		UseServerCreateConflictFile = 4,
	}
}
