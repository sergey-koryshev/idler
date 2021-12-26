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
using System.Windows.Shapes;

namespace Idler
{
    /// <summary>
    /// Interaction logic for NewShiftNote.xaml
    /// </summary>
    public partial class NewShiftNote : Window, INotifyPropertyChanged
    {
        private ShiftNote newNote;
        private NoteCategories categories;

        public ShiftNote NewNote
        {
            get => this.newNote;
            set
            {
                this.newNote = value;
                OnPropertyChanged(nameof(this.NewNote));
            }
        }

        public NoteCategories Categories
        {
            get => this.categories;
            set
            {
                categories = value;
                OnPropertyChanged(nameof(this.Categories));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NewShiftNote()
        {
            InitializeComponent();
        }

        public NewShiftNote(NoteCategories categories) : this()
        {
            this.NewNote = new ShiftNote();
            this.newNote.StartTime = DateTime.Now;
            this.Categories = categories;
        }

        public void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BtnAddNote_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
