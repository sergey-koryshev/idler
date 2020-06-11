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
            //TODO: implement logic to load last shift from DataBase or last shift user interacted
            this.CurrentShift = new Shift(1);
        }

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentShift.Refresh();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentShift.Update();
        }

        private void BtnNextShift_Click(object sender, RoutedEventArgs e)
        {
            if(this.CurrentShift.NextShiftId != null)
            {
                this.CurrentShift = new Shift((int)this.CurrentShift.NextShiftId);
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
