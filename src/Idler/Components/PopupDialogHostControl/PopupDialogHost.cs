namespace Idler.Components
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Idler.Commands;
    using Idler.Components.PopupDialogControl;
    using Idler.Components.PopupDialogHostControl;
    using Idler.Helpers;

    public class PopupDialogHost : ContentControl
    {
        static PopupDialogHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupDialogHost), new FrameworkPropertyMetadata(typeof(PopupDialogHost)));
        }

        public Result ShowDialog(string title, string message, Buttons buttons)
        {
            PopupDialog popupDialog = new PopupDialog()
            {
                Title = title,
                Message = message,
                Buttons = buttons
            };

            var oldContent = this.Content;
            this.Content = popupDialog;

            while (popupDialog.Result == Result.None)
            {
                if (Dispatcher.HasShutdownStarted ||
                Dispatcher.HasShutdownFinished)
                {
                    break;
                }

                Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
                Thread.Sleep(20);
            }

            this.Content = oldContent;
            return popupDialog.Result;
        }

        public void ShowPopUp(string title, Control popUpContent) 
        { 
            PopUpWrapper popUp = new PopUpWrapper
            {
                Title= title,
                Command = new RelayCommand(this.ForceClose),
                Content = popUpContent
            };

            this.Content = popUp;
        }

        private void ForceClose()
        {
            Task onCloseTask = Task.CompletedTask;

            if (this.Content is PopUpWrapper popup &&
                popup.Content is Control control &&
                control.DataContext is IClosableDialog closableDialog)
            {
                onCloseTask = closableDialog.OnDialogClosing();
            }

            onCloseTask.ContinueWith(task =>
            {
                if (!task.IsFaulted)
                {
                    DispatcherHelper.CurrentDispatcher.Invoke(new Action(() => this.ClearContent()));
                }
            });
        }

        private void ClearContent()
        {
            this.Content = null;
        }
    }
}
