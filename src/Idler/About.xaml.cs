using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Idler
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window, INotifyPropertyChanged
    {
        private string version;
        private string copyright;

        public string Version
        {
            get { return version; }
            set { 
                version = value;
                this.OnPropertyChanged(this.Version);
            }
        }

        public string Copyright
        {
            get { return copyright; }
            set
            {
                copyright = value;
                this.OnPropertyChanged(this.Copyright);
            }
        }

        public About()
        {
            var appVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            this.Version = $"({appVersion})";
            this.Copyright = $"Copyright (C) 2020-{DateTime.Now.Year}  Sergey Koryshev";
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void gitHubLinkClicked(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
