using Idler.Helpers.DB;
using Idler.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    public class ShiftNote : MVVMHelper, IUpdatable
    {
        private const string tableName = "ShiftNotes";
        private const string idFieldName = "Id";
        private const string shiftIdFieldName = "ShiftId";
        private const string effortFiedlName = "Effort";
        private const string descriptionFieldName = "Description";
        private const string categoryIdFieldName = "CategoryId";
        private const string startTimeFieldName = "StartTime";
        private const string endTimeFieldName = "EndTime";

        private int? id;
        private int shiftId;
        private decimal effort;
        private string description;
        private int categoryId;
        private DateTime startTime = DateTime.Now;
        private DateTime? endTime;

        public int? Id
        {
            get => this.id;
            set
            {
                this.id = value;
                OnPropertyChanged();
            }
        }

        public int ShiftId
        {
            get => this.shiftId;
            set
            {
                this.shiftId = value;
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

        public DateTime? EndTime
        {
            get => this.endTime;
            set
            {
                this.endTime = value;
                OnPropertyChanged();
            }
        }

        public ShiftNote() { }

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
                this.shiftId = shiftNoteDetails[0].Field<int>(ShiftNote.shiftIdFieldName);
                this.Effort = shiftNoteDetails[0].Field<decimal>(ShiftNote.effortFiedlName);
                this.Description = shiftNoteDetails[0].Field<string>(ShiftNote.descriptionFieldName);
                this.CategoryId = shiftNoteDetails[0].Field<int>(ShiftNote.categoryIdFieldName);
                this.StartTime = shiftNoteDetails[0].Field<DateTime>(ShiftNote.startTimeFieldName);
                this.EndTime = shiftNoteDetails[0].Field<DateTime?>(ShiftNote.endTimeFieldName);
            }

            OnRefreshCompleted();
        }

        public override async Task UpdateAsync()
        {
            OnUpdateStarted();

            string query = string.Empty;

            string endTimeString = this.EndTime == null ? "NULL" : $"'{this.EndTime.ToString()}'";

            try
            {
                if (this.Id == null)
                {
                    query = $@"
INSERT INTO {ShiftNote.tableName} ({ShiftNote.shiftIdFieldName}, {ShiftNote.effortFiedlName}, {ShiftNote.descriptionFieldName}, {ShiftNote.categoryIdFieldName}, {ShiftNote.startTimeFieldName}, {ShiftNote.endTimeFieldName})
VALUES (?, ?, ?, ?, ?, ?)";

                    int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                        query,
                        new List<System.Data.OleDb.OleDbParameter>()
                        {
                            new System.Data.OleDb.OleDbParameter() { Value = this.ShiftId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Effort },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Description },
                            new System.Data.OleDb.OleDbParameter() { Value = this.CategoryId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.StartTime, OleDbType = System.Data.OleDb.OleDbType.Date },
                            new System.Data.OleDb.OleDbParameter() { Value = this.EndTime, OleDbType = System.Data.OleDb.OleDbType.Date }
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
    {ShiftNote.shiftIdFieldName} = ?,
    {ShiftNote.effortFiedlName} = ?,
    {ShiftNote.descriptionFieldName} = ?,
    {ShiftNote.categoryIdFieldName} = ?,
    {ShiftNote.startTimeFieldName} = ?,
    {ShiftNote.endTimeFieldName} = ?
WHERE
    {ShiftNote.idFieldName} = ?";

                    int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                        query,
                        new List<System.Data.OleDb.OleDbParameter>()
                        {
                            new System.Data.OleDb.OleDbParameter() { Value = this.ShiftId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Effort },
                            new System.Data.OleDb.OleDbParameter() { Value = this.Description },
                            new System.Data.OleDb.OleDbParameter() { Value = this.CategoryId },
                            new System.Data.OleDb.OleDbParameter() { Value = this.StartTime, OleDbType = System.Data.OleDb.OleDbType.Date },
                            new System.Data.OleDb.OleDbParameter() { Value = this.EndTime, OleDbType = System.Data.OleDb.OleDbType.Date },
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

        public static async Task<int[]> GetNotesByShiftId(int shiftId)
        {
            string queryToGetNotesByShiftId = $@"
SELECT {ShiftNote.idFieldName}
FROM {ShiftNote.tableName}
WHERE {ShiftNote.shiftIdFieldName} = ?";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetNotesByShiftId,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = shiftId }
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

        public static async Task RemoveShiftNotesByShiftId(int shiftId)
        {
            string query = $@"
DELETE FROM {ShiftNote.tableName}
WHERE {ShiftNote.shiftIdFieldName} = ?";

            int? affectedRow = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = shiftId }
                })
            );

            if ((int)affectedRow == 0)
            {
                Trace.TraceWarning($"There are no notes for shift with id '{shiftId}'");
            }
        }

        public override string ToString()
        {
            return $"Shift Note '{this.Description}' ({this.Effort})";
        }
    }
}
