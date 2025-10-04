namespace Idler.Helpers
{
    using System.Windows;
    using System.Windows.Controls;
    using Idler.Extensions;

    public static class SelectListViewItemHelper
    {
        public static readonly DependencyProperty SelectOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectOnFocus",
                typeof(bool),
                typeof(SelectListViewItemHelper),
                new PropertyMetadata(false, OnSelectOnFocusChanged));

        public static bool GetSelectOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectOnFocusProperty);
        }

        public static void SetSelectOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectOnFocusProperty, value);
        }

        private static void OnSelectOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.GotFocus += TextBox_GotFocus;
                }
                else
                {
                    textBox.GotFocus -= TextBox_GotFocus;
                }
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            ListViewItem listViewItem = textBox.FindAncestor<ListViewItem>();
            if (listViewItem != null)
            {
                listViewItem.IsSelected = true;
            }
        }
    }
}
