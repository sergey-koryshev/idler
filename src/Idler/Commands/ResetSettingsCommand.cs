namespace Idler.Commands
{
    using Idler.Extensions;
    using Idler.Helpers.Notifications;
    using Idler.Properties;
    using Idler.ViewModels;

    public class ResetSettingsCommand : CommandBase
    {
        private readonly SettingsViewModel viewModel;
        private readonly NotificationsManager notificationsManager;

        public ResetSettingsCommand(SettingsViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.notificationsManager = NotificationsManager.GetInstance();
            this.viewModel.PropertyChanged += viewModelPropertyChanged;
        }

        public override void Execute(object parameter)
        {
            Settings.Default.Reload();
            this.viewModel.NoteCategories.RefreshAsync().SafeAsyncCall(() => this.viewModel.ResetFlags(), null, _ => this.notificationsManager.ShowError("Failed to reload categories.")); ;
        }

        public override bool CanExecute(object parameter)
        {
            return this.viewModel.AreAllSettingsUnsaved;
        }

        private void viewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
