using AlephNote.Log;
using AlephNote.WPF.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace AlephNote
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static readonly string PATH_SETTINGS = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"noteapp.config");
		public static readonly string PATH_LOCALDB  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".notes");
		public static readonly string APPNAME_REG = "AlephNoteApp";
		public static readonly string PATH_EXECUTABLE = System.Reflection.Assembly.GetExecutingAssembly().Location;

		public static readonly Version APP_VERSION = GetInformationalVersion();

		public static string AppVersionProperty { get { return APP_VERSION.ToString(); } }

		public static EventLogger Logger = new EventLogger();

		public App()
		{
			DispatcherUnhandledException += AppDispatcherUnhandledException;
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
			
			Application.Current.Shutdown();
#endif
		}

		private static Version GetInformationalVersion()
		{
			try
			{
				var assembly = ResourceAssembly;

				var loc = assembly.Location;
				if (loc == null) return new Version(0, 0, 0, 0);
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
