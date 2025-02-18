﻿namespace Idler.Commands
{
    using System.Threading.Tasks;
    using Idler.Extensions;
    using Idler.Helpers.Notifications;
    using Idler.Properties;
    using Idler.ViewModels;

    internal class SaveSettingsCommand : CommandBase
    {
        private readonly SettingsViewModel viewModel;
        private readonly NotificationsManager notificationsManager;

        public SaveSettingsCommand(SettingsViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.notificationsManager = NotificationsManager.GetInstance();
            this.viewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        public override void Execute(object parameter)
        {
            Settings.Default.Save();
            (this.viewModel.IsDataSourceChanged == false
                ? this.viewModel.NoteCategories.UpdateAsync()
                : Task.CompletedTask).SafeAsyncCall(() => this.viewModel.ResetFlags(), null, _ => this.notificationsManager.ShowError("Failed to save categories."));
        }

        public override bool CanExecute(object parameter)
        {
            return this.viewModel.AreAllSettingsUnsaved;
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.viewModel.AreAllSettingsUnsaved):
                    this.OnCanExecutedChanged();
                    break;
            }
        }
    }
}
