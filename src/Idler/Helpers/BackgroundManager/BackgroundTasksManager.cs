namespace Idler.Helpers.BackgroundManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Idler.Extensions;
    using Idler.Models;

    /// <summary>
    /// Manages background tasks, providing methods to add and track active background tasks.
    /// Notifies subscribers when the list of active tasks changes.
    /// </summary>
    public class BackgroundTasksManager
    {
        private static BackgroundTasksManager instance;

        /// <summary>
        /// Gets the singleton instance of the <see cref="BackgroundTasksManager"/>.
        /// </summary>
        /// <returns>The singleton instance.</returns>
        public static BackgroundTasksManager GetInstance()
        {
            if (instance == null)
            {
                instance = new BackgroundTasksManager();
            }

            return instance;
        }

        /// <summary>
        /// The list of currently active background tasks.
        /// </summary>
        private List<BackgroundTask> activeTasksList = new List<BackgroundTask>();

        /// <summary>
        /// Occurs when the list of active background tasks changes.
        /// </summary>
        public event EventHandler<List<BackgroundTask>> ActiveTasksListChanged;

        /// <summary>
        /// Adds a new background task to the manager and notifies listeners when the task completes or fails.
        /// </summary>
        /// <param name="task">The task to be tracked.</param>
        /// <param name="name">The name of the background task.</param>
        /// <param name="callback">An optional callback to invoke when the task completes.</param>
        public void AddBackgroundTask(Task task, string name, Action callback = null)
        {
            BackgroundTask process = new BackgroundTask
            {
                Name = name,
                Action = task
            };

            task.SafeAsyncCall(() =>
            {
                this.activeTasksList.Remove(process);
                this.ActiveTasksListChanged.Invoke(this, this.activeTasksList);
                callback?.Invoke();
            }, null, (e) =>
            {
                this.activeTasksList.Remove(process);
                this.ActiveTasksListChanged.Invoke(this, this.activeTasksList);
            });

            this.activeTasksList.Add(process);
            this.ActiveTasksListChanged.Invoke(this, this.activeTasksList);
        }
    }
}
