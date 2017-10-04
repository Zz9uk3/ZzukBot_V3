using System;
using System.Globalization;
using System.Windows.Data;

#pragma warning disable 1591

namespace ZzukBot.WPF.Converters
{
    internal class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myBool = (bool) value;
            return !myBool;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}