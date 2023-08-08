using Idler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Idler.Commands
{
    public class RefreshNotesCommand : CommandBase
    {
        private Shift shift;

        public RefreshNotesCommand(Shift shift) {
            this.shift = shift;
        }
        public override void Execute(object parameter)
        {
            bool canRefresh = true;

            if (this.shift.Changed)
            {
                canRefresh = MessageBox.Show(
                    "There are unsaved changes, are you sure you want to refresh without saving?",
                    "Warning",
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK;
            }

            if (canRefresh)
            {
                this.shift.RefreshAsync().SafeFireAndForget();
            }
        }
    }
}
