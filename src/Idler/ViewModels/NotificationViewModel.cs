namespace Idler.ViewModels
{
    using System.Windows.Input;
    using System.Windows.Media;
    using Idler.Commands;
    using Idler.Models;

    public class NotificationViewModel : BaseViewModel
    {
        private NotificationType type;
        private string text;
        private ICommand deleteNotificationCommand;

        public NotificationType Type
        { 
            get => type;
            set
            {
                type = value;
                this.OnPropertyChanged();
            }
        }

        public string Text
        { 
            get => text;
            set
            {
                text = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand DeleteNotificationCommand
        {
            get => deleteNotificationCommand;
            set
            {
                deleteNotificationCommand = value;
                this.OnPropertyChanged();
            }
        }

        public Visual VisualReference { get; set; }

        public NotificationViewModel(NotificationType type, string text)
        {
            this.Type = type;
            this.Text = text;
            this.DeleteNotificationCommand = new DeleteNotificationCommand(this);
        }
    }
}
