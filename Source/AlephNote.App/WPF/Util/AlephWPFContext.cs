using System;
using AlephNote.Common.Util;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;
using AlephNote.WPF.Windows;

namespace AlephNote.WPF.Util
{
	public class AlephWPFContext : IAlephAppContext
	{
		IReadonlyAlephSettings IAlephAppContext.GetSettings() => MainWindow.Instance?.Settings;

		IAlephLogger IAlephAppContext.GetLogger() => LoggerSingleton.Inst;

		Tuple<int, int, int, int> IAlephAppContext.GetAppVersion() => Tuple.Create(App.APP_VERSION.Major, App.APP_VERSION.Minor, App.APP_VERSION.Build, App.APP_VERSION.Revision);

		bool IAlephAppContext.IsDebugMode
		{
			get => App.DebugMode;
			set => App.DebugMode = value;
		}
	}
}
