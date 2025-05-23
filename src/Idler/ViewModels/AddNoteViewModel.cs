namespace Idler.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Managers;
    using Idler.Properties;

    public class AddNoteViewModel : BaseViewModel
    {
        private NoteCategories noteCategories;
        private int categoryId;
        private decimal? effort;
        private string description;
        private DateTime startTime;
        private Shift shift;
        private ICommand addNoteCommand;
        private ListNotesViewModel listNotesViewModel;
        private ObservableCollection<NoteCategory> categories;
        private NlpModelManager nlpModelManager;

        public Shift Shift
        {
            get { return this.shift; }
            set
            {
                this.shift = value;
                this.OnPropertyChanged();
            }
        }

        public NoteCategories NoteCategories
        {
            get { return this.noteCategories; }
            set
            {
                this.noteCategories = value;
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

        public ICommand AddNoteCommand
        {
            get { return addNoteCommand; }
            set
            {
                addNoteCommand = value;
                this.OnPropertyChanged();
            }
        }

        public ListNotesViewModel ListNotesViewModel
        {
            get { return listNotesViewModel; }
            set
            {
                listNotesViewModel = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<NoteCategory> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                this.OnPropertyChanged();
            }
        }

        public AddNoteViewModel()
        {
            this.StartTime = DateTime.Now;
            this.PropertyChanged += AddNoteViewModelPropertyChanged;
            this.nlpModelManager = NlpModelManager.GetInstance();
        }

        private void AddNoteViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.Shift):
                case nameof(this.ListNotesViewModel):
                    this.AddNoteCommand = new AddNoteCommand(this, shift, listNotesViewModel);
                    break;
                case nameof(this.NoteCategories):
                    this.Categories = this.NoteCategories.Categories;

                    // Fixes binding category Id in ComboBox when list of categories are refreshed
                    // since ComboBox doesn't do it by itself
                    this.NoteCategories.RefreshCompleted += (s, a) => this.OnPropertyChanged(nameof(this.CategoryId));
                    break;
                case nameof(this.Description):
                    if (Settings.Default.IsAutoCategorizationEnabled && this.nlpModelManager.IsReady)
                    {
                        // TODO: implement this in a background thread
                        var predictedCategoryId = this.nlpModelManager.PredictCategoryId(this.Description);

                        if (this.NoteCategories.Categories.Any(c => c.Id == predictedCategoryId && !c.Hidden))
                        {
                            this.CategoryId = predictedCategoryId;
                        }
                    }
                    
                    break;
            }
        }

        public void ResetFields()
        {
            this.Effort = null;
            this.Description = String.Empty;
        }
    }
}
