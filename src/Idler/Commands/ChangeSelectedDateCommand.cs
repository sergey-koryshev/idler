namespace Idler.Commands
{
    using System;
    using Idler.Contracts;
    using Idler.Extensions;
    using Idler.Helpers.DB;
    using Idler.Helpers.Notifications;
    using Idler.Properties;

    public class ChangeSelectedDateCommand : CommandBase
    {
        private readonly MainWindow mainWindow;

        public ChangeSelectedDateCommand(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public override void Execute(object parameter)
        {
            if (this.mainWindow.CurrentShift?.Changed == true)
            {
                var result = this.mainWindow.DialogHost.ShowDialog(
                    "Warning",
                    "There are unsaved changes. Are you sure you want to change current date? All changes will be discharged.",
                    Components.PopupDialogControl.Buttons.OkCancel);

                if (result == Components.PopupDialogControl.Result.Cancel)
                {
                    return;
                }
            }

            if (Enum.TryParse(parameter.ToString(), out SelectedDateType dateType))
            {
                var changeSelectedDateAction = new Action<DateTime?>(date =>
                {
                    this.mainWindow.Dispatcher.Invoke(() =>
                    {
                        this.mainWindow.SelectedDate = this.CalculateSelectedDate(date, dateType);
                    });
                });

                var errorHandler = new Action<Exception, bool>((_, __) =>
                {
                    NotificationsManager.Instance.ShowError($"{(dateType == SelectedDateType.NextDate ? "Next" : "Previous")} date cannot be retrieved.");
                });

                if (dateType == SelectedDateType.NextDate)
                {
                    DataBaseFunctions.GetNextDate(this.mainWindow.SelectedDate).SafeAsyncCall(changeSelectedDateAction, null, errorHandler);
                }
                if (dateType == SelectedDateType.PreviousDate)
                {
                    DataBaseFunctions.GetPreviousDate(this.mainWindow.SelectedDate).SafeAsyncCall(changeSelectedDateAction, null, errorHandler);
                }
            }
        }

        private DateTime CalculateSelectedDate(DateTime? date, SelectedDateType type)
        {
            var result = this.mainWindow.SelectedDate;
            do
            {
                result = result.AddDays((int)type);
            } while ((result.DayOfWeek == DayOfWeek.Saturday
                || result.DayOfWeek == DayOfWeek.Sunday)
                && Settings.Default.SkipWeekends);

            if (date == null)
            {
                return result;
            }
            else
            {
                return (result > date && type == SelectedDateType.NextDate)
                    || (result < date && type == SelectedDateType.PreviousDate)
                    ? date.Value : result;
            }
        }

    }
}
