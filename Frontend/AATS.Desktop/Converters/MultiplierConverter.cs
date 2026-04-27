using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AATS.Desktop.Converters;

public class MultiplierConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && double.TryParse(parameter?.ToString(), out double multiplier))
        {
            return intValue * multiplier;
        }
        if (value is double dblValue && double.TryParse(parameter?.ToString(), out double multiplier2))
        {
            return dblValue * multiplier2;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
