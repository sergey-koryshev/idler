using Idler.Commands;
using Idler.Properties;
using Microsoft.Win32;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace Idler
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private NoteCategories noteCategories;
        private ICommand openXLSXDialogCommand;
        private bool areApplicationSettingsUnsaved;
        private bool isDataSourceChanged;

        public NoteCategories NoteCategories
        {
            get => noteCategories;
            set
            {
                noteCategories = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.NoteCategories)));
            }
        }

        public ICommand OpenXLSXDialogCommand 
        { 
            get => openXLSXDialogCommand;
            set
            {
                openXLSXDialogCommand = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.OpenXLSXDialogCommand)));
            }
        }

        public bool AreApplicationSettingsUnsaved
        { 
            get => areApplicationSettingsUnsaved;
            set
            {
                areApplicationSettingsUnsaved = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AreAllSettingsUnsaved)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AreApplicationSettingsUnsaved)));
            }
        }

        public bool AreAllSettingsUnsaved
        {
            get => this.AreApplicationSettingsUnsaved || (this.NoteCategories?.Changed ?? false);
        }

        public bool IsDataSourceChanged 
        { 
            get => isDataSourceChanged;
            set
            {
                isDataSourceChanged = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsDataSourceChanged)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsWindow()
        {
            InitializeComponent();
            this.OpenXLSXDialogCommand = new RelayCommand(OpenExcelTemplate);
            Settings.Default.SettingChanging += this.OnSettingChanging;
        }

        public SettingsWindow(NoteCategories noteCategories) : this()
        {
            this.NoteCategories = noteCategories;
            this.NoteCategories.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.NoteCategories.Changed))
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AreAllSettingsUnsaved)));
                }
            };
        }

        private void OnSettingChanging(object sender, SettingChangingEventArgs e)
        {
            this.AreApplicationSettingsUnsaved = true;

            if (e.SettingName == nameof(Settings.Default.DataSource))
            {
                this.IsDataSourceChanged = true;
            }
        }

        private async void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            await this.NoteCategories.RefreshAsync();
            this.ResetFlags();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            if (this.IsDataSourceChanged == false)
            {
                await this.NoteCategories.UpdateAsync();
            }
            this.ResetFlags();
        }

        private void btnDataSourceOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == true)
            {
                Settings.Default.DataSource = openFile.FileName;
            }
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            this.ResetFlags();
        }

        private void OpenExcelTemplate()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Microsoft Excel (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                Settings.Default.ExcelTemplate = dialog.FileName;
            }
        }

        private void ResetFlags()
        {
            this.AreApplicationSettingsUnsaved = false;
            this.IsDataSourceChanged = false;
        }
    }
}
