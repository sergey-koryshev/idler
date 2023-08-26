using Idler.Contracts;
using Idler.Helpers.DB;
using Idler.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Commands
{
    public class ChangeSelectedDateCommand : CommandBase
    {
        private MainWindow mainWindow;

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

            SelectedDateType dateType;
            if (Enum.TryParse(parameter.ToString(), out dateType))
            {
                if (dateType == SelectedDateType.NextDate)
                {
                    this.mainWindow.SafeAsyncCall(DataBaseFunctions.GetNextDate(this.mainWindow.SelectedDate), (date) =>
                    {
                        this.mainWindow.Dispatcher.Invoke(() =>
                        {
                            this.mainWindow.SelectedDate = this.CalculateSelectedDate(date, dateType);
                        });
                    });
                }
                if (dateType == SelectedDateType.PreviousDate)
                {
                    this.mainWindow.SafeAsyncCall(DataBaseFunctions.GetPreviousDate(this.mainWindow.SelectedDate), (date) =>
                    {
                        this.mainWindow.Dispatcher.Invoke(() =>
                        {
                            this.mainWindow.SelectedDate = this.CalculateSelectedDate(date, dateType);
                        });
                    });
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
