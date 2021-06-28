using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace nac.Forms.converters
{
    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return -((double)value + (parameter as double? ?? 0.0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}