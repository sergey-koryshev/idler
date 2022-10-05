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

        public string Version1
        {
            get { return version; }
            set { 
                version = value;
                this.OnPropertyChanged(this.Version);
            }
        }


        public About()
        {
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            this.Version = $"({appVersion.Major}.{appVersion.Minor})";
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
