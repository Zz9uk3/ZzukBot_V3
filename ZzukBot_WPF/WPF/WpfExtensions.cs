using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ZzukBot.WPF
{
    internal static class WpfExtensions
    {
        internal static RoutedCommand GetCommand(this string resourceKey)
        {
            var ret = (RoutedCommand) Application.Current.FindResource(resourceKey);
            return ret;
        }

        internal static void Dispatch(this Action value)
        {
            Application.Current.Dispatcher.Invoke(value);
        }

        internal static void BeginDispatch(this Action value, DispatcherPriority priority)
        {
            Application.Current.Dispatcher.BeginInvoke(value, priority);
        }
    }
}