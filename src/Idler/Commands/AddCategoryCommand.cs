namespace Idler.Commands
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Idler.ViewModels;

    public class AddCategoryCommand : CommandBase
    {
        private AddCategoryViewModel ViewModel { get; }
        
        private ObservableCollection<NoteCategory> Categories { get; }

        private CategoriesListViewModel CategoriesListViewModel { get; }

        public AddCategoryCommand(AddCategoryViewModel viewModel, ObservableCollection<NoteCategory> categories, CategoriesListViewModel categoriesListViewModel)
        {
            this.ViewModel = viewModel;
            this.ViewModel.PropertyChanged += ViewModelPropertyChangeHandler;

            this.Categories = categories;
            this.CategoriesListViewModel = categoriesListViewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return !string.IsNullOrWhiteSpace(this.ViewModel.Name);
        }

        public override void Execute(object parameter)
        {
            NoteCategory newCategory = new NoteCategory()
            {
                Name = this.ViewModel.Name,
                Hidden = this.ViewModel.Hidden
            };

            this.Categories.Add(newCategory);
            this.CategoriesListViewModel.ItemToScroll = newCategory;

            this.ViewModel.Name = string.Empty;
            this.ViewModel.Hidden = false;
        }

        private void ViewModelPropertyChangeHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AddCategoryViewModel.Name))
            {
                this.OnCanExecutedChanged();
            }
        }
    }
}
