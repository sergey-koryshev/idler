using Idler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Commands
{
    public class SaveShiftCommand : CommandBase
    {
        private Shift shift;
        private MainWindow mainWindowView;

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
            this.ExecuteAsync().SafeFireAndForget();
        }

        public async Task ExecuteAsync()
        {
            await this.shift.UpdateAsync();
            this.mainWindowView.OnPropertyChanged(nameof(this.mainWindowView.CurrentShift));
        }
    }
}
