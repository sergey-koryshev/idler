using Idler.Commands;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Idler.ViewModels
{
    public class AddNoteViewModel : BaseViewModel
    {
        private ObservableCollection<NoteCategory> noteCategories;
        private int categoryId;
        private decimal effort;
        private string description;
        private DateTime startTime;

        public ObservableCollection<NoteCategory> NoteCategories
        {
            get { return this.noteCategories; }
            set
            {
                this.noteCategories = value;
                this.OnPropertyChanged();
            }
        }

        private ICollectionView filteredNoteCategories;

        public ICollectionView FilteredNoteCategories
        {
            get { return filteredNoteCategories; }
            set
            {
                filteredNoteCategories = value; this.OnPropertyChanged();
            }
        }


        public int CategoryId
        {
            get { return categoryId; }
            set
            {
                categoryId = value;
                this.OnPropertyChanged();
            }
        }

        public decimal Effort
        {
            get { return effort; }
            set
            {
                effort = value;
                this.OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                this.OnPropertyChanged();
            }
        }

        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand AddNoteCommand { get; }

        public AddNoteViewModel(ObservableCollection<NoteCategory> noteCategories, Shift shift)
        {
            this.NoteCategories = noteCategories;
            var newView = new CollectionViewSource() { Source = noteCategories };
            this.FilteredNoteCategories = newView.View;
            this.FilteredNoteCategories.Filter = FilterNoteCategories;
            this.AddNoteCommand = new AddNoteCommand(this, shift);
            this.StartTime = DateTime.Now;
        }

        public void ResetFields()
        {
            this.Effort = 0;
            this.Description = String.Empty;
        }

        private bool FilterNoteCategories(object o)
        {
            Console.WriteLine(this.StartTime);
            if (o is NoteCategory noteCategory)
            {
                return !noteCategory.Hidden;
            }

            return false;
        }
    }
}
