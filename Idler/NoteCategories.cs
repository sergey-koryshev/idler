﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class NoteCategories : VMMVHelper
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
            private set
            {
                this.categories = value;
                this.OnPropertyChanged(nameof(this.Categories));
            }
        }

        public NoteCategories()
        {
            this.Refresh();
        }

        /// <summary>
        /// Pulls categories from DataBase
        /// </summary>
        public void Refresh()
        {
            string queryToGetCategories = $@"
SELECT *
FROM {NoteCategories.tableName}";

            DataRowCollection categories = DataBaseConnection.GetRowCollection(queryToGetCategories);

            this.Categories.Clear();

            foreach (DataRow category in categories)
            {
                try
                {
                    this.Categories.Add(new NoteCategory(
                        int.Parse(category[NoteCategories.idFieldName].ToString()),
                        category[NoteCategories.nameFieldName].ToString(),
                        bool.Parse(category[NoteCategories.hiddenFieldName].ToString())
                        ));
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error has occurred while creating new NoteCategory object (Id: {category[NoteCategories.idFieldName].ToString()}, Name: {category[NoteCategories.nameFieldName].ToString()}, Hidden: {category[NoteCategories.hiddenFieldName].ToString()}): {ex}");
                }
            }
        }

        /// <summary>
        /// Updates/adds categories in DataBase
        /// </summary>
        public void Update()
        {
            string query = null;

            foreach (NoteCategory category in this.Categories)
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
)";

                        DataBaseConnection.ExecuteNonQuery(query);
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
                        DataBaseConnection.ExecuteNonQuery(query);
                    }
                }
                catch (Exception ex)
                {
                    throw (new Exception($"Error has occurred while updating category '{category}': {ex}"));
                }
            }
        }
    }
}
