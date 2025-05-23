namespace Idler.Helpers.Notifications
{
    using System;
    using System.Windows;
    using Idler.Models;
    using Idler.ViewModels;

    public class NotificationsManager
    {
        private static readonly Lazy<NotificationsManager> instance = new Lazy<NotificationsManager>(() => new NotificationsManager());
        private NotificationsHostAdorner adorner;

        public static NotificationsManager Instance => instance.Value;

        public NotificationsHostAdorner GetAdorner()
        {
           return Instance?.adorner;
        }

        public void SetAdorner(NotificationsHostAdorner adorner)
        {
            if (instance == null)
            {
                return;
            }

            Instance.adorner = adorner;
        }

        public void RemoveAdorner()
        {
            if (instance == null)
            {
                return;
            }

            Instance.adorner = null;
        }

        public void ShowSuccess(string text, bool autoClosing = true)
        {
            this.AddNotification(NotificationType.Success, text, autoClosing);
        }

        public void ShowInfo(string text, bool autoClosing = true)
        {
            this.AddNotification(NotificationType.Information, text, autoClosing);
        }

        public void ShowError(string text)
        {
            this.AddNotification(NotificationType.Error, text);
        }

        public void ShowWarning(string text)
        {
            this.AddNotification(NotificationType.Warning, text);
        }

        public void DeleteNotification(NotificationViewModel notificationViewModel)
        {
            this.adorner.RemoveNotificationVisual(notificationViewModel);
        }

        private void AddNotification(NotificationType type, string text, bool autoClosing = false)
        {
            if (this.adorner == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => this.adorner.AddNotificationVisual(new NotificationViewModel(type, text, autoClosing)));
        }
    }
}
