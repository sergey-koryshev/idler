namespace Idler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Idler.Helpers.DB;
    using Idler.Helpers.MVVM;
    using Idler.Models;

    /// <summary>
    /// Represents table "NoteCategories"
    /// </summary>
    public class NoteCategories : UpdatableObject
    {
        private const string tableName = "NoteCategories";
        private const string idFieldName = "Id";
        private const string nameFieldName = "Name";
        private const string hiddenFieldName = "Hidden";

        private ObservableCollection<NoteCategory> categories = new ObservableCollection<NoteCategory>();

        /// <summary>
        /// Gets list of categories
        /// </summary>
        public ObservableCollection<NoteCategory> Categories
        {
            get => this.categories;
            set
            {
                this.categories = value;
                this.OnPropertyChanged();
            }
        }

        public NoteCategories()
        {
            this.Categories.CollectionChanged += CategoriesCollectionChangedHandler;
        }

        private void CategoriesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Changed != true)
            {
                this.Changed = true;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (NoteCategory noteCategory in e.NewItems)
                    {
                        noteCategory.PropertyChanged += CategoryPropertyChangedHandler;
                    }
                    break;
            }
        }

        /// <summary>
        /// Pulls categories from DataBase
        /// </summary>
        protected override async Task RefreshInternalAsync()
        {
            var categories = await NoteCategories.GetCategories();
            this.Categories.Clear();

            foreach (NoteCategory category in categories)
            {
                this.Categories.Add(category);
            }
        }

        private void CategoryPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NoteCategory.Changed):
                    this.Changed = ((NoteCategory)sender).Changed;
                    break;
            }
        }

        /// <summary>
        /// Updates/adds categories in DataBase
        /// </summary>
        protected override async Task UpdateInternalAsync()
        {
            string query = null;

            foreach (NoteCategory category in this.Categories.Where(c => c.Changed == true))
            {
                try
                {
                    if (category.Id == null)
                    {
                        query = $@"
INSERT INTO {NoteCategories.tableName} ({NoteCategories.nameFieldName}, {NoteCategories.hiddenFieldName})
VALUES (?, ?);";

                        int? id = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteNonQueryAsync(
                            query,
                            new List<System.Data.OleDb.OleDbParameter>()
                            {
                                new System.Data.OleDb.OleDbParameter() { Value = category.Name },
                                new System.Data.OleDb.OleDbParameter() { Value = category.Hidden }
                            },
                            returnIdentity: true)
                        );

                        if (id == null)
                        {
                            new SqlException("New Category was not inserted");
                        }
                        else
                        {
                            category.Id = id;
                        }
                    }
                    else
                    {
                        query = $@"
UPDATE {NoteCategories.tableName}
SET
    {NoteCategories.nameFieldName} = ?,
    {NoteCategories.hiddenFieldName} = ?
WHERE
    {NoteCategories.idFieldName} = ?";

                        await Task.Run(async () => await DataBaseConnection.Instance.ExecuteNonQueryAsync(
                            query,
                            new List<System.Data.OleDb.OleDbParameter>()
                            {
                                new System.Data.OleDb.OleDbParameter() { Value = category.Name },
                                new System.Data.OleDb.OleDbParameter() { Value = category.Hidden },
                                new System.Data.OleDb.OleDbParameter() { Value = category.Id }
                            })
                        );
                    }

                    category.ChangeType = ListItemChangeType.None;
                }
                catch (Exception ex)
                {
                    throw (new SqlException($"Error has occurred while updating category '{category}': {ex.Message}", query, ex));
                }

                category.Changed = false;
            }

            int[] originalNoteCategories = (await NoteCategories.GetCategories()).Where(c => c.Id.HasValue).Select(c => c.Id.Value).ToArray();

            int[] diff = originalNoteCategories.Except(from category in this.Categories select (int)category.Id).ToArray();

            foreach (int id in diff)
            {
                await RemoveCategoryById(id);
            }
        }

        public static async Task<List<NoteCategory>> GetCategories()
        {
            string queryToGetCategories = $@"
SELECT *
FROM {NoteCategories.tableName}
ORDER BY {NoteCategories.nameFieldName}";

            return await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(queryToGetCategories, (r) => new NoteCategory(
                r.GetInt32(r.GetOrdinal(NoteCategories.idFieldName)), r.GetString(r.GetOrdinal(NoteCategories.nameFieldName)), r.GetBoolean(r.GetOrdinal(NoteCategories.hiddenFieldName)))));
        }

        public static async Task RemoveCategoryById(int id)
        {
            string query = $@"
DELETE FROM {NoteCategories.tableName} 
WHERE {NoteCategories.idFieldName} = ?";

            int? affectedRow = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteNonQueryAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = id }
                })
            );

            if ((int)affectedRow == 0)
            {
                Trace.TraceWarning($"There is no category with id '{id}'");
            }
        }
    }
}
