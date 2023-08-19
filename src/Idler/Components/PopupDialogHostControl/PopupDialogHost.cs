using Idler.Commands;
using Idler.Components.PopupDialogControl;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Idler.Components
{
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

        public void ForceClose()
        {
            this.Content = null;
        }
    }
}
