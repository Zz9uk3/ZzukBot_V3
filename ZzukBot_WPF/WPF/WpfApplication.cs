using System.Windows;
using System.Windows.Controls;
using ZzukBot.WPF.Commands;

namespace ZzukBot.WPF
{
    internal class WpfApplication : Application
    {
        static WpfApplication()
        {
            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(UserControl),
                new FrameworkPropertyMetadata(OnElementDataContextChanged));

            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(TreeViewItem),
                new FrameworkPropertyMetadata(OnElementDataContextChanged));

            //FrameworkElement.DataContextProperty.OverrideMetadata(typeof(TabItem), 
            //    new FrameworkPropertyMetadata(OnElementDataContextChanged));

            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata(OnElementDataContextChanged));

            //Context = new ServiceContext(null);
        }

        private static void OnElementDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            if (e.OldValue != null)
                CommandHandler.UnregisterUieLement(element, e.OldValue);
            if (e.NewValue != null)
                CommandHandler.RegisterUiElement(element, e.NewValue);
        }
    }
}