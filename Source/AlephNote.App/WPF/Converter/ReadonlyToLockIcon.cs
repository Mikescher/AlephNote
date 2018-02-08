using AlephNote.WPF.MVVM;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AlephNote.WPF.Converter
{
	class ReadonlyToLockIcon : OneWayConverter<bool, ImageSource>
	{
		protected override ImageSource Convert(bool value, object parameter)
		{
			if (value)
				return new BitmapImage(new Uri("pack://application:,,,/AlephNote;component/Resources/lock.png"));
			else
				return new BitmapImage(new Uri("pack://application:,,,/AlephNote;component/Resources/lock_open.png"));
		}
	}
}
