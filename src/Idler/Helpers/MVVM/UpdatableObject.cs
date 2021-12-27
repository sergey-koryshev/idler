using Idler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Helpers.MVVM
{
    /// <summary>
    /// Represents updatable object
    /// </summary>
    public abstract class UpdatableObject : ObservableObject, IUpdatable
    {
        protected bool isRefreashing;

        /// <summary>
        /// Determines if the object is currently refreshing
        /// </summary>
        public bool IsRefreshing
        {
            get => isRefreashing;
            set
            {
                isRefreashing = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Occurs when refresh has been completed
        /// </summary>
        public event EventHandler RefreshCompleted;

        /// <summary>
        /// Occurs when refresh has been started
        /// </summary>
        public event EventHandler RefreshStarted;

        /// <summary>
        /// Occurs when updating has been completed
        /// </summary>
        public event EventHandler UpdateCompleted;

        /// <summary>
        /// Occurs when updating has been started
        /// </summary>
        public event EventHandler UpdateStarted;

        /// <summary>
        /// Notify that refreshing has been completed
        /// </summary>
        protected void OnRefreshCompleted()
        {
            this.IsRefreshing = false;
            this.RefreshCompleted?.Invoke(this, null);
            this.Changed = false;
        }

        /// <summary>
        /// Notify that refreshing has been started
        /// </summary>
        protected void OnRefreshStarted()
        {
            this.IsRefreshing = true;
            this.RefreshStarted?.Invoke(this, null);
        }

        /// <summary>
        /// Notify that updating has been completed
        /// </summary>
        protected void OnUpdateCompleted()
        {
            this.IsRefreshing = false;
            this.UpdateCompleted?.Invoke(this, null);
            this.Changed = false;
        }

        /// <summary>
        /// Notify that updating has been started
        /// </summary>
        protected void OnUpdateStarted()
        {
            this.IsRefreshing = true;
            this.UpdateStarted?.Invoke(this, null);
        }

        /// <summary>
        /// Refreshes the object
        /// </summary>
        public virtual async Task RefreshAsync() { }

        /// <summary>
        /// Saves the object
        /// </summary>
        public virtual async Task UpdateAsync() { }
    }
}
