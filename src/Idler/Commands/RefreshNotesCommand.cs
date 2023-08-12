using Idler.Components;
using Idler.Components.PopupDialogControl;
using Idler.Extensions;

namespace Idler.Commands
{
    public class RefreshNotesCommand : CommandBase
    {
        private Shift shift;
        private PopupDialogHost dialogHost;

        public RefreshNotesCommand(Shift shift, PopupDialogHost dialogHost) {
            this.shift = shift;
            this.dialogHost = dialogHost;
        }
        public override void Execute(object parameter)
        {
            bool canRefresh = true;

            if (this.shift.Changed)
            {
                canRefresh = this.dialogHost.ShowDialog(
                    "Warning",
                    "There are unsaved changes, are you sure you want to refresh without saving?",
                    Buttons.OkCancel) == Result.OK;
            }

            if (canRefresh)
            {
                this.shift.RefreshAsync().SafeFireAndForget();
            }
        }
    }
}
