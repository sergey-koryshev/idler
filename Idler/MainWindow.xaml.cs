using Idler.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Idler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Shift currentShift;
        private NoteCategories noteCategories = new NoteCategories();

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

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            this.PropertyChanged += MainWindowPropertyChangedHandler;

            if (Settings.Default.LastInteractedShiftId == 0)
            {
                int? lastShiftId = Shift.GetLastShiftId();
                if (lastShiftId != null)
                {
                    this.CurrentShift = new Shift((int)lastShiftId);
                }
                else
                {
                    this.CurrentShift = new Shift(Shift.unnamedShiftPrevix);
                }
            }
            else
            {
                try
                {
                    this.CurrentShift = new Shift(Settings.Default.LastInteractedShiftId);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    this.CurrentShift = new Shift(Shift.unnamedShiftPrevix);
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
                        if (Settings.Default.LastInteractedShiftId != (int)this.CurrentShift.Id)
                        {
                            Settings.Default.LastInteractedShiftId = (int)this.CurrentShift.Id;
                            Settings.Default.Save();
                        }
                    }
                    break;
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentShift.Refresh();
            OnPropertyChanged(nameof(this.CurrentShift));
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentShift.Update();
            OnPropertyChanged(nameof(this.CurrentShift));
        }

        private void BtnNextShift_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentShift.NextShiftId != null)
            {
                this.CurrentShift = new Shift((int)this.CurrentShift.NextShiftId);
            }
            else
            {
                this.CurrentShift = new Shift(Shift.unnamedShiftPrevix) { PreviousShiftId = this.CurrentShift.Id };
            }
        }

        private void BtnPreviousShift_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentShift.PreviousShiftId != null)
            {
                this.CurrentShift = new Shift((int)this.CurrentShift.PreviousShiftId);
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
    }
}
