using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

#pragma warning disable 1591

namespace ZzukBot.WPF.AttachedProperties
{
    internal class ListBoxBehavior
    {
        private static readonly Dictionary<ListBox, Capture> Associations =
            new Dictionary<ListBox, Capture>();

        internal static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof(bool),
                typeof(ListBoxBehavior),
                new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

        internal static bool GetScrollOnNewItem(DependencyObject obj)
        {
            return (bool) obj.GetValue(ScrollOnNewItemProperty);
        }

        internal static void SetScrollOnNewItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollOnNewItemProperty, value);
        }

        internal static void OnScrollOnNewItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            if (listBox == null) return;
            bool oldValue = (bool) e.OldValue, newValue = (bool) e.NewValue;
            if (newValue == oldValue) return;
            if (newValue)
            {
                listBox.Loaded += ListBox_Loaded;
                listBox.Unloaded += ListBox_Unloaded;
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.AddValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;
                if (Associations.ContainsKey(listBox))
                    Associations[listBox].Dispose();
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
        }

        private static void ListBox_ItemsSourceChanged(object sender, EventArgs e)
        {
            var listBox = (ListBox) sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            Associations[listBox] = new Capture(listBox);
        }

        private static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            listBox.Unloaded -= ListBox_Unloaded;
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            var incc = listBox.Items as INotifyCollectionChanged;
            if (incc == null) return;
            listBox.Loaded -= ListBox_Loaded;
            Associations[listBox] = new Capture(listBox);
        }

        private class Capture : IDisposable
        {
            private readonly INotifyCollectionChanged _incc;
            private readonly ListBox _listBox;

            internal Capture(ListBox listBox)
            {
                _listBox = listBox;
                _incc = listBox.ItemsSource as INotifyCollectionChanged;
                if (_incc == null) return;
                _incc.CollectionChanged += incc_CollectionChanged;
                _listBox.IsVisibleChanged += ListBoxOnIsVisibleChanged;
            }

            private void ListBoxOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
            {
                var count = _listBox.Items.Count;
                if (!(bool)dependencyPropertyChangedEventArgs.NewValue || count == 0) return;
                _listBox.UpdateLayout();
                var index = count - 1;
                _listBox.ScrollIntoView(_listBox.Items[index]);
                _listBox.SelectedItem = _listBox.Items[index];
            }

            public void Dispose()
            {
                if (_incc != null)
                    _incc.CollectionChanged -= incc_CollectionChanged;
                if (_listBox != null)
                    _listBox.IsVisibleChanged -= ListBoxOnIsVisibleChanged;
            }

            private void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action != NotifyCollectionChangedAction.Add) return;
                _listBox.ScrollIntoView(e.NewItems[0]);
                _listBox.SelectedItem = e.NewItems[0];
            }
        }
    }
}