using System;
using System.Globalization;
using System.Windows.Data;

namespace ZzukBot.WPF.Converters
{
    internal class WidthAdjustConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myValue = (double) value;
            if (myValue - 5 < 0) return myValue;
            return myValue - 20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}