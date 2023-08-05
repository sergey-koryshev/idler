using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            var appVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.Version = $"({appVersion})";
            this.Copyright = $"Copyright (C) {DateTime.Now.Year}  Sergey Koryshev";
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
