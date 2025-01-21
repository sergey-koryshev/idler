﻿namespace Idler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Idler.Commands;
    using Idler.Helpers.DB;
    using Idler.Helpers.MVVM;
    using Idler.Interfaces;
    using Idler.Models;

    public class ShiftNote : UpdatableObject, IDraggableItem
    {
        private const string tableName = "ShiftNotes";
        private const string idFieldName = "Id";
        private const string effortFiedlName = "Effort";
        private const string descriptionFieldName = "Description";
        private const string categoryIdFieldName = "CategoryId";
        private const string startTimeFieldName = "StartTime";
        private const string endTimeFieldName = "EndTime";
        private const string sortOrderFieldName = "SortOrder";

        private int? id;
        private decimal effort;
        private string description;
        private int categoryId;
        private DateTime startTime = DateTime.Now;
        private ICommand removeNoteCommand;
        private int sortOrder;
        private ListItemChangeType changeType;
        private DragOverPlaceholderPosition dragOverPlaceholderPosition;
        private int errorsCount;

        public int? Id
        {
            get => this.id;
            set
            {
                this.id = value;
                OnPropertyChanged();
            }
        }

        public decimal Effort
        {
            get => this.effort;
            set
            {
                this.effort = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => this.description;
            set
            {
                this.description = value;
                OnPropertyChanged();
            }
        }

        public int CategoryId
        {
            get => this.categoryId;
            set
            {
                this.categoryId = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartTime
        {
            get => this.startTime;
            set
            {
                this.startTime = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveNoteCommand
        { 
            get => removeNoteCommand;
            set { 
                removeNoteCommand = value;
                OnPropertyChanged();
            }
        }

        public int SortOrder
        {
            get => sortOrder;
            set
            {
                sortOrder = value;
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

        public DragOverPlaceholderPosition DragOverPlaceholderPosition
        { 
            get => dragOverPlaceholderPosition;
            set
            {
                dragOverPlaceholderPosition = value;
                OnPropertyChanged();
            }
        }

        public int ErrorsCount
        { 
            get => errorsCount;
            set
            {
                errorsCount = value;
                OnPropertyChanged();
            }
        }

        protected override HashSet<string> MeaningfulProperties => new HashSet<string>
        {
            nameof(CategoryId),
            nameof(Effort),
            nameof(Description),
            nameof(SortOrder)
        };

        public ShiftNote(ObservableCollection<ShiftNote> notes)
        {
            this.RemoveNoteCommand = new RemoveNoteCommand(notes, this);
            this.ChangeType = ListItemChangeType.None;
            this.PropertyChanged += OnNotePropertyChanged;
        }

        private void OnNotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ShiftNote.CategoryId):
                case nameof(ShiftNote.Effort):
                case nameof(ShiftNote.Description):
                    this.ChangeType = this.Id == null ? ListItemChangeType.Created : ListItemChangeType.Modified;
                    break;
                case nameof(ShiftNote.SortOrder):
                    if (this.ChangeType != ListItemChangeType.Created && this.ChangeType != ListItemChangeType.Modified)
                    {
                        this.ChangeType = ListItemChangeType.SortOrderChanged;
                    }

                    break;
            }
        }

        protected override async Task RefreshInternalAsync()
        {
            string queryToGetShiftNoteDetails = $@"
SELECT *
FROM {ShiftNote.tableName}
WHERE
    {ShiftNote.idFieldName} = ?";

            DataRowCollection shiftNoteDetails = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetShiftNoteDetails,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value =  this.Id }
                })
            );

            if (shiftNoteDetails.Count == 0)
            {
                throw (new DataBaseRowNotFoundException($"There is no Shift Note with {this.Id}", queryToGetShiftNoteDetails));
            }
            else
            {
                this.Effort = shiftNoteDetails[0].Field<decimal>(ShiftNote.effortFiedlName);
                this.Description = shiftNoteDetails[0].Field<string>(ShiftNote.descriptionFieldName);
                this.CategoryId = shiftNoteDetails[0].Field<int>(ShiftNote.categoryIdFieldName);
                this.StartTime = shiftNoteDetails[0].Field<DateTime>(ShiftNote.startTimeFieldName);
                this.SortOrder = shiftNoteDetails[0].Field<int>(ShiftNote.sortOrderFieldName);
            }

            this.ChangeType = ListItemChangeType.None;
        }

        protected override async Task UpdateInternalAsync()
        {
            string query = string.Empty;

            try
            {
                if (this.Id == null)
                {
                    query = $@"
INSERT INTO {ShiftNote.tableName} ({ShiftNote.effortFiedlName}, {ShiftNote.descriptionFieldName}, {ShiftNote.categoryIdFieldName}, {ShiftNote.startTimeFieldName}, {ShiftNote.endTimeFieldName}, {ShiftNote.sortOrderFieldName})
VALUES (?, ?, ?, ?, NULL, ?)";

                    int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                        query,
                        new List<System.Data.OleDb.OleDbParameter>()
                        {
                            new System.Data.OleDb.OleDbParameter() { Value = this.Effort },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Description },
                            new System.Data.OleDb.OleDbParameter() { Value = this.CategoryId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.StartTime, OleDbType = System.Data.OleDb.OleDbType.Date },
                            new System.Data.OleDb.OleDbParameter() { Value = this.SortOrder }
                        },
                        true)
                    );

                    if (id == null)
                    {
                        throw (new SqlException("New Category was not inserted", query));
                    }
                    else
                    {
                        this.Id = id;
                    }
                }
                else
                {
                    query = $@"
UPDATE {ShiftNote.tableName}
SET
    {ShiftNote.effortFiedlName} = ?,
    {ShiftNote.descriptionFieldName} = ?,
    {ShiftNote.categoryIdFieldName} = ?,
    {ShiftNote.startTimeFieldName} = ?,
    {ShiftNote.endTimeFieldName} = NULL,
    {ShiftNote.sortOrderFieldName} = ?
WHERE
    {ShiftNote.idFieldName} = ?";

                    int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                        query,
                        new List<System.Data.OleDb.OleDbParameter>()
                        {
                            new System.Data.OleDb.OleDbParameter() { Value = this.Effort },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Description },
                            new System.Data.OleDb.OleDbParameter() { Value = this.CategoryId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.StartTime, OleDbType = System.Data.OleDb.OleDbType.Date },
                            new System.Data.OleDb.OleDbParameter() { Value = this.SortOrder },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Id }
                        })
                    );
                }
            }
            catch (SqlException ex)
            {
                throw (new SqlException($"Error has occurred while updating Shift Note '{this}': {ex.Message}", query, ex));
            }

            this.ChangeType = ListItemChangeType.None;
        }

        public static async Task<int[]> GetNotesByDate(DateTime date)
        {
            string queryToGetNotesByShiftId = $@"
SELECT {ShiftNote.idFieldName}
FROM {ShiftNote.tableName}
WHERE (((Format([{ShiftNote.startTimeFieldName}],""mm/dd/yyyy""))=Format(?,""mm/dd/yyyy"")))";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetNotesByShiftId,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = date }
                })
            );

            var notesIds = from DataRow note in notes select note.Field<int>(ShiftNote.idFieldName);

            return notesIds.ToArray();
        }

        public static async Task RemoveShiftNoteByShiftNoteId(int shiftNoteId)
        {
            string query = $@"
DELETE FROM {ShiftNote.tableName}
WHERE {ShiftNote.idFieldName} = ?";

            int? affectedRow = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = shiftNoteId }
                })
            );

            if ((int)affectedRow == 0)
            {
                Trace.TraceWarning($"There is no shift note with id '{shiftNoteId}'");
            }
        }

        public override string ToString()
        {
            return $"Shift Note '{this.Description}' ({this.Effort})";
        }

        /// <summary>
        /// Fixes binding category Id in ComboBox when list of categories are refreshed
        /// since ComboBox doesn't do it by itself
        /// </summary>
        public void RebindCategoryId()
        {
            this.OnPropertyChanged(nameof(this.CategoryId), true);
        }
    }
}
