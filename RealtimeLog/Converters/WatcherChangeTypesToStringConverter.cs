using RealtimeLog.Utility;
using System;
using System.Globalization;
using System.Windows.Data;

namespace RealtimeLog.Converters;
internal class WatcherChangeTypesToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is FileChangeType w)
		{
			return w.ToString();
		}

		return string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is string s)
		{
			if (Enum.TryParse(s, true, out FileChangeType e))
			{
				return e;
			}
		}

		return FileChangeType.None;
	}
}
