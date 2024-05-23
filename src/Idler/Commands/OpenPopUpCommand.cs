namespace Idler.Commands
{
    using System;
    using System.Windows.Controls;
    using Idler.Components;
    using Idler.ViewModels;

    public class OpenPopUpCommand<T> : CommandBase where T : UserControl
    {
        private readonly PopupDialogHost dialogHost;
        private readonly Func<BaseViewModel> viewModelFunc;
        private readonly string title;

        public OpenPopUpCommand(PopupDialogHost dialogHost, string title, Func<BaseViewModel> viewModelFunc)
        {
            this.dialogHost = dialogHost;
            this.title = title;
            this.viewModelFunc = viewModelFunc;
        }

        public override void Execute(object parameter)
        {
            T dialogContent = (T)Activator.CreateInstance(typeof(T));
            dialogContent.DataContext = this.viewModelFunc();
            this.dialogHost.ShowPopUp(this.title, dialogContent);
        }
    }
}
