using Idler.Helpers.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    /// <summary>
    /// Represents a single row in table NoteCategories
    /// </summary>
    public class NoteCategory : ObservableObject
    {
        private int? id;
        private string name;
        private bool hidden;

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

        public NoteCategory() { }

        /// <summary>
        /// Creates category with Id
        /// </summary>
        /// <param name="id">id of category</param>
        /// <param name="name">name of category</param>
        /// <param name="hidden">determines if the category is hidden</param>
        public NoteCategory(int id, string name, bool hidden)
        {
            this.Id = id;
            this.Name = name;
            this.Hidden = hidden;
        }

        /// <summary>
        /// Creates category without Id
        /// </summary>
        /// <param name="name">name of category</param>
        /// <param name="hidden">determines if the category is hidden</param>
        public NoteCategory(string name, bool hidden)
        {
            this.Id = null;
            this.Name = name;
            this.Hidden = hidden;
        }

        public override string ToString()
        {
            return $"Note Category '{this.Name}' (#{this.Id}, hidden: {this.Hidden})";
        }
    }
}
