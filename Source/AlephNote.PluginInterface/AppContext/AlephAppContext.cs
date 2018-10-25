using System;

namespace AlephNote.PluginInterface.AppContext
{
	public static class AlephAppContext
	{
		private static IReadonlyAlephSettings _fallback = null;
		private static IAlephAppContext _context;

		public static void Init(IAlephAppContext c)
		{
			_context = c;
		}

		public static IReadonlyAlephSettings Settings => _context.GetSettings() ?? _fallback;

		public static IAlephLogger Logger => _context.GetLogger();
		
		public static Tuple<int, int, int, int> AppVersion => _context.GetAppVersion();

		public static bool DebugMode { get => _context.IsDebugMode; set => _context.IsDebugMode=value; }

		public static void SetFallbackSettings(IReadonlyAlephSettings s) { _fallback = s; }
	}
}
