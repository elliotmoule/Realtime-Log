using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RealtimeLog.Converters;

public class FileWatchStatusToMediaColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is FileWatchStatus fws)
		{
			switch (fws)
			{
				case FileWatchStatus.Good:
					return ProjectConstants.GoodColor;
				case FileWatchStatus.Error:
					return ProjectConstants.ErrorColor;
				case FileWatchStatus.Warning:
					return ProjectConstants.WarningColor;
			}
		}

		return ProjectConstants.UnknownBrush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null
			&& value is Brush c)
		{
			if (c == ProjectConstants.GoodColor)
			{
				return FileWatchStatus.Good;
			}
			else if (c == ProjectConstants.ErrorColor)
			{
				return FileWatchStatus.Error;
			}
			else if (c == ProjectConstants.WarningColor)
			{
				return FileWatchStatus.Warning;
			}
		}

		return ProjectConstants.UnknownBrush;
	}
}
