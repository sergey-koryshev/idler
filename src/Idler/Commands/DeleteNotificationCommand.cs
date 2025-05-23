namespace Idler.Commands
{
    using Idler.Helpers.Notifications;
    using Idler.ViewModels;

    public class DeleteNotificationCommand : CommandBase
    {
        private readonly NotificationViewModel notificationViewModel;

        public DeleteNotificationCommand(NotificationViewModel notificationViewModel)
        { 
            this.notificationViewModel = notificationViewModel;
        }

        public override void Execute(object _)
        {
            NotificationsManager.Instance.DeleteNotification(notificationViewModel);
        }
    }
}
