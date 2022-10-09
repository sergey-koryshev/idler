using Idler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Commands
{
    internal class AddNoteCommand : CommandBase
    {
        private readonly Shift shift;
        private AddNoteViewModel addNoteViewModel;

        public AddNoteCommand(Shift shift)
        {
            this.shift = shift;
        }

        public AddNoteCommand(AddNoteViewModel addNoteViewModel, Shift shift)
        {
            this.addNoteViewModel = addNoteViewModel;
            this.shift = shift;

            this.addNoteViewModel.PropertyChanged += AddNoteViewModelPropertyChanged;
        }

        private void AddNoteViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.addNoteViewModel.CategoryId):
                case nameof(this.addNoteViewModel.Effort):
                case nameof(this.addNoteViewModel.Description):
                    this.OnCanExecutedChanged();
                    break;
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.addNoteViewModel.CategoryId > 0 && this.addNoteViewModel.Effort > 0 && !String.IsNullOrWhiteSpace(this.addNoteViewModel.Description);
        }

        public override void Execute(object parameter)
        {
            ShiftNote note = new ShiftNote()
            {
                CategoryId = addNoteViewModel.CategoryId,
                Effort = addNoteViewModel.Effort,
                Description = addNoteViewModel.Description,
                StartTime = addNoteViewModel.StartTime
            };
            this.shift.AddNewShiftNote(note);
            this.addNoteViewModel.ResetFields();
        }
    }
}
