using Idler.Helpers.DB;
using Idler.Properties;
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
        private NoteCategories noteCategories = new NoteCategories();
        private bool isBusy;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Trace.TraceInformation("Initializing main window");

            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            this.FullAppName = $"{appName} ({appVersion.Major}.{appVersion.Minor})";

            InitializeComponent();

            this.PropertyChanged += MainWindowPropertyChangedHandler;

            InitializeCurrentShift();
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
                    }
                    break;
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

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            await this.CurrentShift.UpdateAsync();
            OnPropertyChanged(nameof(this.CurrentShift));
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

        private void BtnAddNewNote_Click(object sender, RoutedEventArgs e)
        {
            NewShiftNote newShiftNote = new NewShiftNote(this.NoteCategories);
            if (newShiftNote.ShowDialog() == true)
            {
                this.CurrentShift.AddNewShiftNote(newShiftNote.NewNote);
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
    }
}
