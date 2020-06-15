using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    public abstract class VMMVHelper : INotifyPropertyChanged, IUpdatable
    {
        private bool changed;

        public bool Changed
        {
            get => this.changed;

            set
            {
                this.changed = value;
                OnPropertyChanged(nameof(this.Changed));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
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

        public virtual void Refresh()
        {
            this.Changed = false;
        }

        public virtual void Update()
        {
            this.Changed = false;
        }
    }
}
