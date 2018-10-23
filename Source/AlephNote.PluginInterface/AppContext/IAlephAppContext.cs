using System;

namespace AlephNote.PluginInterface.AppContext
{
	public interface IAlephAppContext
	{
		IReadonlyAlephSettings GetSettings();
		IAlephLogger GetLogger();
		Tuple<int, int, int, int> GetAppVersion();

		bool IsDebugMode { get; set; }

	}
}
