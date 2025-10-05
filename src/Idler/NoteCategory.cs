namespace Idler
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Idler.Helpers.MVVM;
    using Idler.Interfaces;
    using Idler.Models;

    /// <summary>
    /// Represents a single row in table NoteCategories
    /// </summary>
    public class NoteCategory : ObservableObject, ISpellCheckable
    {
        private int? id;
        private string name;
        private bool hidden;
        private ListItemChangeType changeType;
        private int spellingErrorsCount;

        /// <summary>
        /// Gets/sets Id of category
        /// </summary>
        public int? Id
        {
            get => this.id;
            set
            {
                this.id = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets/sets name of category
        /// </summary>
        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Determines if the category is hidden
        /// </summary>
        public bool Hidden
        {
            get => hidden;
            set
            {
                this.hidden = value;
                OnPropertyChanged();
            }
        }

        public ListItemChangeType ChangeType
        {
            get => changeType;
            set
            {
                changeType = value;
                OnPropertyChanged();
            }
        }

        public int SpellingErrorsCount
        {
            get => spellingErrorsCount;
            set
            {
                spellingErrorsCount = value;
                OnPropertyChanged();
            }
        }

        protected override HashSet<string> MeaningfulProperties => new HashSet<string>
        {
            nameof(Name),
            nameof(Hidden)
        };

        public NoteCategory()
        {
            this.ChangeType = ListItemChangeType.Created;
            this.PropertyChanged += PropertyChangedHandler;
        }

        /// <summary>
        /// Creates category with Id
        /// </summary>
        /// <param name="id">id of category</param>
        /// <param name="name">name of category</param>
        /// <param name="hidden">determines if the category is hidden</param>
        public NoteCategory(int id, string name, bool hidden) : this()
        {
            this.Id = id;
            this.Name = name;
            this.Hidden = hidden;
            this.ChangeType = ListItemChangeType.None;
            this.Changed = false;
        }

        /// <summary>
        /// Creates category without Id
        /// </summary>
        /// <param name="name">name of category</param>
        /// <param name="hidden">determines if the category is hidden</param>
        public NoteCategory(string name, bool hidden) : this()
        {
            this.Id = null;
            this.Name = name;
            this.Hidden = hidden;
        }

        public override string ToString()
        {
            return $"Note Category '{this.Name}' (#{this.Id}, hidden: {this.Hidden})";
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (this.MeaningfulProperties.Contains(e.PropertyName) &&
                this.Id.HasValue &&
                this.ChangeType != ListItemChangeType.Modified)
            {
                this.ChangeType = ListItemChangeType.Modified;
            }
        }
    }
}
