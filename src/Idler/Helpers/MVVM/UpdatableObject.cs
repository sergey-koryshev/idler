namespace Idler.Helpers.MVVM
{
    using System;
    using System.Threading.Tasks;
    using Idler.Interfaces;

    public abstract class UpdatableObject : ObservableObject, IUpdatable
    {
        protected bool isRefreshing;

        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                this.OnPropertyChanged();
            }
        }

        public event EventHandler RefreshCompleted;

        public event EventHandler RefreshStarted;

        public event EventHandler UpdateCompleted;

        public event EventHandler UpdateStarted;

        public async Task RefreshAsync()
        {
            try
            {
                this.OnRefreshStarted();
                await this.RefreshInternalAsync();
                this.Changed = false;
            }
            finally
            {
                this.OnRefreshCompleted();
            }
        }

        public async Task UpdateAsync() {
            try
            {
                this.OnUpdateStarted();
                await this.UpdateInternalAsync();
                this.Changed = false;
            }
            finally
            {
                this.OnUpdateCompleted();
            }
        }

        protected abstract Task RefreshInternalAsync();

        protected abstract Task UpdateInternalAsync();

        private void OnRefreshCompleted()
        {
            this.IsRefreshing = false;
            this.RefreshCompleted?.Invoke(this, null);
        }

        private void OnRefreshStarted()
        {
            this.IsRefreshing = true;
            this.RefreshStarted?.Invoke(this, null);
        }

        private void OnUpdateCompleted()
        {
            this.IsRefreshing = false;
            this.UpdateCompleted?.Invoke(this, null);
        }

        private void OnUpdateStarted()
        {
            this.IsRefreshing = true;
            this.UpdateStarted?.Invoke(this, null);
        }
    }
}
