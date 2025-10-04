namespace Idler.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Data;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Helpers;

    public class CategoriesListViewModel : BaseViewModel
    {
        private ObservableCollection<NoteCategory> categories;
        private ICommand removeCategoryCommand;
        private object itemToScroll;
        private ICollectionView categoriesView;
        private readonly CollectionViewSource categoriesViewSource;
        private bool isRefreshButtonVisible;
        private ICommand refreshViewCommand;
        private IComparer comparer;

        public ObservableCollection<NoteCategory> Categories
        {
            get => categories;
            set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveCategoryCommand
        {
            get => removeCategoryCommand;
            set
            {
                removeCategoryCommand = value;
                this.OnPropertyChanged();
            }
        }

        public object ItemToScroll
        {
            get => itemToScroll;
            set
            {
                itemToScroll = value;
                this.OnPropertyChanged();
            }
        }

        public ICollectionView CategoriesView
        {
            get => categoriesView;
            set
            {
                categoriesView = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsRefreshButtonVisible
        {
            get => isRefreshButtonVisible;
            set
            {
                isRefreshButtonVisible = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand RefreshViewCommand
        {
            get => refreshViewCommand;
            set
            {
                refreshViewCommand = value;
                this.OnPropertyChanged();
            }
        }


        public CategoriesListViewModel(ObservableCollection<NoteCategory> categories)
        {
            this.Categories = categories;
            this.RemoveCategoryCommand = new RemoveCategoryCommand(categories);

            this.categoriesViewSource = new CollectionViewSource() { Source = this.Categories };
            this.categoriesViewSource.SortDescriptions.Add(new SortDescription
            {
                Direction = ListSortDirection.Ascending,
                PropertyName = nameof(NoteCategory.Name)
            });
            this.comparer = new SortComparer(this.categoriesViewSource.SortDescriptions);

            this.categoriesViewSource.View.CollectionChanged += this.OnCategoriesViewCollectionChanged;
            this.categoriesViewSource.View.CurrentChanged += this.OnCategoriesViewCurrentChanged;
            this.IsRefreshButtonVisible = false;
            this.RefreshViewCommand = new RefreshViewCommand(this.categoriesViewSource.View);

            this.CategoriesView = this.categoriesViewSource.View;
        }

        private void OnCategoriesViewCurrentChanged(object sender, EventArgs e)
        {
            this.UpdateRefreshButtonVisibility();
        }

        private void OnCategoriesViewCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateRefreshButtonVisibility();
        }

        private void UpdateRefreshButtonVisibility()
        {
            this.IsRefreshButtonVisible = !this.IsViewSorted(this.CategoriesView);
        }

        private bool IsViewSorted(ICollectionView view)
        {            
            var enumerator = view.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return true;
            }

            object previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (comparer.Compare(previous, current) > 0)
                {
                    return false;
                }

                previous = current;
            }

            return true;
        }
    }
}
