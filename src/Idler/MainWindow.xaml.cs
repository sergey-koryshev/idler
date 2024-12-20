﻿namespace Idler
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Components;
    using Idler.Components.PopupDialogControl;
    using Idler.Extensions;
    using Idler.Helpers.DB;
    using Idler.Helpers.Notifications;
    using Idler.Properties;
    using Idler.ViewModels;
    using Idler.Views;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string appName = "Idler";

        private readonly NotificationsManager notificationsManager;

        private string fullAppName;
        private string fullAppVersion;
        private NoteCategories noteCategories;
        private bool isBusy;
        private AddNoteViewModel addNoteViewModel;
        private DateTime selectedDate;
        private ICommand saveShiftCommand;
        private Shift currentShift;
        private ListNotesViewModel listNotesViewModel;
        private ICommand refreshNotesCommand;
        private PopupDialogHost dialogHost;
        private ICommand exportNotesCommand;
        private ICommand changeSelectedDateCommand;
        private Dictionary<DateTime, decimal> daysToHighlight;
        private DateTime displayDate;
        private ICommand openAboutPopUpCommand;
        private ICommand openHelpUrlCommand;
        private ICommand openSettingsPopUpCommand;

        private Action<bool> SetProcessing => new Action<bool>(x => this.Dispatcher.Invoke(() => this.IsBusy = x));

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

        public NoteCategories NoteCategories
        {
            get => this.noteCategories;
            set
            {
                this.noteCategories = value;
                OnPropertyChanged(nameof(this.NoteCategories));
            }
        }

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                this.OnPropertyChanged(nameof(this.SelectedDate));
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

        public Shift CurrentShift
        {
            get => currentShift;
            set
            {
                currentShift = value;
                OnPropertyChanged(nameof(this.CurrentShift));
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
                    result += addNoteViewModel.Effort ?? 0;
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

        public ListNotesViewModel ListNotesViewModel {
            get => listNotesViewModel;
            set { 
                listNotesViewModel = value;
                this.OnPropertyChanged(nameof(this.ListNotesViewModel));
            }
        }

        public ICommand RefreshNotesCommand
        {
            get => refreshNotesCommand;
            set
            {
                refreshNotesCommand = value;
                this.OnPropertyChanged(nameof(this.RefreshNotesCommand));
            }
        }

        public PopupDialogHost DialogHost
        {
            get => dialogHost;
            set { 
                dialogHost = value;
                this.OnPropertyChanged(nameof(this.DialogHost));
            }
        }

        public ICommand ExportNotesCommand { 
            get => exportNotesCommand;
            set
            {
                exportNotesCommand = value;
                this.OnPropertyChanged(nameof(this.ExportNotesCommand));
            }
        }

        public ICommand ChangeSelectedDateCommand 
        { 
            get => changeSelectedDateCommand;
            set
            {
                changeSelectedDateCommand = value;
                this.OnPropertyChanged(nameof(this.ChangeSelectedDateCommand));
            }
        }

        public Dictionary<DateTime, decimal> DaysToHighlight
        {
            get => daysToHighlight;
            set
            {
                daysToHighlight = value;
                this.OnPropertyChanged(nameof(this.DaysToHighlight));
            }
        }

        public DateTime DisplayDate 
        { 
            get => displayDate;
            set
            {
                displayDate = value;
                this.OnPropertyChanged(nameof(this.DisplayDate));
            }
        }

        public ICommand OpenAboutPopUpCommand
        {
            get => openAboutPopUpCommand;
            set
            {
                openAboutPopUpCommand = value;
                this.OnPropertyChanged(nameof(this.OpenAboutPopUpCommand));
            }
        }

        public ICommand OpenHelpUrlCommand
        {
            get => openHelpUrlCommand;
            set
            {
                openHelpUrlCommand = value;
                this.OnPropertyChanged(nameof(this.OpenHelpUrlCommand));
            }
        }

        public ICommand OpenSettingsPopUpCommand
        {
            get => openSettingsPopUpCommand;
            set
            {
                openSettingsPopUpCommand = value;
                this.OnPropertyChanged(nameof(this.OpenSettingsPopUpCommand));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Trace.TraceInformation("Initializing main window");

            this.FullAppName = $"{appName}";
            this.fullAppVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            this.RestoreWindowPosition();

            this.InitializeComponent();

            // displays selected date immediately in main window for better user experience
            this.SelectedDate = Settings.Default.SelectedDate != default ? Settings.Default.SelectedDate : DateTime.Now;

            this.Closing += WindowClosingHandler;
            this.PropertyChanged += MainWindowPropertyChangedHandler;
            Settings.Default.SettingsSaving += this.OnSettignsChanging;
            DataBaseConnection.ConnectionStringChanged += OnConnectionStringChanged;

            this.notificationsManager = NotificationsManager.GetInstance();
            this.DialogHost = new PopupDialogHost();
            this.NoteCategories = new NoteCategories();
            this.CurrentShift = new Shift();

            this.ListNotesViewModel = new ListNotesViewModel(this.NoteCategories, this.CurrentShift.Notes);
            this.ExportNotesCommand = new OpenPopUpCommand<ExportNotesView>(this.DialogHost, "Export Notes", () => new ExportNotesViewModel());
            this.OpenAboutPopUpCommand = new OpenPopUpCommand<AboutView>(this.DialogHost, "About", () => new AboutViewModel());
            this.OpenSettingsPopUpCommand = new OpenPopUpCommand<SettingsView>(this.DialogHost, "Settings", () => new SettingsViewModel(this.NoteCategories));
            this.ChangeSelectedDateCommand = new ChangeSelectedDateCommand(this);
            this.OpenHelpUrlCommand = new OpenUrlCommand($"https://github.com/sergey-koryshev/idler/tree/v{this.fullAppVersion}#readme");

            this.Dispatcher.Invoke(async () =>
            {
                await this.NoteCategories.RefreshAsync();

                // initiates loading notes for selected dates
                this.OnPropertyChanged(nameof(this.SelectedDate));
            }).SafeAsyncCall(null, this.SetProcessing, _ => this.notificationsManager.ShowError("Failed to initialize the application."));
        }

        private void OnConnectionStringChanged(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                await this.NoteCategories.RefreshAsync();
                await this.CurrentShift.RefreshAsync();
            }).SafeAsyncCall(null, this.SetProcessing);
        }

        private void OnSettignsChanging(object sender, CancelEventArgs e)
        {
            // Forces the calendar control to recalculate highlighting
            this.OnPropertyChanged(nameof(this.DaysToHighlight));
        }

        private void WindowClosingHandler(object sender, CancelEventArgs e)
        {
            e.Cancel = !this.CanApplicationBeClosed();

            if (!e.Cancel)
            {
                this.SaveWindowPosition();
            }
        }

        private void MainWindowPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.CurrentShift):
                    this.AddNoteViewModel.Shift = this.CurrentShift;
                    this.CurrentShift.PropertyChanged += CurrentShiftPropertyChanged;
                    this.CurrentShift.RefreshCompleted += (s, a) => this.FetchDaysToHighlight(this.DisplayDate.Month, this.DisplayDate.Year);
                    this.SaveShiftCommand = new SaveShiftCommand(this, this.CurrentShift);
                    this.RefreshNotesCommand = new RefreshNotesCommand(this.CurrentShift, this.DialogHost);
                    break;
                case nameof(this.ListNotesViewModel):
                    this.AddNoteViewModel.ListNotesViewModel = this.ListNotesViewModel;
                    break;
                case nameof(this.NoteCategories):
                    this.AddNoteViewModel.NoteCategories = this.NoteCategories;
                    break;
                case nameof(DialogHost):
                    this.RefreshNotesCommand = new RefreshNotesCommand(this.CurrentShift, this.DialogHost);
                    break;
                case nameof(this.SelectedDate):
                    Settings.Default.SelectedDate = this.SelectedDate;
                    Settings.Default.Save();
                    if (this.CurrentShift != null)
                    {
                        this.CurrentShift.SelectedDate = this.SelectedDate;
                        this.CurrentShift.RefreshAsync().SafeAsyncCall(null, this.SetProcessing, _ => this.notificationsManager.ShowError("Failed to load notes for selected date."));
                    }
                    break;
                case nameof(this.DisplayDate):
                    this.FetchDaysToHighlight(this.DisplayDate.Month, this.DisplayDate.Year);
                    break;
                case nameof(this.TotalEffort):
                    if (this.DaysToHighlight != null && this.DaysToHighlight.ContainsKey(this.SelectedDate.Date))
                    {
                        this.DaysToHighlight[this.SelectedDate.Date] = this.TotalEffort;
                        this.OnPropertyChanged(nameof(this.DaysToHighlight));
                    }
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

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            if (!CanApplicationBeClosed())
            {
                return;
            }
            Application.Current.Shutdown();
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

        private bool CanApplicationBeClosed()
        {
            if (this.CurrentShift?.Changed == true)
            {
                return this.DialogHost.ShowDialog(
                    "Warning",
                    "There are unsaved changes, are you sure you want to close the application?",
                    Buttons.OkCancel) == Result.OK;
            }
            return true;
        }

        private void SaveWindowPosition()
        {
            Settings.Default.MainWindowMaximized = this.WindowState == WindowState.Maximized;

            if (this.WindowState != WindowState.Maximized)
            {
                Settings.Default.MainWindowHeight = this.Height;
                Settings.Default.MainWindowWidth = this.Width;
                Settings.Default.MainWindowTop = this.Top;
                Settings.Default.MainWindowLeft = this.Left;
            }
            else
            {
                Settings.Default.MainWindowHeight = RestoreBounds.Height;
                Settings.Default.MainWindowWidth = RestoreBounds.Width;
                Settings.Default.MainWindowTop = RestoreBounds.Top;
                Settings.Default.MainWindowLeft = RestoreBounds.Left;
            }

            Settings.Default.Save();
        }

        private void RestoreWindowPosition()
        {
            this.Height = Settings.Default.MainWindowHeight;
            this.Width = Settings.Default.MainWindowWidth;
            this.Top = Settings.Default.MainWindowTop;
            this.Left = Settings.Default.MainWindowLeft;
            this.WindowState = Settings.Default.MainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void FetchDaysToHighlight(int month, int year)
        {
            if (!Settings.Default.IsHighlightingEnabled || Settings.Default.DailyWorkLoad == 0)
            {
                return;
            }
            
            DataBaseFunctions.GetMonthlyTotalEffort(month, year)
                .SafeAsyncCall(result => this.DaysToHighlight = result);
        }
    }
}
