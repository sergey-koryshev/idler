namespace Idler.Helpers.BackgroundManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using Idler.Extensions;
    using Idler.Models;

    /// <summary>
    /// Manages background tasks, providing methods to add and track active background tasks.
    /// Notifies subscribers when the list of active tasks changes.
    /// </summary>
    public class BackgroundTasksManager
    {
        private static readonly Lazy<BackgroundTasksManager> instance = new Lazy<BackgroundTasksManager>(() => new BackgroundTasksManager());
        private readonly List<BackgroundTask> activeTasksList = new List<BackgroundTask>();

        /// <summary>
        /// Gets the singleton instance of the <see cref="BackgroundTasksManager"/> class.
        /// </summary>
        public static BackgroundTasksManager Instance => instance.Value;

        /// <summary>
        /// Occurs when the list of active background tasks changes.
        /// </summary>
        public event EventHandler<List<BackgroundTask>> ActiveTasksListChanged;


        /// <summary>
        /// Prevents direct instantiation of the <see cref="BackgroundTasksManager"/> class.
        /// </summary>
        private BackgroundTasksManager() { }

        /// <summary>
        /// Adds a new background task to the manager and notifies listeners when the task completes or fails.
        /// </summary>
        /// <param name="task">The task to be tracked.</param>
        /// <param name="name">The name of the background task.</param>
        /// <param name="callback">An optional callback to invoke when the task completes.</param>
        /// <param name="errorCallback">An optional callback to invoke when the task fails.</param>
        public void AddBackgroundTask(Task task, string name, Action callback = null, Action<Exception> errorCallback = null)
        {
            BackgroundTask process = new BackgroundTask
            {
                Name = name,
                Action = task
            };

            task.SafeAsyncCall(() =>
            {
                this.DeleteBackgroundTask(process);
                callback?.Invoke();
            }, null, (e, __) =>
            {
                this.DeleteBackgroundTask(process);
                errorCallback?.Invoke(e);
            });

            Application.Current.Dispatcher.Invoke(() => {
                this.InsertBackgroundTask(process);
            });
        }

        private void DeleteBackgroundTask(BackgroundTask process)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.activeTasksList.Remove(process);
                this.ActiveTasksListChanged?.Invoke(this, this.activeTasksList);
            });
        }

        private void InsertBackgroundTask(BackgroundTask process)
        {
            Application.Current.Dispatcher.Invoke(() => {
                this.activeTasksList.Add(process);
                this.ActiveTasksListChanged?.Invoke(this, this.activeTasksList);
            });
        }
    }
}
