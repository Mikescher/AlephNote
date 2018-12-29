using AlephNote.Log;
using AlephNote.WPF.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using AlephNote.Impl;
using AlephNote.Common.Themes;
using AlephNote.Common.Util;
using AlephNote.Common.Settings;
using System.Windows;
using System.Globalization;
using System.Threading;
using AlephNote.Common.Settings.Types;
using AlephNote.Native;
using AlephNote.PluginInterface;
using AlephNote.PluginInterface.AppContext;
using AlephNote.WPF.Dialogs;
using AlephNote.WPF.Util;
using MSHC.Util.Helper;

namespace AlephNote
{
	public partial class App
	{
		public static readonly Version APP_VERSION = GetInformationalVersion();

		public static string AppVersionProperty { get { return APP_VERSION.Revision == 0 ? APP_VERSION.ToString(3) : (APP_VERSION.ToString(4) + " BETA"); } }

		public static readonly Random GlobalRandom = new Random();

		public static CommandLineArguments Args;

		public static BasicWPFLogger Logger => (BasicWPFLogger)LoggerSingleton.Inst;

		public static readonly ThemeCache    Themes    = new ThemeCache();
		public static readonly PluginManager PluginMan = new PluginManager();

		public static bool DebugMode = false;

		public static bool IsFirstLaunch = false;

		public static bool IsUpdateMigration = false;
		public static Version UpdateMigrationFrom;
		public static Version UpdateMigrationTo;
		
		public App()
		{
			//NOP
		}

		private void Application_Startup(object sender, StartupEventArgs suea)
		{
			DispatcherUnhandledException += AppDispatcherUnhandledException;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			
			AlephAppContext.Init(new AlephWPFContext());

			LoggerSingleton.Register(new EventLogger());
			PluginManagerSingleton.Register(PluginMan);
			ThemeManager.Register(Themes);

			Args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

			if (Args.Contains("debug")) DebugMode = true;

			UpdateMigrationFrom = Args.GetVersionDefault("migration_from", default(Version));
			UpdateMigrationTo   = Args.GetVersionDefault("migration_to", default(Version));

			IsUpdateMigration = (UpdateMigrationFrom != default(Version)) && (UpdateMigrationTo != default(Version)) && Args.Contains("updated");

#if DEBUG
			DebugMode = true;
#endif
			PluginManager.Inst.LoadPlugins(AppDomain.CurrentDomain.BaseDirectory);
			App.Themes.Init(Args.GetStringDefault("themes", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes")));

			AppSettings settings;
			try
			{
				if (File.Exists(AppSettings.PATH_SETTINGS))
				{
					settings = AppSettings.Load(AppSettings.PATH_SETTINGS);
					if (IsUpdateMigration)
					{
						settings.Migrate(UpdateMigrationFrom, UpdateMigrationTo);
						settings.Save();
					}
				}
				else
				{
					settings = AppSettings.CreateEmpty(AppSettings.PATH_SETTINGS);
					settings.Save();

					IsFirstLaunch = true;
				}
			}
			catch (Exception e)
			{
				ExceptionDialog.Show(null, "Could not load settings", "Could not load settings from " + AppSettings.PATH_SETTINGS, e);
				settings = AppSettings.CreateEmpty(AppSettings.PATH_SETTINGS);
			}

			AlephAppContext.SetFallbackSettings(settings); // [HACK] used if someone accesses Context.Settings before there is an context ...

			if (settings.LockOnStartup && !settings.IsReadOnlyMode) settings.IsReadOnlyMode = true;
			if (settings.ForceDebugMode) DebugMode = true;
			if (settings.DisableLogger) LoggerSingleton.Swap(new VoidLogger());

			ThemeManager.Inst.LoadWithErrorDialog(settings);

			if (settings.SingleInstanceMode)
			{
				var mtx = new Mutex(true, "AlephNoteApp_"+settings.ClientID);
				if (!mtx.WaitOne(TimeSpan.Zero, true))
				{
					
					NativeMethods.PostMessage(
						(IntPtr)NativeMethods.HWND_BROADCAST,
						NativeMethods.WM_SHOWME,
						IntPtr.Zero,
						IntPtr.Zero);

					return;
				}
			}
			
			var mw = new MainWindow(settings);
			
			if (!(settings.MinimizeToTray && settings.StartupState == ExtendedWindowState.Minimized)) mw.Show();
		}
		
		void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			ExceptionDialog.Show(null, "Internal Application Error", "An internal exception occured.\r\nThis should really not happen, the application will be terminated.\r\nPlease send a bug report to me.", e.Exception);

#if DEBUG
			// In debug mode do not custom-handle the exception, let Visual Studio handle it
			e.Handled = false;

			Console.Error.WriteLine(e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));

			Debugger.Break();
#else
			e.Handled = true;
			
			Current.Shutdown();
#endif
		}

		private static Version GetInformationalVersion()
		{
			try
			{
				var assembly = ResourceAssembly;

				var loc = assembly.Location;
				var vi = FileVersionInfo.GetVersionInfo(loc);
				return new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
			}
			catch (Exception)
			{
				return new Version(0, 0, 0, 0);
			}
		}
	}
}
