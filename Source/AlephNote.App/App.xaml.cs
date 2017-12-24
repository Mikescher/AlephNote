using AlephNote.Log;
using AlephNote.WPF.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using AlephNote.Commandline;
using AlephNote.Common.Plugins;
using AlephNote.Impl;

namespace AlephNote
{
	public partial class App
	{
		public static readonly string PATH_SETTINGS    = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.config");
		public static readonly string PATH_SCROLLCACHE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.scrollcache.config");
		public static readonly string PATH_LOCALDB     = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".notes");
		public static readonly string APPNAME_REG      = "AlephNoteApp_{0:N}";
		public static readonly string PATH_EXECUTABLE  = System.Reflection.Assembly.GetExecutingAssembly().Location;

		public static readonly Version APP_VERSION = GetInformationalVersion();

		public static string AppVersionProperty { get { return APP_VERSION.Revision == 0 ? APP_VERSION.ToString(3) : (APP_VERSION.ToString(4) + " BETA"); } }

		public static readonly Random GlobalRandom = new Random();

		public static CommandLineArguments Args;

		public static readonly EventLogger Logger = new EventLogger();
		public static bool DebugMode = false;

		public static bool IsUpdateMigration = false;
		public static Version UpdateMigrationFrom;
		public static Version UpdateMigrationTo;

		public App()
		{
			DispatcherUnhandledException += AppDispatcherUnhandledException;

			PluginManagerSingleton.Register(new PluginManager());

			Args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

			if (Args.Contains("debug")) DebugMode = true;

			UpdateMigrationFrom = Args.GetVersionDefault("migration_from", default(Version));
			UpdateMigrationTo   = Args.GetVersionDefault("migration_to", default(Version));

			IsUpdateMigration = (UpdateMigrationFrom != default(Version)) && (UpdateMigrationTo != default(Version)) && Args.Contains("updated");

#if DEBUG
			DebugMode = true;
#endif
			Logger.DebugEnabled = DebugMode;

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
