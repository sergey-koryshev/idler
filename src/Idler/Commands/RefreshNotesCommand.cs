﻿namespace Idler.Commands
{
    using Idler.Components;
    using Idler.Components.PopupDialogControl;
    using Idler.Extensions;
    using Idler.Helpers.Notifications;

    public class RefreshNotesCommand : CommandBase
    {
        private readonly Shift shift;
        private readonly PopupDialogHost dialogHost;

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
                this.shift.RefreshAsync().SafeAsyncCall(null, null, (_, __) => NotificationsManager.Instance.ShowError("Failed to refresh notes."));
            }
        }
    }
}
