namespace Idler.ViewModels
{
    using System.Collections.ObjectModel;
    using Idler.Helpers.BackgroundManager;
    using Idler.Models;

    /// <summary>
    /// ViewModel that manages and exposes the list of active background tasks.
    /// </summary>
    public class BackgroundTasksViewModel : BaseViewModel
    {
        /// <summary>
        /// Backing field for <see cref="ActiveTasksList"/>.
        /// </summary>
        private ObservableCollection<BackgroundTask> activeTasksList;

        /// <summary>
        /// Reference to the singleton background tasks manager.
        /// </summary>
        private BackgroundTasksManager backgroundManager;

        /// <summary>
        /// Gets or sets the collection of currently active background tasks.
        /// </summary>
        public ObservableCollection<BackgroundTask> ActiveTasksList
        { 
            get => activeTasksList;
            set
            {
                activeTasksList = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTasksViewModel"/> class.
        /// Subscribes to the <see cref="BackgroundTasksManager.ActiveTasksListChanged"/> event to keep the task list updated.
        /// </summary>
        public BackgroundTasksViewModel()
        {
            this.ActiveTasksList = new ObservableCollection<BackgroundTask>();
            this.backgroundManager = BackgroundTasksManager.GetInstance();
            this.backgroundManager.ActiveTasksListChanged += (sender, processes) =>
            {
                this.ActiveTasksList = new ObservableCollection<BackgroundTask>(processes);
            };
        }
    }
}
