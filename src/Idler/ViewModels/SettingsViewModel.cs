namespace Idler.ViewModels
{
    using System.Configuration;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Properties;

    public class SettingsViewModel : BaseViewModel
    {
        private NoteCategories noteCategories;
        private ICommand openXLSXDialogCommand;
        private bool areApplicationSettingsUnsaved;
        private bool isDataSourceChanged;
        private ICommand saveSettingsCommand;
        private ICommand resetSettingsCommand;
        private ICommand openDataSourceDialogCommand;

        public NoteCategories NoteCategories
        {
            get => noteCategories;
            set
            {
                noteCategories = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand OpenXLSXDialogCommand
        {
            get => openXLSXDialogCommand;
            set
            {
                openXLSXDialogCommand = value;
                this.OnPropertyChanged();
            }
        }

        public bool AreApplicationSettingsUnsaved
        {
            get => areApplicationSettingsUnsaved;
            set
            {
                areApplicationSettingsUnsaved = value;
                this.OnPropertyChanged(nameof(this.AreAllSettingsUnsaved));
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
            }
        }

        public ICommand SaveSettingsCommand
        {
            get => saveSettingsCommand;
            set
            {
                saveSettingsCommand = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand ResetSettingsCommand
        {
            get => resetSettingsCommand;
            set
            {
                resetSettingsCommand = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand OpenDataSourceDialogCommand
        {
            get => openDataSourceDialogCommand;
            set
            {
                openDataSourceDialogCommand = value;
                this.OnPropertyChanged();
            }
        }

        public SettingsViewModel(NoteCategories noteCategories)
        {
            this.OpenXLSXDialogCommand = new LaunchOpenDialogCommand("Microsoft Excel (*.xlsx)|*.xlsx", dialog =>
            {
                Settings.Default.ExcelTemplate = dialog.FileName;
            });
            this.OpenDataSourceDialogCommand = new LaunchOpenDialogCommand("Microsoft Access (*.mdb)|*.mdb", dialog =>
            {
                Settings.Default.DataSource = dialog.FileName;
            });
            this.SaveSettingsCommand = new SaveSettingsCommand(this);
            this.ResetSettingsCommand = new ResetSettingsCommand(this);
            this.NoteCategories = noteCategories;
            Settings.Default.SettingChanging += this.OnSettingChanging;
            this.NoteCategories.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.NoteCategories.Changed))
                {
                    this.OnPropertyChanged(nameof(this.AreAllSettingsUnsaved));
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

        public void ResetFlags()
        {
            this.AreApplicationSettingsUnsaved = false;
            this.IsDataSourceChanged = false;
        }
    }
}
