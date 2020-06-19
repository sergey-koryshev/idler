using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
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
        private DateTime startTime;
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
    Id = {this.Id}";

            DataRowCollection shiftNoteDetails = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(queryToGetShiftNoteDetails));

            if (shiftNoteDetails.Count == 0)
            {
                throw (new DataBaseRowNotFoundException($"There is no Shift Note with {this.Id}", queryToGetShiftNoteDetails));
            }
            else
            {
                this.shiftId = (int)shiftNoteDetails[0][ShiftNote.shiftIdFieldName];
                this.Effort = (decimal)shiftNoteDetails[0][ShiftNote.effortFiedlName];
                this.Description = (string)shiftNoteDetails[0][ShiftNote.descriptionFieldName];
                this.CategoryId = (int)shiftNoteDetails[0][ShiftNote.categoryIdFieldName];
                this.StartTime = (DateTime)shiftNoteDetails[0][ShiftNote.startTimeFieldName];
                if (shiftNoteDetails[0][ShiftNote.endTimeFieldName] is DBNull)
                {
                    this.EndTime = null;
                }
                else
                {
                    this.EndTime = (DateTime?)shiftNoteDetails[0][ShiftNote.endTimeFieldName];
                }
            }

            await base.RefreshAsync();

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
INSERT INTO {ShiftNote.tableName} ({ShiftNote.shiftIdFieldName},{ShiftNote.effortFiedlName}, {ShiftNote.descriptionFieldName}, {ShiftNote.categoryIdFieldName}, {ShiftNote.startTimeFieldName},  {ShiftNote.endTimeFieldName})
VALUES (
    {this.ShiftId},
    {this.Effort},
    '{this.Description}',
    {this.CategoryId},
    '{this.StartTime}',
    {endTimeString}
)";
                    int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query, true));

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
    {ShiftNote.shiftIdFieldName} = {this.ShiftId},
    {ShiftNote.effortFiedlName} = {this.Effort},
    {ShiftNote.descriptionFieldName} = '{this.Description}',
    {ShiftNote.categoryIdFieldName} = {this.CategoryId},
    {ShiftNote.startTimeFieldName} = '{this.StartTime}',
    {ShiftNote.endTimeFieldName} = {endTimeString}
WHERE
    {ShiftNote.idFieldName} = {this.Id}";

                    await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query));
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
WHERE {shiftIdFieldName} = {shiftId}
";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(queryToGetNotesByShiftId));

            var notesIds = from DataRow note in notes select note.Field<int>(ShiftNote.idFieldName);

            return notesIds.ToArray();
        }

        public override string ToString()
        {
            return $"Shift Note '{this.Description}' ({this.Effort})";
        }
    }
}
