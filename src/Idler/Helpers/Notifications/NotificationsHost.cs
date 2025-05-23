namespace Idler.Helpers.Notifications
{
    using System.Windows;
    using System.Windows.Documents;

    public static class NotificationsHost
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(NotificationsHost), new FrameworkPropertyMetadata(default(bool), OnIsEnabledPropertyChanged)
        {
            BindsTwoWayByDefault = false,
        });

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        private static void OnIsEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                if (e.NewValue != e.OldValue)
                {
                    if ((bool)e.NewValue)
                    {
                        if (element.IsLoaded)
                        {
                            AddAdorner(element);
                        }
                        else
                        {
                            element.Loaded += (s, a) => AddAdorner(element);
                        }
                    }
                    else
                    {
                        if (element.IsLoaded)
                        {
                            RemoveAdorner(element);
                        }
                        else
                        {
                            element.Loaded += (s, a) => RemoveAdorner(element);
                        }
                    }
                }
            }
        }

        private static void AddAdorner(FrameworkElement element)
        {
            var adorner = new NotificationsHostAdorner(element);
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer != null)
            {
                adornerLayer.Add(adorner);
                NotificationsManager.Instance.SetAdorner(adorner);
            }
        }

        private static void RemoveAdorner(FrameworkElement element)
        {
            var adorner = NotificationsManager.Instance.GetAdorner();

            if (adorner != null)
            {
                AdornerLayer.GetAdornerLayer(element)?.Remove(adorner);
                NotificationsManager.Instance.RemoveAdorner();
            }
        }
    }
}
