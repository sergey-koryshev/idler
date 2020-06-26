using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    /// <summary>
    /// Represents table "NoteCategories"
    /// </summary>
    public class NoteCategories : MVVMHelper, IUpdatable
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
            this.RefreshAsync();
        }

        private void CategoriesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Changed != true)
            {
                this.Changed = true;
            }
        }

        /// <summary>
        /// Pulls categories from DataBase
        /// </summary>
        public override async Task RefreshAsync()
        {
            DataTable categoriesTable = await NoteCategories.GetCategories();

            this.Categories.Clear();

            foreach (DataRow category in categoriesTable.Rows)
            {
                try
                {
                    NoteCategory newCategory = new NoteCategory(
                        (int)category[NoteCategories.idFieldName],
                        (string)category[NoteCategories.nameFieldName],
                        (bool)category[NoteCategories.hiddenFieldName]
                    )
                    {
                        Changed = false
                    };

                    newCategory.PropertyChanged += CategoryPropertyChangedHandler;
                    this.Categories.Add(newCategory);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error has occurred while creating new NoteCategory object (Id: {category[NoteCategories.idFieldName].ToString()}, Name: {category[NoteCategories.nameFieldName].ToString()}, Hidden: {category[NoteCategories.hiddenFieldName].ToString()}): {ex}");
                }
            }
        }

        private void CategoryPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NoteCategory.Changed):
                    if (this.Changed != true)
                    {
                        this.Changed = ((NoteCategory)sender).Changed;
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates/adds categories in DataBase
        /// </summary>
        public override async Task UpdateAsync()
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
VALUES (
    '{category.Name}',
    {Convert.ToInt32(category.Hidden)}
);";

                        int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query, true));

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
    {NoteCategories.nameFieldName} = '{category.Name}',
    {NoteCategories.hiddenFieldName} = {Convert.ToInt32(category.Hidden)}
WHERE
    {NoteCategories.idFieldName} = {category.Id}";

                        await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query));
                    }
                }
                catch (Exception ex)
                {
                    throw (new SqlException($"Error has occurred while updating category '{category}': {ex.Message}", query, ex));
                }

                category.Changed = false;
            }

            int[] originalNoteCategories = (await NoteCategories.GetCategories()).AsEnumerable().Select(c => c.Field<int>(NoteCategories.idFieldName)).ToArray();

            int[] diff = originalNoteCategories.Except(from category in this.Categories select (int)category.Id).ToArray();

            foreach (int id in diff)
            {
                await RemoveCategoryById(id);
            }
        }

        public static async Task<DataTable> GetCategories()
        {
            string queryToGetCategories = $@"
SELECT *
FROM {NoteCategories.tableName}";

            DataTable categories = await Task.Run(async () => await DataBaseConnection.GetTableAsync(queryToGetCategories));

            return categories;
        }

        public static async Task RemoveCategoryById(int id)
        {
            string query = $@"
DELETE FROM {NoteCategories.tableName} 
WHERE Id = {id}";

            int? affectedRow = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query));

            if ((int)affectedRow == 0)
            {
                Trace.TraceWarning($"There is no category with id '{id}'");
            }
        }
    }
}
