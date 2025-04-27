using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SET09102.Converters;

public class StatusToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "operational" => Colors.White,
                "maintenance" => Colors.Black,
                "offline" => Colors.White,
                _ => Colors.Black
            };
        }
        return Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}