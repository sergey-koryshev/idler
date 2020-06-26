using Idler.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private NoteCategories noteCategories;
        public NoteCategories NoteCategories
        {
            get => noteCategories;
            set
            {
                noteCategories = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.NoteCategories)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(NoteCategories noteCategories) : this()
        {
            this.NoteCategories = noteCategories;
        }

        private async void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            await this.NoteCategories.RefreshAsync();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            await this.NoteCategories.UpdateAsync();
        }

        private void btnDataSourceOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == true)
            {
                this.txtDataSource.Text = openFile.FileName;
            }
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
        }
    }
}
