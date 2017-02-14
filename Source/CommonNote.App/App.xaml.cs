using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CommonNote
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static readonly string PATH_SETTINGS = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"common_note.config");
		public static readonly string PATH_LOCALDB  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".notes");
		public static readonly string APPNAME_REG = "CommonNoteApp";
		public static readonly string PATH_EXECUTABLE = System.Reflection.Assembly.GetExecutingAssembly().Location;

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
#else
			e.Handled = true;

			MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
			
			Application.Current.Shutdown();
#endif
		}
	}
}
