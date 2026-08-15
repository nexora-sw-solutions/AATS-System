using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AATS.Desktop.Converters;

public class StringEqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Only update the source property when the RadioButton is CHECKED (value == true).
        // If value is false (unchecking), do nothing.
        if (value is true)
        {
             return parameter?.ToString();
        }
        
        return Avalonia.AvaloniaProperty.UnsetValue;
    }
}
