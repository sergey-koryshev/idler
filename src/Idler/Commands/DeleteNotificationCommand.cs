using Idler.Helpers.Notifications;
using Idler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Commands
{
    public class DeleteNotificationCommand : CommandBase
    {
        private NotificationViewModel notificationViewModel;

        public DeleteNotificationCommand(NotificationViewModel notificationViewModel)
        { 
            this.notificationViewModel = notificationViewModel;
        }

        public override void Execute(object _)
        {
            NotificationsManager.GetInstance().DeleteNotification(notificationViewModel);
        }
    }
}
