namespace Idler.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when any property has been changed except of property "Changed"
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers that specified property has been changed
        /// </summary>
        /// <param name="propertyName">(Optional) Name of property</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
