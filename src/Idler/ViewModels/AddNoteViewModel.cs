namespace Idler.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Extensions;
    using Idler.Helpers.Notifications;
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
        private bool predictingCategory;
        private CancellationTokenSource cancellationTokenSource;
        private bool? categoryChangedProgrammatically;
        private ICommand resumeAutoCategorizationCommand;

        // This variable is used to track how many auto categorization tasks are currently active
        private int activeAutoCategorizationTasksCount = 0;

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

                // We assume that CategoryId is set programmatically every time, it will be set to false when user selects
                // item in related ComboBox.
                this.CategoryChangedProgrammatically = true;
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

        public bool PredictingCategory
        {
            get => predictingCategory;
            set
            {
                predictingCategory = value;
                this.OnPropertyChanged();
            }
        }

        public bool? CategoryChangedProgrammatically
        {
            get => categoryChangedProgrammatically;
            set
            {
                categoryChangedProgrammatically = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand ResumeAutoCategorizationCommand
        {
            get => resumeAutoCategorizationCommand;
            set
            {
                resumeAutoCategorizationCommand = value;
                this.OnPropertyChanged();
            }
        }

        public AddNoteViewModel()
        {
            this.StartTime = DateTime.Now;
            this.PropertyChanged += AddNoteViewModelPropertyChanged;
            this.ResumeAutoCategorizationCommand = new ResumeAutoCategorization(this);
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
                    this.StartAutoCategorizationProcess();
                    break;
            }
        }

        public void ResetFields()
        {
            this.Effort = null;
            this.Description = String.Empty;
            this.FlushAutoCategorization();
        }

        public void StartAutoCategorizationProcess()
        {
            if (Settings.Default.IsAutoCategorizationEnabled && NlpModelManager.Instance.IsReady && (this.CategoryChangedProgrammatically == null || this.CategoryChangedProgrammatically.Value == true))
            {
                this.cancellationTokenSource?.Cancel();
                this.cancellationTokenSource?.Dispose();
                this.cancellationTokenSource = new CancellationTokenSource();

                Task.Run(() => NlpModelManager.Instance.PredictCategoryId(this.Description, this.cancellationTokenSource.Token), this.cancellationTokenSource.Token)
                    .SafeAsyncCall(predictedCategoryId =>
                    {
                        if (this.NoteCategories.Categories.Any(c => c.Id == predictedCategoryId && !c.Hidden))
                        {
                            this.CategoryId = predictedCategoryId;
                        }
                    },
                    isPredicting =>
                    {
                        if (isPredicting)
                        {
                            Interlocked.Increment(ref this.activeAutoCategorizationTasksCount);
                        }
                        else
                        {
                            Interlocked.Decrement(ref this.activeAutoCategorizationTasksCount);
                        }

                        // we need to set PredictingCategory to true only if we are not already predicting or there are active auto categorization tasks
                        this.PredictingCategory = isPredicting || this.activeAutoCategorizationTasksCount > 0;
                    },
                    (ex, isCanceled) =>
                    {
                        if (isCanceled)
                        {
                            // we don't need to show error if task was canceled
                            return;
                        }

                        NotificationsManager.Instance.ShowError($"Error has occurred while predicting category for the provided description.");
                    });
            }
        }

        public void OnComboBoxSelectionChanged()
        {
            this.FlushAutoCategorization();

            // Since user has selected category manually, we set the flag to false to disable auto categorization.
            if (this.CategoryChangedProgrammatically == null || this.CategoryChangedProgrammatically == true)
            {
                this.CategoryChangedProgrammatically = false;
            }
        }

        private void FlushAutoCategorization()
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = null;
            this.PredictingCategory = false;
            this.CategoryChangedProgrammatically = null;
        }
    }
}
