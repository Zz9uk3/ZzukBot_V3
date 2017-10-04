using System;
using System.Globalization;
using System.Windows.Data;

#pragma warning disable 1591

namespace ZzukBot.WPF.Converters
{
    internal class ChannelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myString = (string) value;
            if (myString.Contains("Channel")) return "Channel";
            return myString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}