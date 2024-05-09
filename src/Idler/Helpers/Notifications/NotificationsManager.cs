namespace Idler.Helpers.Notifications
{
    using System.Windows;
    using Idler.Models;
    using Idler.ViewModels;

    public class NotificationsManager
    {
        private static NotificationsManager instance;
        private NotificationsHostAdorner adorner;

        public static NotificationsManager GetInstance()
        {
            if (instance == null)
            {
                instance = new NotificationsManager();
            }

            return instance;
        }

        public NotificationsHostAdorner GetAdorner()
        {
           return instance?.adorner;
        }

        public void SetAdorner(NotificationsHostAdorner adorner)
        {
            if (instance == null)
            {
                return;
            }

            instance.adorner = adorner;
        }

        public void RemoveAdorner()
        {
            if (instance == null)
            {
                return;
            }

            instance.adorner = null;
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
