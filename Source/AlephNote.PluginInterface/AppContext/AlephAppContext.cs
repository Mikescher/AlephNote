using System;
using AlephNote.PluginInterface.AppContext;

namespace AlephNote.PluginInterface
{
	public class AlephAppContext
	{
		private static IAlephAppContext _context;

		public static void Init(IAlephAppContext c)
		{
			_context = c;
		}

		public static IReadonlyAlephSettings Settings => _context.GetSettings();

		public static IAlephLogger Logger => _context.GetLogger();
		
		public static Tuple<int, int, int, int> AppVersion => _context.GetAppVersion();

		public static bool DebugMode { get => _context.IsDebugMode; set => _context.IsDebugMode=value; }
	}
}
