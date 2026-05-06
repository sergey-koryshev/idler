namespace Idler.Commands
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Idler.Extensions;
    using Idler.Helpers.BackgroundManager;
    using Idler.Helpers.Notifications;
    using Idler.Localization;
    using Idler.Managers;
    using Idler.Models;
    using Idler.Properties;
    using Idler.ViewModels;

    internal class SaveSettingsCommand : CommandBase
    {
        private readonly SettingsViewModel viewModel;

        public SaveSettingsCommand(SettingsViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        public override void Execute(object parameter)
        {
            Settings.Default.Save();

            if (this.viewModel.IsAutoCategorizationChanged && Settings.Default.IsAutoCategorizationEnabled)
            {
                // Reinitialize NlpModelManager if auto-categorizatio setting was changed
                NlpModelManager.Instance.ModelStatusChanged += OnAutoCategorizationModelStatusChanged;

                BackgroundTasksManager.Instance
                    .AddBackgroundTask(
                        Task.Run(async () => await NlpModelManager.Instance.InitializeAsync()),
                        Strings.AutoCategorizationInitializationBackgroundTaskTitle,
                        errorCallback: err =>
                        {
                            Trace.TraceError("Error has occured while initializing auto-categorization feature: {0}", err);
                        })
                    .ContinueWith((_) => NlpModelManager.Instance.ModelStatusChanged -= OnAutoCategorizationModelStatusChanged);
            }

            (this.viewModel.IsDataSourceChanged == false
                ? this.viewModel.NoteCategories.UpdateAsync()
                : Task.CompletedTask).SafeAsyncCall((_) => this.viewModel.ResetFlags(), null, _ => NotificationsManager.Instance.ShowError("Failed to save categories."));
        }

        private void OnAutoCategorizationModelStatusChanged(object sender, NlpModelStatus e)
        {
            if (e == NlpModelStatus.Trained)
            {
                NotificationsManager.Instance.ShowSuccess(Strings.AutoCategorizationInitializationSuccessMessage);
            }

            if (e == NlpModelStatus.Failed)
            {
                NotificationsManager.Instance.ShowError(Strings.AutoCategorizationInitializationFailureMessage);
            }

            if (e == NlpModelStatus.Training)
            {
                NotificationsManager.Instance.ShowInfo(Strings.AutoCategorizationModelBeingTrainedMessage);
            }
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
