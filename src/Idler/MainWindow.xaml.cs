using Idler.Commands;
using Idler.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Idler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string appName = "Idler";
        private string fullAppName;
        private NoteCategories noteCategories;
        private bool isBusy;
        private AddNoteViewModel addNoteViewModel;
        private DateTime selectedDate;
        private ICommand saveShiftCommand;
        private Shift currentShift;

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


            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.FullAppName = $"{appName} ({version})";

            InitializeComponent();

            this.PropertyChanged += MainWindowPropertyChangedHandler;

            this.NoteCategories = new NoteCategories();

            this.isBusy = true;

            InitialLoadingShiftNotes().ContinueWith((action) =>
            {
                this.isBusy = false;
                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred during initial loading notes: {action.Exception}");
                }
            });
        }

        private async Task InitialLoadingShiftNotes()
        {
            this.SelectedDate = Properties.Settings.Default.SelectedDate != default(DateTime) ? Properties.Settings.Default.SelectedDate : DateTime.Now;

            try
            {
                Trace.TraceInformation($"Loading notes for date {this.SelectedDate}");
                this.CurrentShift = new Shift() { SelectedDate = this.SelectedDate };
                await this.CurrentShift.RefreshAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private void MainWindowPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.CurrentShift):
                    this.AddNoteViewModel.Shift = this.CurrentShift;
                    this.CurrentShift.PropertyChanged += CurrentShiftPropertyChanged;
                    this.SaveShiftCommand = new SaveShiftCommand(this, this.CurrentShift);
                    break;
                case nameof(this.NoteCategories):
                    this.AddNoteViewModel.NoteCategories = this.NoteCategories.Categories;
                    break;
                case nameof(this.SelectedDate):
                    Properties.Settings.Default.SelectedDate = this.SelectedDate;
                    Properties.Settings.Default.Save();
                    if (this.CurrentShift != null)
                    {
                        this.CurrentShift.SelectedDate = this.SelectedDate;
                        this.isBusy = true;
                        this.CurrentShift.RefreshAsync().ContinueWith((task) =>
                        {
                            this.isBusy = false;
                            if (task.IsFaulted)
                            {
                                Trace.TraceError($"Error has been occurred during refreshing notes: {task.Exception}");
                            }
                        });
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

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await this.CurrentShift.RefreshAsync();
        }

        private void BtnNextShift_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedDate = this.selectedDate.AddDays(1);
        }

        private void BtnPreviousShift_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedDate = this.selectedDate.AddDays(-1);
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
