﻿namespace Idler.Commands
{
    using System.Threading.Tasks;
    using Idler.Extensions;
    using Idler.Helpers.Notifications;

    public class SaveShiftCommand : CommandBase
    {
        private readonly Shift shift;
        private readonly MainWindow mainWindowView;

        public SaveShiftCommand(MainWindow mainWindowView, Shift shift)
        {
            this.shift = shift;
            this.mainWindowView = mainWindowView;
            this.shift.PropertyChanged += ShiftPropertyChanged;
        }

        private void ShiftPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(this.mainWindowView.CurrentShift.Changed):
                    this.OnCanExecutedChanged();
                    break;
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.shift.Changed;
        }

        public override void Execute(object parameter)
        {
            this.ExecuteAsync().SafeAsyncCall(null, null, _ => NotificationsManager.Instance.ShowError("Failed to save notes."));
        }

        public async Task ExecuteAsync()
        {
            await this.shift.UpdateAsync();
            this.mainWindowView.OnPropertyChanged(nameof(this.mainWindowView.CurrentShift));
            NotificationsManager.Instance.ShowSuccess("Notes have been successfully saved.");
        }
    }
}
