namespace AlephNote.PluginInterface
{
	public class RemotePluginWrapper
	{
		public readonly IRemotePlugin Plugin;

		public RemotePluginWrapper(IRemotePlugin p)
		{
			Plugin = p;
		}

		public override string ToString()
		{
			return Plugin.DisplayTitleShort;
		}

	}
}
