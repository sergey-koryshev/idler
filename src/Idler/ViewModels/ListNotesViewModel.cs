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
        private GridLength dateColumnWidth;
        private ObservableCollection<ShiftNote> notes;
        private ObservableCollection<NoteCategory> categories;

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

        public GridLength DateColumnWidth
        {
            get => dateColumnWidth;
            set
            {
                dateColumnWidth = value;
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

        public ListNotesViewModel(ObservableCollection<NoteCategory> categories, ObservableCollection<ShiftNote> notes)
        {
            this.Categories = categories;
            this.Notes = notes;

            this.CategoryColumnWidth = new GridLength(Properties.Settings.Default.CategoryColumnWidth);
            this.EffortColumnWidth = new GridLength(Properties.Settings.Default.EffortColumnWidth);
            this.DateColumnWidth = new GridLength(Properties.Settings.Default.DateColumnWidth);
        }

        ~ListNotesViewModel()
        {
            Properties.Settings.Default.CategoryColumnWidth = this.CategoryColumnWidth.Value; 
            Properties.Settings.Default.EffortColumnWidth = this.EffortColumnWidth.Value;
            Properties.Settings.Default.DateColumnWidth= this.DateColumnWidth.Value;
            Properties.Settings.Default.Save();
        }
    }
}
