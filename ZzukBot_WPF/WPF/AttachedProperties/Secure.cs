using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ZzukBot.WPF.AttachedProperties
{
    internal class Secure
    {
        private static readonly DependencyProperty PasswordInitializedProperty =
            DependencyProperty.RegisterAttached("PasswordInitialized", typeof(bool), typeof(Secure),
                new PropertyMetadata(false));

        private static readonly DependencyProperty SettingPasswordProperty =
            DependencyProperty.RegisterAttached("SettingPassword", typeof(bool), typeof(Secure),
                new PropertyMetadata(false));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(Secure),
                new FrameworkPropertyMetadata("", HandleBoundPasswordChanged)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        internal static string GetPassword(DependencyObject obj)
        {
            return (string) obj.GetValue(PasswordProperty);
        }

        internal static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        private static void HandleBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = dp as PasswordBox;
            if (passwordBox == null)
                return;

            // If we're being called because we set the value of the property we're bound to (from inside 
            // HandlePasswordChanged, then do nothing - we already have the latest value).
            if ((bool) passwordBox.GetValue(SettingPasswordProperty))
                return;

            // If this is the initial set (see the comment on PasswordProperty), set ourselves up
            if (!(bool) passwordBox.GetValue(PasswordInitializedProperty))
            {
                passwordBox.SetValue(PasswordInitializedProperty, true);
                passwordBox.PasswordChanged += HandlePasswordChanged;
            }

            passwordBox.Password = e.NewValue as string;
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox) sender;
            passwordBox.SetValue(SettingPasswordProperty, true);
            SetPassword(passwordBox, passwordBox.Password);
            passwordBox.SetValue(SettingPasswordProperty, false);
        }
    }
}