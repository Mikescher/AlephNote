using CommonNote.Settings;
using MSHC.WPF.MVVM;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonNote.WPF.Converter
{
	class StateToAppIcon : OneWayConverter<SynchronizationState, ImageSource>
	{
		public StateToAppIcon() { }

		protected override ImageSource Convert(SynchronizationState value, object parameter)
		{
			switch (value)
			{
				case SynchronizationState.NotSynced:
					return new BitmapImage(new Uri("pack://application:,,,/CommonNote;component/Resources/IconYellow.ico"));
				case SynchronizationState.Syncing:
					return new BitmapImage(new Uri("pack://application:,,,/CommonNote;component/Resources/IconSync.ico"));
				case SynchronizationState.UpToDate:
					return new BitmapImage(new Uri("pack://application:,,,/CommonNote;component/Resources/IconGreen.ico"));
				case SynchronizationState.Error:
					return new BitmapImage(new Uri("pack://application:,,,/CommonNote;component/Resources/IconRed.ico"));
				default:
					throw new ArgumentOutOfRangeException("value", value, null);
			}
		}
	}
}
