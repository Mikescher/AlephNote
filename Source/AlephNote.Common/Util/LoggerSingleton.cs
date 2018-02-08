using AlephNote.PluginInterface;
using System;

namespace AlephNote.Common.Util
{
	public static class LoggerSingleton
	{
		private static readonly object _lock = new object();

		public static IAlephLogger Inst { get; private set; }

		public static void Register(IAlephLogger log)
		{
			lock (_lock)
			{
				if (Inst != null) throw new NotSupportedException();

				Inst = log;
			}
		}

	}
}
