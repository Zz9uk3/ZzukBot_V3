using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

#pragma warning disable 1591

namespace ZzukBot.WPF.Converters
{
    internal class TabSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return null;
            var tabControl = values[0] as TabControl;
            var width = (double) values[1];
            if (tabControl == null) return null;
            var itemsCount = tabControl.Items.Count;
            if (itemsCount == 0) itemsCount = 1;
            return width / itemsCount - 1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}