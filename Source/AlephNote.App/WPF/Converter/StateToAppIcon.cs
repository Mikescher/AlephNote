using AlephNote.Common.Settings.Types;
using MSHC.WPF.MVVM;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AlephNote.WPF.Converter
{
	class StateToAppIcon : OneWayConverter<SynchronizationState, ImageSource>
	{
		protected override ImageSource Convert(SynchronizationState value, object parameter)
		{
			switch (value)
			{
				case SynchronizationState.NotSynced:
					return new BitmapImage(new Uri("pack://application:,,,/AlephNote;component/Resources/IconYellow.ico"));
				case SynchronizationState.Syncing:
					return new BitmapImage(new Uri("pack://application:,,,/AlephNote;component/Resources/IconSync.ico"));
				case SynchronizationState.UpToDate:
					return new BitmapImage(new Uri("pack://application:,,,/AlephNote;component/Resources/IconGreen.ico"));
				case SynchronizationState.Error:
					return new BitmapImage(new Uri("pack://application:,,,/AlephNote;component/Resources/IconRed.ico"));
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}
		}
	}
}
