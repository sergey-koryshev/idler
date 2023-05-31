using Idler.Commands;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Idler.ViewModels
{
    public class AddNoteViewModel : BaseViewModel
    {
        private ObservableCollection<NoteCategory> noteCategories;
        private ICollectionView filteredNoteCategories;
        private int categoryId;
        private decimal? effort;
        private string description;
        private DateTime startTime;
        private Shift shift;
        private XmlLanguage spellcheckLanguage;
        private ICommand addNoteCommand;

        public Shift Shift
        {
            get { return this.shift; }
            set
            {
                this.shift = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<NoteCategory> NoteCategories
        {
            get { return this.noteCategories; }
            set
            {
                this.noteCategories = value;
                this.OnPropertyChanged();
            }
        }

        public ICollectionView FilteredNoteCategories
        {
            get { return filteredNoteCategories; }
            private set
            {
                filteredNoteCategories = value;
                this.OnPropertyChanged();
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

        public decimal? Effort
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

        public XmlLanguage SpellcheckLanguage
        {
            get { return spellcheckLanguage; }
            set
            {
                spellcheckLanguage = value;
                this.OnPropertyChanged();
            }
        }


        public ICommand AddNoteCommand
        {
            get { return addNoteCommand; }
            set
            {
                addNoteCommand = value;
                this.OnPropertyChanged();
            }
        }

        public AddNoteViewModel()
        {
            this.StartTime = DateTime.Now;
            this.PropertyChanged += AddNoteViewModelPropertyChanged;
            var currentLanguageManager = InputLanguageManager.Current;
            this.SpellcheckLanguage = XmlLanguage.GetLanguage(currentLanguageManager.CurrentInputLanguage.Name);
            currentLanguageManager.InputLanguageChanged += InputLanguageChanged;
        }

        private void InputLanguageChanged(object sender, InputLanguageEventArgs e)
        {
            this.SpellcheckLanguage = XmlLanguage.GetLanguage(e.NewLanguage.Name);
        }

        private void AddNoteViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.NoteCategories):
                    this.UpdateFilteredNoteCategoriesView(this.NoteCategories);
                    break;
                case nameof(this.Shift):
                    this.CreateCommand(this.Shift);
                    break;
            }
        }

        private void CreateCommand(Shift shift)
        {
            this.AddNoteCommand = new AddNoteCommand(this, shift);
        }

        private void UpdateFilteredNoteCategoriesView(ObservableCollection<NoteCategory> noteCategories)
        {
            var newView = new CollectionViewSource() { Source = noteCategories };
            this.FilteredNoteCategories = newView.View;
            this.FilteredNoteCategories.Filter = FilterNoteCategories;
        }

        public void ResetFields()
        {
            this.Effort = 0;
            this.Description = String.Empty;
        }

        private bool FilterNoteCategories(object o)
        {
            if (o is NoteCategory noteCategory)
            {
                return !noteCategory.Hidden;
            }

            return false;
        }

        public void RefreshFilteredNoteCategoriesView()
        {
            this.FilteredNoteCategories.Refresh();
        }
    }
}
