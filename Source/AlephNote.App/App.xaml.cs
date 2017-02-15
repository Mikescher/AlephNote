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

		public static readonly string APP_VERSION = GetInformationalVersion();

		public static string AppVersionProperty { get { return APP_VERSION; } }

		public App()
		{
			DispatcherUnhandledException += AppDispatcherUnhandledException;
		}
		
		void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			string errorMessage = string.Format("An application error occurred.\nThe application cannot continue and will shut down.\n\nError:\n{0}", e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));

#if DEBUG
			// In debug mode do not custom-handle the exception, let Visual Studio handle it
			e.Handled = false;

			Console.Error.WriteLine(errorMessage);

			Debugger.Break();
#else
			e.Handled = true;

			MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
			
			Application.Current.Shutdown();
#endif
		}

		private static string GetInformationalVersion()
		{
			try
			{
				var assembly = ResourceAssembly;

				var loc = assembly.Location;
				if (loc == null) return "???.???.???.???";
				return FileVersionInfo.GetVersionInfo(loc).ProductVersion;
			}
			catch (Exception)
			{
				return "???.???.???.???";
			}
		}
	}
}
