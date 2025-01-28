namespace Idler.Commands
{
    using System.Collections.ObjectModel;

    internal class RemoveCategoryCommand : CommandBase
    {
        private ObservableCollection<NoteCategory> Categories { get; }

        public RemoveCategoryCommand(ObservableCollection<NoteCategory> categories)
        {
            this.Categories = categories;
        }

        public override void Execute(object parameter)
        {
            if (parameter is NoteCategory category)
            {
                this.Categories.Remove(category);
            }
        }
    }
}
