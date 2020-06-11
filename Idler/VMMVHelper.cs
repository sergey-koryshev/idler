using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    public abstract class VMMVHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Trace.TraceInformation($"Property '{propertyName}' has been changed to value '{this.GetType().GetProperty(propertyName).GetValue(this)}'");
        }
    }
}
