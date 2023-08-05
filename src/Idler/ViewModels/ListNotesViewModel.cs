using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Idler.ViewModels
{
    public class ListNotesViewModel : BaseViewModel
    {
        private GridLength effortClumnWidth;
        private GridLength categoryColumnWidth;
        private ObservableCollection<ShiftNote> notes;
        private ObservableCollection<NoteCategory> categories;
        private bool areNotesBlurred;

        public GridLength CategoryColumnWidth
        {
            get => categoryColumnWidth;
            set
            {
                categoryColumnWidth = value;
                this.OnPropertyChanged();
            }
        }

        public GridLength EffortColumnWidth
        {
            get => effortClumnWidth;
            set
            {
                effortClumnWidth = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<ShiftNote> Notes
        {
            get => notes;
            set
            {
                notes = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<NoteCategory> Categories
        {
            get => categories;
            set
            {
                categories = value;
                this.OnPropertyChanged();
            }
        }


        public bool AreNotesBlurred
        {
            get => areNotesBlurred; 
            set { 
                areNotesBlurred = value;
                this.OnPropertyChanged();
            }
        }

        public ListNotesViewModel(ObservableCollection<NoteCategory> categories, ObservableCollection<ShiftNote> notes)
        {
            this.Categories = categories;
            this.Notes = notes;
            this.AreNotesBlurred = false;

            this.CategoryColumnWidth = new GridLength(Properties.Settings.Default.CategoryColumnWidth);
            this.EffortColumnWidth = new GridLength(Properties.Settings.Default.EffortColumnWidth);
        }

        ~ListNotesViewModel()
        {
            Properties.Settings.Default.CategoryColumnWidth = this.CategoryColumnWidth.Value;
            Properties.Settings.Default.EffortColumnWidth = this.EffortColumnWidth.Value;
            Properties.Settings.Default.Save();
        }
    }
}
