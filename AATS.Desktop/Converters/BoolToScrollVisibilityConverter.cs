using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace AATS.Desktop.Converters
{
    public class BoolToScrollVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isAnyDropdownOpen)
            {
                // If any dropdown is open, disable scrolling. Otherwise, set to Auto.
                return isAnyDropdownOpen ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            }
            return ScrollBarVisibility.Auto;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
