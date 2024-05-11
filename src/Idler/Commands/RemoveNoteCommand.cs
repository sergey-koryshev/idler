namespace Idler.Commands
{
    using System.Collections.ObjectModel;

    public class RemoveNoteCommand : CommandBase
    {
        private readonly ObservableCollection<ShiftNote> notes;
        private readonly ShiftNote targetNote;

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
