namespace Idler.Commands
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Idler.Helpers.BackgroundManager;
    using Idler.Helpers.Notifications;
    using Idler.Managers;
    using Idler.Models;
    using Idler.Properties;

    public class RetrainAutoCategorizationModelCommand : CommandBase
    {
        public RetrainAutoCategorizationModelCommand()
        {
            NlpModelManager.Instance.ModelStatusChanged += (s, status) => this.OnCanExecutedChanged();
            Settings.Default.SettingsSaving += (s, e) => this.OnCanExecutedChanged();
        }

        public override void Execute(object parameter)
        {
            NlpModelManager.Instance.ModelStatusChanged += this.OnAutoCategorizationModelStatusChanged;
            BackgroundTasksManager.Instance.AddBackgroundTask(
                Task.Run(async () => await NlpModelManager.Instance.RetrainModelAsync()),
                Strings.AutoCategorizationRetrainingBackgroundTaskTitle,
                errorCallback: err =>
                {
                    Trace.TraceError("Error has occured while retraining auto-categorization model: {0}", err);
                });
        }

        private void OnAutoCategorizationModelStatusChanged(object sender, NlpModelStatus e)
        {
            if (e == NlpModelStatus.Trained)
            {
                NotificationsManager.Instance.ShowSuccess(Strings.AutoCategorizationRetrainingSuccessMessage);
            }

            if (e == NlpModelStatus.Failed)
            {
                NotificationsManager.Instance.ShowError(Strings.AutoCategorizationRetrainingFailureMessage);
            }

            if (e == NlpModelStatus.Training)
            {
                NotificationsManager.Instance.ShowInfo(Strings.AutoCategorizationModelBeingTrainedMessage);
            }
        }

        public override bool CanExecute(object parameter)
        {
            return Settings.Default.IsAutoCategorizationEnabled &&
                NlpModelStatus.Completed.HasFlag(NlpModelManager.Instance.NlpModelStatus);
        }
    }
}
