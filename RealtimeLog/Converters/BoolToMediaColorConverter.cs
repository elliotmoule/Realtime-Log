using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RealtimeLog.Converters;

public class BoolToMediaColorConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is bool b && b ? ProjectConstants.GoodColor : ProjectConstants.ErrorColor;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is Brush c && c == ProjectConstants.GoodColor;
	}
}
