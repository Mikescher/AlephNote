using AlephNote.Common.Plugins;
using System;

namespace AlephNote.Common.Util
{
	public static class PluginManagerSingleton
	{
		private static readonly object _lock = new object();

		public static IPluginManager Inst { get; private set; }

		public static void Register(IPluginManager man)
		{
			lock (_lock)
			{
				if (Inst != null) throw new NotSupportedException();

				Inst = man;
			}
		}

	}
}
