namespace Idler.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Idler.Commands;
    using Idler.Extensions;
    using Idler.Helpers.Notifications;
    using Idler.Managers;
    using Idler.Properties;

    public class AddNoteViewModel : BaseViewModel
    {
        private const int DebounceDelayMs = 300;

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
        private CancellationTokenSource autoCategorizationCancellationTokenSource;
        private CancellationTokenSource autoCompleteCancellationTokenSource;
        private bool? categoryChangedProgrammatically;
        private ICommand resumeAutoCategorizationCommand;
        private DispatcherTimer descriptionDebounceTimer;
        private AutoCompleteManager autoCompleteManager;
        private int activeAutoCategorizationTasksCount = 0; // This variable is used to track how many auto categorization tasks are currently active
        private string suggestingDescription;

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

        /// <summary>
        /// Gets or sets a value indicating whether the category was changed programmatically.
        /// </summary>
        /// <remarks>
        /// The property can have the following values: <see langword="null"/> - category was not changed yet;
        /// <see langword="true"/> - category was changed programmatically; <see langword="false"/> - category
        /// was changed manually (via ComboBox).
        /// </remarks>
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

        public string SuggestingDescription
        {
            get => suggestingDescription;
            set
            {
                suggestingDescription = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether automatic categorization is enabled.
        /// </summary>
        public bool IsAutoCategorizationEnabled => Settings.Default.IsAutoCategorizationEnabled &&
            NlpModelManager.Instance.IsReady &&
            (this.CategoryChangedProgrammatically == null || this.CategoryChangedProgrammatically.Value == true);

        public AddNoteViewModel()
        {
            this.StartTime = DateTime.Now;
            this.PropertyChanged += AddNoteViewModelPropertyChanged;
            this.ResumeAutoCategorizationCommand = new ResumeAutoCategorization(this);
            this.autoCompleteManager = new AutoCompleteManager();
            this.InitializeDescriptionDebounceTimer();
        }

        /// <summary>
        /// Initializes the debounce timer with the specified interval and event handler.
        /// </summary>
        /// <remarks>The timer is configured to use the interval defined by <see cref="DebounceDelayMs"/> 
        /// and triggers the <see cref="OnDebounceTimerTick"/> method when the timer elapses.</remarks>
        private void InitializeDescriptionDebounceTimer()
        {
            this.descriptionDebounceTimer = new DispatcherTimer();
            this.descriptionDebounceTimer.Interval = TimeSpan.FromMilliseconds(DebounceDelayMs);
            this.descriptionDebounceTimer.Tick += this.OnDebounceTimerTick;
        }

        /// <summary>
        /// Handles the tick event of the debounce timer, stopping the timer and triggering the auto-categorization
        /// process.
        /// </summary>
        /// <param name="sender">The source of the event, typically the debounce timer.</param>
        /// <param name="e">The event data associated with the timer tick.</param>
        private void OnDebounceTimerTick(object sender, EventArgs e)
        {
            this.descriptionDebounceTimer.Stop();
            this.ExecuteAutoCategorizationProcess();
            this.TriggerDescriptionAutoCompleteProcess();
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
                    this.TriggerDescriptionDebounceTimer();
                    break;
            }
        }

        public void ResetFields()
        {
            this.Effort = null;
            this.Description = String.Empty;
            this.FlushAutoCategorization();
            this.CancelAutoCompleteProcess();
        }

        /// <summary>
        /// Starts the debounce process for the auto-categorization feature.
        /// </summary>
        public void TriggerDescriptionDebounceTimer()
        {
            this.descriptionDebounceTimer.Stop();
            this.CancelAutoCompleteProcess(); // we cancel auto-complete process immediately here to avoid showing late suggestion in UI
            
            if (this.IsAutoCategorizationEnabled ||
                Settings.Default.IsAutoCompleteEnabled)
            {
                this.descriptionDebounceTimer.Start();
            }
        }

        private void ExecuteAutoCategorizationProcess()
        {
            if (this.IsAutoCategorizationEnabled)
            {
                this.autoCategorizationCancellationTokenSource?.Cancel();
                this.autoCategorizationCancellationTokenSource?.Dispose();
                this.autoCategorizationCancellationTokenSource = new CancellationTokenSource();

                Task.Run(() => NlpModelManager.Instance.PredictCategoryId(this.Description, this.autoCategorizationCancellationTokenSource.Token), this.autoCategorizationCancellationTokenSource.Token)
                    .SafeAsyncCall((predictedCategoryId, _) =>
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
                    ex => NotificationsManager.Instance.ShowError($"Error has occurred while predicting category for the provided description."));
            }
        }

        private void TriggerDescriptionAutoCompleteProcess()
        {
            if (!Settings.Default.IsAutoCompleteEnabled)
            {
                return;
            }

            this.autoCompleteCancellationTokenSource?.Cancel();
            this.autoCompleteCancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => this.autoCompleteManager.GetSuggestion(this.Description, this.autoCompleteCancellationTokenSource.Token), this.autoCompleteCancellationTokenSource.Token)
                .SafeAsyncCall((suggestingDescription, cancellationToken) =>
                {
                    this.SuggestingDescription = cancellationToken.IsCancellationRequested ? null : suggestingDescription;
                },
                null,
                (error) => Trace.TraceError("Error has occurred while finding description for auto-complete: {0}", error),
                this.autoCompleteCancellationTokenSource.Token);
        }

        /// <summary>
        /// Handles the event when the selection in the combo box changes.
        /// </summary>
        /// <remarks>
        /// This method disables automatic categorization by setting the 
        /// CategoryChangedProgrammatically property to false if it is either null or true. Additionally, it flushes
        /// any pending auto-categorization operations.
        /// </remarks>
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
            this.descriptionDebounceTimer.Stop();
            this.autoCategorizationCancellationTokenSource?.Cancel();
            this.autoCategorizationCancellationTokenSource?.Dispose();
            this.autoCategorizationCancellationTokenSource = null;
            this.PredictingCategory = false;
            this.CategoryChangedProgrammatically = null;
        }

        private void CancelAutoCompleteProcess()
        {
            this.autoCompleteCancellationTokenSource?.Cancel();
            this.SuggestingDescription = null;
        }
    }
}
