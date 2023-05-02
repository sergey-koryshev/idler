using Idler.Commands;
using Idler.Helpers.DB;
using Idler.ViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Idler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string appName = "Idler";
        private Shift currentShift;
        private string fullAppName;
        private NoteCategories noteCategories;
        private bool isBusy;
        private AddNoteViewModel addNoteViewModel;
        private ICommand saveShiftCommand;
        private DispatcherTimer reminder;

        public AddNoteViewModel AddNoteViewModel
        {
            get
            {
                if (this.addNoteViewModel == null)
                {
                    this.addNoteViewModel = this.CreateAddNoteViewModel();
                }

                return this.addNoteViewModel;
            }
        }

        public string FullAppName
        {
            get => this.fullAppName;
            set
            {
                this.fullAppName = value;
                OnPropertyChanged(nameof(this.FullAppName));
            }
        }

        public Shift CurrentShift
        {
            get => this.currentShift;
            set
            {
                this.currentShift = value;
                OnPropertyChanged(nameof(this.CurrentShift));
            }
        }

        public NoteCategories NoteCategories
        {
            get => this.noteCategories;
            set
            {
                this.noteCategories = value;
                OnPropertyChanged(nameof(this.NoteCategories));
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                this.isBusy = value;
                OnPropertyChanged(nameof(this.IsBusy));
            }
        }

        public decimal TotalEffort
        {
            get
            {
                decimal result = 0;

                if (this.CurrentShift != null)
                {
                    result += this.CurrentShift.TotalEffort;
                }

                if (this.AddNoteViewModel is AddNoteViewModel addNoteViewModel)
                {
                    result += addNoteViewModel.Effort;
                }

                return result;
            }
        }

        public ICommand SaveShiftCommand
        {
            get { return saveShiftCommand; }
            set
            {
                saveShiftCommand = value;
                this.OnPropertyChanged(nameof(this.SaveShiftCommand));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Trace.TraceInformation("Initializing main window");

            Properties.Settings.Default.SettingsSaving += OnSettignsSaving;

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.FullAppName = $"{appName} ({version})";

            this.InitializeReminer();

            InitializeComponent();

            this.PropertyChanged += MainWindowPropertyChangedHandler;

            this.NoteCategories = new NoteCategories();

            InitializeCurrentShift();
        }

        private void OnSettignsSaving(object sender, CancelEventArgs e)
        {
            this.reminder.Interval = Properties.Settings.Default.ReminderInterval;
            if (Properties.Settings.Default.ReminderInterval.Ticks > 0 && Properties.Settings.Default.IsReminderEnabled)
            {
                this.reminder.Start();
            }
            else
            {
                this.reminder.Stop();
            }
        }

        private void InitializeReminer()
        {
            this.reminder = new DispatcherTimer();
            this.reminder.Tick += OnReminderActivated;
            this.reminder.Interval = Properties.Settings.Default.ReminderInterval;
            if (Properties.Settings.Default.ReminderInterval.Ticks > 0 && Properties.Settings.Default.IsReminderEnabled)
            {

                this.reminder.Start();
            }
            else
            {
                this.reminder.Stop();
            }
        }

        private void OnReminderActivated(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
	        {
                return;
	        }
            new ToastContentBuilder()
                .AddArgument("action", "remindFillReport")
                .AddAppLogoOverride(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/reminder-icon.png")))
                .AddText("Idler Reminder")
                .AddText("Hey! Just remind you to fill your current work progress.")
                .Show();
        }

        private async Task InitializeCurrentShift()
        {
            if (Properties.Settings.Default.LastInteractedShiftId == 0)
            {
                Trace.TraceInformation("Creating new shift since last interacted shift id is equal to 0");

                this.CurrentShift = new Shift()
                {
                    Name = Shift.unnamedShiftPrevix,
                    PreviousShiftId = await Shift.GetLastShiftId()
                };
            }
            else
            {
                try
                {
                    Trace.TraceInformation($"Loading last interacted shift with id {Properties.Settings.Default.LastInteractedShiftId}");

                    this.CurrentShift = new Shift() { Id = Properties.Settings.Default.LastInteractedShiftId };
                    await this.CurrentShift.RefreshAsync();
                }
                catch (DataBaseRowNotFoundException ex)
                {
                    Trace.TraceInformation($"Creating new shift since last interacted shift with id {Properties.Settings.Default.LastInteractedShiftId} doesn't exist");

                    Trace.TraceInformation(ex.Message);
                    this.CurrentShift = new Shift()
                    {
                        Name = Shift.unnamedShiftPrevix,
                        PreviousShiftId = await Shift.GetLastShiftId()
                    };
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        private void MainWindowPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.CurrentShift):
                    if (this.CurrentShift.Id != null)
                    {
                        if (Properties.Settings.Default.LastInteractedShiftId != (int)this.CurrentShift.Id)
                        {
                            Properties.Settings.Default.LastInteractedShiftId = (int)this.CurrentShift.Id;
                            Properties.Settings.Default.Save();
                        }
                        this.CurrentShift.PropertyChanged += CurrentShiftPropertyChanged;
                    }
                    this.AddNoteViewModel.Shift = this.CurrentShift;
                    this.SaveShiftCommand = new SaveShiftCommand(this, this.CurrentShift);
                    break;
                case nameof(this.NoteCategories):
                    this.AddNoteViewModel.NoteCategories = this.NoteCategories.Categories;
                    break;
            }
        }

        private void CurrentShiftPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.CurrentShift.TotalEffort))
            {
                OnPropertyChanged(nameof(this.TotalEffort));
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await this.CurrentShift.RefreshAsync();
        }

        private async void BtnNextShift_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentShift.NextShiftId != null)
            {
                this.CurrentShift = new Shift() { Id = (int)this.CurrentShift.NextShiftId };
                await this.CurrentShift.RefreshAsync();
            }
            else
            {
                this.CurrentShift = new Shift()
                {
                    Name = Shift.unnamedShiftPrevix,
                    PreviousShiftId = this.CurrentShift.Id
                };
                OnPropertyChanged(nameof(this.TotalEffort));
            }
        }

        private async void BtnPreviousShift_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentShift.PreviousShiftId != null)
            {
                this.CurrentShift = new Shift() { Id = (int)this.CurrentShift.PreviousShiftId };
                await this.CurrentShift.RefreshAsync();
            }
        }

        private async void BtnRemoveShift_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentShift.Id != null)
            {
                await Shift.RemoveShiftByShiftId((int)this.CurrentShift.Id);
                await ShiftNote.RemoveShiftNotesByShiftId((int)this.CurrentShift.Id);

                if (this.CurrentShift.PreviousShiftId == null)
                {
                    if (this.CurrentShift.NextShiftId == null)
                    {
                        this.CurrentShift = new Shift()
                        {
                            Name = Shift.unnamedShiftPrevix
                        };
                    }
                    else
                    {
                        this.CurrentShift = new Shift() { Id = this.CurrentShift.NextShiftId };
                        await this.CurrentShift.RefreshAsync();
                    }
                }
                else
                {
                    this.CurrentShift = new Shift() { Id = this.CurrentShift.PreviousShiftId };
                    await this.CurrentShift.RefreshAsync();
                }
            }
        }

        private void MnuSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this.NoteCategories);
            settingsWindow.ShowDialog();
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        private void MnuContent_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/sergey-koryshev/Idler/tree/release/poc"));
        }

        private AddNoteViewModel CreateAddNoteViewModel()
        {
            var addNoteViewModel = new AddNoteViewModel();
            addNoteViewModel.PropertyChanged += AddNoteViewModelPropertyChanged;
            return addNoteViewModel;
        }

        private void AddNoteViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Effort")
            {
                OnPropertyChanged(nameof(this.TotalEffort));
            }
        }
    }
}
