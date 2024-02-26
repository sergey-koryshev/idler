using Idler.Interfaces;
using Idler.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Idler.ViewModels
{
    public class ListNotesViewModel : BaseCategoriesViewModel, IDragAndDrop
    {
        private GridLength effortClumnWidth;
        private GridLength categoryColumnWidth;
        private Shift shift;
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

        public Shift Shift
        {
            get => shift;
            set
            {
                shift = value;
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
            get => Properties.Settings.Default.AutoBlurInterval.Ticks > 0 &&
                Properties.Settings.Default.IsAutoBlurEnabled;
        }

        public ListNotesViewModel(NoteCategories categories, Shift shift) : base()
        {
            this.Shift = shift;
            this.Shift.RefreshCompleted += OnShiftUpdated;
            this.Shift.UpdateCompleted += OnShiftUpdated;

            this.Categories = categories;

            this.CategoryColumnWidth = new GridLength(Properties.Settings.Default.CategoryColumnWidth);
            this.EffortColumnWidth = new GridLength(Properties.Settings.Default.EffortColumnWidth);
            Properties.Settings.Default.SettingsSaving += OnSettignsSaving;
            this.PropertyChanged += OnPropertyChangedHandler;
            this.InitializeAutoBlurReminer();

            var newView = new CollectionViewSource() { Source = shift.Notes, IsLiveSortingRequested = true };
            this.SortedNotes = newView.View;
            this.SortedNotes.SortDescriptions.Add(new SortDescription()
            {
                 Direction = ListSortDirection.Ascending,
                 PropertyName = nameof(ShiftNote.SortOrder)
            });
        }

        private void OnPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(this.AreNotesBlurred):
                    this.ManageAutoBlurTimer();
                    break;
            }
        }

        ~ListNotesViewModel()
        {
            Properties.Settings.Default.CategoryColumnWidth = this.CategoryColumnWidth.Value;
            Properties.Settings.Default.EffortColumnWidth = this.EffortColumnWidth.Value;
            Properties.Settings.Default.Save();
        }

        private void OnSettignsSaving(object sender, CancelEventArgs e)
        {
            this.autoBlurTimer.Interval = Properties.Settings.Default.AutoBlurInterval;
            this.ManageAutoBlurTimer();
        }

        private void InitializeAutoBlurReminer()
        {
            this.autoBlurTimer = new DispatcherTimer();
            this.autoBlurTimer.Tick += OnAutoBlurTimerTick;
            this.autoBlurTimer.Interval = Properties.Settings.Default.AutoBlurInterval;
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

            if (this.Shift.Notes.GroupBy(n => n.SortOrder).Where(g => g.Count() > 1).Any())
            {
                this.FixSortOrder(this.Shift.Notes);
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

            foreach (var note in this.Shift.Notes)
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

        protected override void OnCategoriesFiltering(object sender, FilterEventArgs e)
        {
            var selectedCategories = this.Shift.Notes.Select(n => n.CategoryId).ToHashSet();
            var category = e.Item as NoteCategory;
            e.Accepted = !category.Hidden || selectedCategories.Contains((int)category.Id);

            foreach (var note in Shift.Notes)
            {
                note.RiseCategoryIdPropertyChange();
            }
        }

        private void FixSortOrder(ObservableCollection<ShiftNote> notes)
        {
            int sortOrder = 0;

            foreach (var item in notes.OrderBy(n => n.SortOrder))
            {
                item.SortOrder = sortOrder++;
            }
        }

        private void OnShiftUpdated(object sender, EventArgs e)
        {
            this.CategoriesViewSource.View.Refresh();

            foreach (var note in Shift.Notes)
            {
                note.RiseCategoryIdPropertyChange();
            }
        }
    }
}
