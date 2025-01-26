namespace Idler.Helpers
{
    using System.Windows;
    using System.Windows.Controls;

    public static class ListViewScrollHelper
    {
        public static readonly DependencyProperty ScrollToProperty = DependencyProperty.RegisterAttached(
        "ScrollTo", typeof(object), typeof(ListViewScrollHelper), new FrameworkPropertyMetadata(null, OnScrollToPropertyChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static void SetScrollTo(DependencyObject element, object value)
        {
            element.SetValue(ScrollToProperty, value);
        }

        public static object GetScrollTo(DependencyObject element)
        {
            return element.GetValue(ScrollToProperty);
        }

        private static void OnScrollToPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is ListView listView) || e.NewValue == null)
            {
                return;
            }

            listView.ScrollIntoView(e.NewValue);
        }
    }
}
