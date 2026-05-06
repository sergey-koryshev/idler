namespace Idler.ViewModels
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Extensions;
    using Idler.Helpers.Notifications;
    using Idler.Managers;
    using Idler.Models;
    using Idler.Properties;

    public class SettingsViewModel : BaseDialogViewModel
    {
        private NoteCategories noteCategories;
        private ICommand openXLSXDialogCommand;
        private bool areApplicationSettingsUnsaved;
        private bool isDataSourceChanged;
        private ICommand saveSettingsCommand;
        private ICommand resetSettingsCommand;
        private ICommand openDataSourceDialogCommand;
        private AddCategoryViewModel addCategoryViewModel;
        private CategoriesListViewModel categoriesListViewModel;
        private ICommand retrainAutoCategorizationModelCommand;
        private bool isAutoCategorizationModelBusy;
        private DateTime? autoCategorizationModelLastTrainedOn;
        private bool isAutoCategorizationChanged;

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

        public AddCategoryViewModel AddCategoryViewModel
        {
            get => addCategoryViewModel;
            set
            {
                addCategoryViewModel = value;
                this.OnPropertyChanged();
            }
        }

        public CategoriesListViewModel CategoriesListViewModel
        {
            get => categoriesListViewModel;
            set
            {
                categoriesListViewModel = value;
                OnPropertyChanged();
            }
        }

        public ICommand RetrainAutoCategorizationModelCommand
        {
            get => retrainAutoCategorizationModelCommand;
            set
            {
                retrainAutoCategorizationModelCommand = value;
                OnPropertyChanged();
            }
        }

        public bool IsAutoCategorizationModelBusy
        {
            get => isAutoCategorizationModelBusy;
            set
            {
                isAutoCategorizationModelBusy = value;
                OnPropertyChanged();
            }
        }

        public DateTime? AutoCategorizationModelLastTrainedOn
        {
            get => autoCategorizationModelLastTrainedOn;
            set
            {
                autoCategorizationModelLastTrainedOn = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsAutoCategorizationChanged
        { 
            get => isAutoCategorizationChanged;
            set
            {
                isAutoCategorizationChanged = value;
                this.OnPropertyChanged();
            }
        }

        public event EventHandler<bool> AutoCategorizationStateChanged;

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
            this.CategoriesListViewModel = new CategoriesListViewModel(this.noteCategories.Categories);
            this.AddCategoryViewModel = new AddCategoryViewModel(this.NoteCategories.Categories, CategoriesListViewModel);
            this.RetrainAutoCategorizationModelCommand = new RetrainAutoCategorizationModelCommand();
            this.IsAutoCategorizationModelBusy = this.GetAutoCategorizationModelBusyStatus(NlpModelManager.Instance.NlpModelStatus);
            this.AutoCategorizationModelLastTrainedOn = NlpModelManager.Instance.ModelMetadata?.TrainedOn?.ToLocalTime();

            NlpModelManager.Instance.ModelStatusChanged += (s, status) =>
            {
                this.IsAutoCategorizationModelBusy = this.GetAutoCategorizationModelBusyStatus(status);
            };

            NlpModelManager.Instance.ModelMetadataChanged += (s, metadata) =>
            {
                this.AutoCategorizationModelLastTrainedOn = metadata?.TrainedOn?.ToLocalTime();
            };
        }

        private void OnSettingChanging(object sender, SettingChangingEventArgs e)
        {
            this.AreApplicationSettingsUnsaved = true;

            if (e.SettingName == nameof(Settings.Default.DataSource))
            {
                this.IsDataSourceChanged = true;
            }

            if (e.SettingName == nameof(Settings.Default.IsAutoCategorizationEnabled))
            {
                this.IsAutoCategorizationChanged = true;
            }
        }

        public void ResetFlags()
        {
            this.AreApplicationSettingsUnsaved = false;
            this.IsDataSourceChanged = false;
        }

        public Task ResetSettings()
        {
            Settings.Default.Reload();
            return this.NoteCategories
                .RefreshAsync()
                .SafeAsyncCall((_) => this.ResetFlags(), null, _ => NotificationsManager.Instance.ShowError("Failed to reload categories."));
        }

        public override Task OnDialogClosing() => this.ResetSettings();

        public void OnAutoCategorizationStateChanged()
        {
            this.AutoCategorizationStateChanged?.Invoke(this, Settings.Default.IsAutoCategorizationEnabled);
        }

        private bool GetAutoCategorizationModelBusyStatus(NlpModelStatus status)
        {
            return status != NlpModelStatus.None && NlpModelStatus.InProgress.HasFlag(status);
        }
    }
}
