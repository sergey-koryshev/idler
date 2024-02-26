namespace Idler.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Data;

    public abstract class BaseCategoriesViewModel : BaseViewModel
    {
        private NoteCategories categories;
        private CollectionViewSource categoriesViewSource;

        public CollectionViewSource CategoriesViewSource
        {
            get => categoriesViewSource;
            set
            {
                categoriesViewSource = value;
                this.OnPropertyChanged();
            }
        }

        public NoteCategories Categories
        {
            get { return this.categories; }
            set
            {
                this.categories = value;
                this.OnPropertyChanged();
            }
        }

        public BaseCategoriesViewModel() : base()
        {
            this.PropertyChanged += OnBaseCategoriesViewModelPropertyChanged;

        }

        protected abstract void OnCategoriesFiltering(object sender, FilterEventArgs e);

        private void OnBaseCategoriesViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.Categories):
                    InitializeCategoriesView(this.Categories.Categories);
                    this.Categories.UpdateCompleted += OnCategoriesChanged;
                    this.Categories.RefreshCompleted += OnCategoriesChanged;
                    break;
            }
        }

        private void OnCategoriesChanged(object sender, EventArgs e)
        {
            this.CategoriesViewSource.View.Refresh();
        }

        private void InitializeCategoriesView(ObservableCollection<NoteCategory> noteCategories)
        {
            this.CategoriesViewSource = new CollectionViewSource { Source = noteCategories };
            this.CategoriesViewSource.Filter += OnCategoriesFiltering;
        }
    }
}
