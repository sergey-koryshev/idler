namespace Idler.ViewModels
{
    using System;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Idler.Commands;
    using Idler.Models;

    public class NotificationViewModel : BaseViewModel
    {
        private readonly TimeSpan autoClosingInterval = TimeSpan.FromSeconds(5);

        private NotificationType type;
        private string text;
        private ICommand deleteNotificationCommand;
        private bool isAutoClosing;

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

        public bool IsAutoClosing
        {
            get => isAutoClosing;
            set
            {
                isAutoClosing = value;
                this.OnPropertyChanged();
            }
        }

        public Visual VisualReference { get; set; }

        public NotificationViewModel(NotificationType type, string text, bool autoClosing)
        {
            this.Type = type;
            this.Text = text;
            this.DeleteNotificationCommand = new DeleteNotificationCommand(this);

            if (autoClosing)
            {
                this.IsAutoClosing = true;
                new DispatcherTimer(autoClosingInterval, DispatcherPriority.Normal, (sender, args) =>
                    {
                        this.DeleteNotificationCommand.Execute(null);
                }, Dispatcher.CurrentDispatcher);
            }
        }
    }
}
