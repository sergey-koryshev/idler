using Idler.Commands;

namespace Idler.Components.PopupDialogControl
{
    public class OkCommand : CommandBase
    {
        private PopupDialog popupDialog;
        public OkCommand(PopupDialog popupDialog) { 
            this.popupDialog = popupDialog;
        }

        public override void Execute(object parameter)
        {
            this.popupDialog.Result = Result.OK;
        }
    }

    public class CancelCommand : CommandBase
    {
        private PopupDialog popupDialog;
        public CancelCommand(PopupDialog popupDialog)
        {
            this.popupDialog = popupDialog;
        }

        public override void Execute(object parameter)
        {
            this.popupDialog.Result = Result.Cancel;
        }
    }
}
