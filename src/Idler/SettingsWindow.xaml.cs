namespace Idler
{
    using System.ComponentModel;
    using System.Windows;
    using Idler.ViewModels;

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private SettingsViewModel settingsViewModel;

        public SettingsViewModel SettingsViewModel
        { 
            get => settingsViewModel; 
            set
            {
                settingsViewModel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SettingsViewModel)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(NoteCategories noteCategories) : this()
        {
            this.SettingsViewModel = new SettingsViewModel(noteCategories);
        }
    }
}
