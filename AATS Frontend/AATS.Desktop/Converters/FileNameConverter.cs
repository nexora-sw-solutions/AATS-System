using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AATS.Desktop.Converters
{
    public class FileNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string pathOrUrl && !string.IsNullOrEmpty(pathOrUrl))
            {
                try
                {
                    if (pathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        return System.IO.Path.GetFileName(new Uri(pathOrUrl).LocalPath);
                    }
                    return System.IO.Path.GetFileName(pathOrUrl);
                }
                catch
                {
                    return pathOrUrl;
                }
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
