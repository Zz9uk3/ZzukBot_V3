using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace ZzukBot.WPF.AttachedProperties
{
    internal class Result
    {
        internal static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached("DialogResult", typeof(DialogResult?), typeof(Result),
                new FrameworkPropertyMetadata(null, HandleDialogResultChanged)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        internal static DialogResult? GetDialogResult(DependencyObject obj)
        {
            return (DialogResult?) obj.GetValue(DialogResultProperty);
        }

        internal static void SetDialogResult(DependencyObject obj, DialogResult? value)
        {
            obj.SetValue(DialogResultProperty, value);
        }

        private static void HandleDialogResultChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var window = dp as Window;
            if (window == null) return;
            var result = e.NewValue as DialogResult?;
            if (result == null) return;
            if (result.Value == DialogResult.Retry) window.DialogResult = null;
            window.DialogResult = result.Value == DialogResult.OK || result.Value == DialogResult.Yes;
            window.Close();
        }
    }
}