using Idler.Commands;
using Idler.Helpers.DB;
using Idler.Helpers.MVVM;
using Idler.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Idler
{
    public class ShiftNote : UpdatableObject
    {
        private const string tableName = "ShiftNotes";
        private const string idFieldName = "Id";
        private const string effortFiedlName = "Effort";
        private const string descriptionFieldName = "Description";
        private const string categoryIdFieldName = "CategoryId";
        private const string startTimeFieldName = "StartTime";
        private const string endTimeFieldName = "EndTime";

        private int? id;
        private decimal effort;
        private string description;
        private int categoryId;
        private DateTime startTime = DateTime.Now;
        private ICommand removeNoteCommand;

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

        public ICommand RemoveNoteCommand { 
            get => removeNoteCommand;
            set { 
                removeNoteCommand = value;
                OnPropertyChanged();
            }
        }

        public ShiftNote(ObservableCollection<ShiftNote> notes) {
            this.RemoveNoteCommand = new RemoveNoteCommand(notes, this);
        }

        public override async Task RefreshAsync()
        {
            OnRefreshStarted();

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
            }

            OnRefreshCompleted();
        }

        public override async Task UpdateAsync()
        {
            OnUpdateStarted();

            string query = string.Empty;

            try
            {
                if (this.Id == null)
                {
                    query = $@"
INSERT INTO {ShiftNote.tableName} ({ShiftNote.effortFiedlName}, {ShiftNote.descriptionFieldName}, {ShiftNote.categoryIdFieldName}, {ShiftNote.startTimeFieldName}, {ShiftNote.endTimeFieldName})
VALUES (?, ?, ?, ?, NULL)";

                    int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                        query,
                        new List<System.Data.OleDb.OleDbParameter>()
                        {
                            new System.Data.OleDb.OleDbParameter() { Value = this.Effort },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Description },
                            new System.Data.OleDb.OleDbParameter() { Value = this.CategoryId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.StartTime, OleDbType = System.Data.OleDb.OleDbType.Date },
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
    {ShiftNote.endTimeFieldName} = NULL
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
                            new System.Data.OleDb.OleDbParameter() { Value = this.Id }
                        })
                    );
                }
            }
            catch (SqlException ex)
            {
                throw (new SqlException($"Error has occurred while updating Shift Note '{this}': {ex.Message}", query, ex));
            }

            OnUpdateCompleted();
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
    }
}
