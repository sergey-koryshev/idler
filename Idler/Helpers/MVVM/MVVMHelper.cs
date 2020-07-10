using Idler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Helpers.MVVM
{
    public abstract class MVVMHelper : INotifyPropertyChanged, IUpdatable
    {
        private bool changed;
        private bool isRefreashing;

        public bool Changed
        {
            get => this.changed;

            set
            {
                this.changed = value;
                OnPropertyChanged();
            }
        }

        public bool IsRefreashing
        {
            get => isRefreashing;
            set
            {
                isRefreashing = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler RefreshCompleted;
        public event EventHandler RefreshStarted;
        public event EventHandler UpdateCompleted;
        public event EventHandler UpdateStarted;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Trace.TraceInformation($"Property '{propertyName}' has been changed to value '{this.GetType().GetProperty(propertyName).GetValue(this)}'");
            switch (propertyName)
            {
                case nameof(this.Changed):
                    break;
                default:
                    this.Changed = true;
                    break;
            }
        }

        protected void OnRefreshCompleted()
        {
            this.IsRefreashing = false;
            this.RefreshCompleted?.Invoke(this, null);
            this.Changed = false;
        }

        protected void OnRefreshStarted()
        {
            this.IsRefreashing = true;
            this.RefreshStarted?.Invoke(this, null);
        }

        protected void OnUpdateCompleted()
        {
            this.IsRefreashing = false;
            this.UpdateCompleted?.Invoke(this, null);
            this.Changed = false;
        }

        protected void OnUpdateStarted()
        {
            this.IsRefreashing = true;
            this.UpdateStarted?.Invoke(this, null);
        }

        public virtual async Task RefreshAsync() { }

        public virtual async Task UpdateAsync() { }
    }
}
