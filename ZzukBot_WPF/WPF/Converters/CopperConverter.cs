using System;
using System.Globalization;
using System.Windows.Data;

namespace ZzukBot.WPF.Converters
{
    internal class CopperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var copper = (int) value;

            var moneyString = "";
            if (copper < 0)
            {
                moneyString += "- ";
                copper = Math.Abs(copper);
            }
            if (copper >= 10000)
            {
                var gold = (int) (copper / (float) 10000);
                moneyString += gold + "G ";
                copper -= gold * 10000;
            }
            if (copper > 100)
            {
                var silver = (int) (copper / (float) 100);
                moneyString += silver + "S ";
                copper -= silver * 100;
            }
            moneyString += copper + "C";
            return moneyString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}