namespace Idler.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Threading;
    using Idler.Interfaces;
    using Idler.Models;
    using Idler.Properties;

    using ShiftNote = Idler.ShiftNote;

    public class ListNotesViewModel : BaseViewModel, IDragAndDrop
    {
        private GridLength effortClumnWidth;
        private GridLength categoryColumnWidth;
        private ObservableCollection<ShiftNote> notes;
        private ObservableCollection<NoteCategory> categories;
        private bool areNotesBlurred;
        private DispatcherTimer autoBlurTimer;
        private ICollectionView sortedNotes;

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

        public ICollectionView SortedNotes
        {
            get => sortedNotes;
            set
            {
                sortedNotes = value;
                this.OnPropertyChanged();
            }
        }

        private bool IsAutoBlurEnabled
        {
            get => Settings.Default.AutoBlurInterval.Ticks > 0 &&
                Settings.Default.IsAutoBlurEnabled;
        }

        public ListNotesViewModel(NoteCategories noteCategories, ObservableCollection<ShiftNote> notes)
        {
            this.Categories = noteCategories.Categories;
            noteCategories.RefreshCompleted += (s, e) =>
            {
                foreach (var item in this.Notes)
                {
                    item.RebindCategoryId();
                }
            };
            this.Notes = notes;

            this.CategoryColumnWidth = new GridLength(InternalSettings.Default.CategoryColumnWidth);
            this.EffortColumnWidth = new GridLength(InternalSettings.Default.EffortColumnWidth);
            Settings.Default.SettingsSaving += OnSettignsSaving;
            this.PropertyChanged += OnPropertyChangedHandler;
            this.InitializeAutoBlurReminer();

            var newView = new CollectionViewSource() { Source = notes, IsLiveSortingRequested = true };
            this.SortedNotes = newView.View;
            this.SortedNotes.SortDescriptions.Add(new SortDescription()
            {
                Direction = ListSortDirection.Ascending,
                PropertyName = nameof(ShiftNote.SortOrder)
            });
            this.SortedNotes.SortDescriptions.Add(new SortDescription()
            {
                Direction = ListSortDirection.Ascending,
                PropertyName = nameof(ShiftNote.Id)
            });

            this.AreNotesBlurred = this.IsAutoBlurEnabled || InternalSettings.Default.AreNotesBlurred;
        }

        private void OnPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(this.AreNotesBlurred):
                    this.ManageAutoBlurTimer();
                    InternalSettings.Default.AreNotesBlurred = this.AreNotesBlurred;
                    InternalSettings.Default.Save();
                    break;
            }
        }

        ~ListNotesViewModel()
        {
            InternalSettings.Default.CategoryColumnWidth = this.CategoryColumnWidth.Value;
            InternalSettings.Default.EffortColumnWidth = this.EffortColumnWidth.Value;
            InternalSettings.Default.Save();
        }

        private void OnSettignsSaving(object sender, CancelEventArgs e)
        {
            this.autoBlurTimer.Interval = Settings.Default.AutoBlurInterval;
            this.ManageAutoBlurTimer();
        }

        private void InitializeAutoBlurReminer()
        {
            this.autoBlurTimer = new DispatcherTimer();
            this.autoBlurTimer.Tick += OnAutoBlurTimerTick;
            this.autoBlurTimer.Interval = Settings.Default.AutoBlurInterval;
            this.ManageAutoBlurTimer();
        }

        private void OnAutoBlurTimerTick(object sender, EventArgs e)
        {
            this.AreNotesBlurred = true;
        }

        private void ManageAutoBlurTimer()
        {
            if (this.IsAutoBlurEnabled && !this.AreNotesBlurred)
            {
                this.autoBlurTimer.Start();
            }
            else
            {
                this.autoBlurTimer.Stop();
            }
        }

        public void ResetAutoBlurTimer()
        {
            this.autoBlurTimer.Stop();
            this.ManageAutoBlurTimer();
        }

        public void OnElementDropped(IDraggableItem dropped, IDraggableItem target)
        {
            if (target.DragOverPlaceholderPosition == DragOverPlaceholderPosition.None)
            {
                return;
            }

            if (this.Notes.GroupBy(n => n.SortOrder).Where(g => g.Count() > 1).Any())
            {
                this.FixSortOrder(this.Notes);
            }

            int orderDiff = dropped.SortOrder - target.SortOrder;

            if (orderDiff == 0)
            {
                return;
            }

            int droppedSortOrder = dropped.SortOrder;
            int targetSortOrder = orderDiff > 0
                ? target.DragOverPlaceholderPosition == DragOverPlaceholderPosition.Bottom ? target.SortOrder + 1 : target.SortOrder
                : target.DragOverPlaceholderPosition == DragOverPlaceholderPosition.Top ? target.SortOrder - 1 : target.SortOrder;
            int[] orderPair = new[] { droppedSortOrder, targetSortOrder };
            int minOrder = orderPair.Min();
            int maxOrder = orderPair.Max();

            foreach (var note in Notes)
            {
                if (note.SortOrder >= minOrder && note.SortOrder <= maxOrder)
                {
                    if (orderDiff > 0 && note.SortOrder == maxOrder)
                    {
                        note.SortOrder = minOrder;
                    }
                    else if (orderDiff < 0 && note.SortOrder == minOrder)
                    {
                        note.SortOrder = maxOrder;
                    }
                    else
                    {
                        note.SortOrder = orderDiff > 0 ? ++note.SortOrder : --note.SortOrder;
                    }
                }
            }

            this.SortedNotes?.Refresh();
        }

        private void FixSortOrder(ObservableCollection<ShiftNote> notes)
        {
            int sortOrder = 0;

            foreach (var item in notes.OrderBy(n => n.SortOrder))
            {
                item.SortOrder = sortOrder++;
            }
        }
    }
}
