using Idler.Commands;
using Idler.Helpers.DB;
using Idler.Properties;
using Idler.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

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
        private DateTime selectedDate;
        private ICommand saveShiftCommand;

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

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set { 
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

            InitializeCurrentShift();
        }

        private async Task InitializeCurrentShift()
        {
            if (Properties.Settings.Default.SelectedDate == null)
            {
                this.SelectedDate = DateTime.Now;
            }
            else
            {
                try
                {
                    Trace.TraceInformation($"Loading last interacted shift with id {Properties.Settings.Default.SelectedDate}");

                    //this.CurrentShift = new Shift() { Id = Properties.Settings.Default.SelectedDate };
                    //await this.CurrentShift.RefreshAsync();
                }
                catch (DataBaseRowNotFoundException ex)
                {
                    Trace.TraceInformation($"Creating new shift since last interacted shift with id {Properties.Settings.Default.SelectedDate} doesn't exist");

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
                        //if (Properties.Settings.Default.SelectedDate != (int)this.CurrentShift.Id)
                        //{
                        //    Properties.Settings.Default.SelectedDate = (int)this.CurrentShift.Id;
                        //    Properties.Settings.Default.Save();
                        //}
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
