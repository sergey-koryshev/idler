using Idler.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Idler.ViewModels
{
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
