using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Commands
{
    public class RemoveNoteCommand : CommandBase
    {
        private ObservableCollection<ShiftNote> notes;
        private ShiftNote targetNote;

        public RemoveNoteCommand(ObservableCollection<ShiftNote> notes, ShiftNote targetNote) {
            this.notes = notes;
            this.targetNote = targetNote;
        }

        public override void Execute(object parameter)
        {
            notes?.Remove(this.targetNote);
        }
    }
}
