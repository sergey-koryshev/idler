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
    /// <summary>
    /// Represents observable object
    /// </summary>
    public abstract class ObservableObject: INotifyPropertyChanged
    {
        protected bool changed;

        /// <summary>
        /// Determines if the object has been changed
        /// </summary>
        public bool Changed
        {
            get => this.changed;

            set
            {
                this.changed = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Occurs when any property has been changed except of property "Changed"
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers that specified property has been changed
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Trace.TraceInformation($"Property '{propertyName}' has been changed to value '{this.GetType().GetProperty(propertyName).GetValue(this)}'");

            // some properties don't mean the object has been changed
            switch (propertyName)
            {
                case nameof(this.Changed):
                    break;
                default:
                    this.Changed = true;
                    break;
            }
        }
    }
}
