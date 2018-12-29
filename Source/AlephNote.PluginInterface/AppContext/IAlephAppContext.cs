using System;

namespace AlephNote.PluginInterface.AppContext
{
	public interface IAlephAppContext
	{
		IReadonlyAlephSettings GetSettings();
		AlephLogger GetLogger();
		Tuple<int, int, int, int> GetAppVersion();

		bool IsDebugMode { get; set; }

	}
}
