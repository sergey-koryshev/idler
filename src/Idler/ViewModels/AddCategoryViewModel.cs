namespace Idler.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Idler.Commands;

    public class AddCategoryViewModel : BaseViewModel
    {
        private string name;
        private bool hidden;
        private ICommand addCategoryCommand;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                this.OnPropertyChanged();
            }
        }

        public bool Hidden
        {
            get => hidden;
            set
            {
                hidden = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand AddCategoryCommand
        {
            get => addCategoryCommand;
            set
            {
                addCategoryCommand = value;
                this.OnPropertyChanged();
            }
        }

        public AddCategoryViewModel(ObservableCollection<NoteCategory> categories)
        {
            this.AddCategoryCommand = new AddCategoryCommand(this, categories);
        }
    }
}
