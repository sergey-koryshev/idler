﻿namespace Idler.ViewModels
{
    using Idler.Commands;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    public class CategoriesListViewModel : BaseViewModel
    {
        private ObservableCollection<NoteCategory> categories;
        private ICommand removeCategoryCommand;

        public ObservableCollection<NoteCategory> Categories
        {
            get => categories;
            set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveCategoryCommand
        {
            get => removeCategoryCommand;
            set
            {
                removeCategoryCommand = value;
                this.OnPropertyChanged();
            }
        }

        public CategoriesListViewModel(ObservableCollection<NoteCategory> categories)
        {
            this.Categories = categories;
            this.RemoveCategoryCommand = new RemoveCategoryCommand(categories);
        }
    }
}
