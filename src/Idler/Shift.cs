using Idler.Helpers.DB;
using Idler.Helpers.MVVM;
using Idler.Interfaces;
using System;
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
    /// Represents a single row from table Shift
    /// </summary>
    public class Shift : UpdatableObject
    {
        public const string unnamedShiftPrevix = "Untitled shift";
        private const string tableName = "Shift";
        private const string idFieldName = "Id";
        private const string nameFieldName = "Name";

        private int? id;
        private string name;
        private ObservableCollection<ShiftNote> notes = new ObservableCollection<ShiftNote>();
        private int? previousShiftId;
        private int? nextShiftId;

        /// <summary>
        /// Gets/sets Id of shift
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
        /// Gets/sets name of shift
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
        /// Gets/sets collection of Shift Notes
        /// </summary>
        public ObservableCollection<ShiftNote> Notes
        {
            get => this.notes;
            set
            {
                this.notes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets id of previous shift
        /// </summary>
        public int? PreviousShiftId
        {
            get => this.previousShiftId;
            set
            {
                this.previousShiftId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets id of next shift
        /// </summary>
        public int? NextShiftId
        {
            get => this.nextShiftId;
            private set
            {
                this.nextShiftId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets total effort
        /// </summary>
        public decimal TotalEffort
        {
            get
            {
                return Notes.Sum(x => x.Effort);
            }
        }

        public Shift()
        {
            this.Notes.CollectionChanged += NotesCollectionChangedHandler;
        }

        private void NotesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Changed != true)
            {
                this.Changed = true;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ShiftNote newShiftNote in e.NewItems)
                    {
                        newShiftNote.PropertyChanged += ShiftNotePropertyChangedHandler;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handler for event "PropertyChanged" of class ShiftNote
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShiftNotePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ShiftNote.Effort):
                    OnPropertyChanged(nameof(this.TotalEffort));
                    break;
                case nameof(ShiftNote.Changed):
                    if (this.Changed != true)
                    {
                        this.Changed = ((ShiftNote)sender).Changed;
                    }
                    break;
            }
        }

        /// <summary>
        /// Retrieves values from DataBase
        /// </summary>
        public override async Task RefreshAsync()
        {
            this.OnRefreshStarted();

            string queryToGetshiftDetails = $@"
SELECT *
FROM {Shift.tableName}
WHERE {Shift.idFieldName} = ?";


            DataRowCollection shiftDetails = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetshiftDetails,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = this.Id }
                })
            );

            if (shiftDetails.Count == 0)
            {
                throw (new DataBaseRowNotFoundException($"There is no shift with {this.Id}", queryToGetshiftDetails));
            }
            else
            {
                this.Id = shiftDetails[0].Field<int>(Shift.idFieldName);
                this.Name = shiftDetails[0].Field<string>(Shift.nameFieldName);
            }

            this.Notes.Clear();

            int[] shiftNoteIds = await Task.Run(async () => await ShiftNote.GetNotesByShiftId((int)this.Id));

            foreach (int shiftNoteId in shiftNoteIds)
            {
                ShiftNote newNote = new ShiftNote();
                this.Notes.Add(newNote);
                newNote.Id = shiftNoteId;
                await newNote.RefreshAsync();
            }

            this.PreviousShiftId = await this.GetPreviousShiftId();
            this.NextShiftId = await this.GetNextShiftId();

            this.OnRefreshCompleted();
        }

        /// <summary>
        /// Saves all changes in properties in DataBase
        /// </summary>
        public override async Task UpdateAsync()
        {
            this.OnUpdateStarted();

            if (this.Id == null)
            {
                string query = $@"
INSERT INTO {Shift.tableName} ({Shift.nameFieldName})
VALUES ( ? )";
                int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                    query,
                    new List<System.Data.OleDb.OleDbParameter>()
                    {
                        new System.Data.OleDb.OleDbParameter() { Value = this.Name }
                    },
                    true));

                if (id == null)
                {
                    throw (new SqlException("New shift was not inserted to the table", query));
                }
                else
                {
                    this.Id = id;
                }
            }
            else
            {
                string query = $@"
UPDATE {Shift.tableName}
SET
    {Shift.nameFieldName} = ?
WHERE
    {Shift.idFieldName} = ?";

                await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                    query,
                    new List<System.Data.OleDb.OleDbParameter>()
                    {
                        new System.Data.OleDb.OleDbParameter() { Value = this.Name },
                        new System.Data.OleDb.OleDbParameter() { Value = this.Id }
                    })
                );
            }

            foreach (ShiftNote shiftNote in this.Notes)
            {
                if (shiftNote.ShiftId != (int)this.Id)
                {
                    shiftNote.ShiftId = (int)this.Id;
                }

                if (shiftNote.Changed == true)
                {
                    await shiftNote.UpdateAsync();
                }
            }

            int[] originShiftIDs = await ShiftNote.GetNotesByShiftId((int)this.Id);

            int[] diff = originShiftIDs.Except(from shiftNote in this.Notes select (int)shiftNote.Id).ToArray();

            foreach (int shiftNoteIdToDelete in diff)
            {
                await ShiftNote.RemoveShiftNoteByShiftNoteId(shiftNoteIdToDelete);
            }

            OnUpdateCompleted();
        }

        /// <summary>
        /// Retrieves id of previous shift if it exists
        /// </summary>
        /// <returns>id or null</returns>
        public async Task<int?> GetPreviousShiftId()
        {
            string queryToGetPreviousShift = $@"
SELECT TOP 1
    {Shift.idFieldName}
FROM {Shift.tableName}
WHERE {Shift.idFieldName} < ?
ORDER BY {Shift.idFieldName} DESC";

            DataRowCollection previousShift = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetPreviousShift,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = this.Id }
                })
            );

            var previousShiftId = from DataRow shift in previousShift select shift.Field<int?>(Shift.idFieldName);

            return previousShiftId.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves id of next shift if it exists
        /// </summary>
        /// <returns>id or null</returns>
        public async Task<int?> GetNextShiftId()
        {
            string queryToGetNextShift = $@"
SELECT TOP 1
    {Shift.idFieldName}
FROM {Shift.tableName}
WHERE {Shift.idFieldName} > ?
ORDER BY {Shift.idFieldName}";

            DataRowCollection nextShift = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetNextShift,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = this.Id }
                })
            );

            var nextShiftId = from DataRow shift in nextShift select shift.Field<int?>(Shift.idFieldName);

            return nextShiftId.FirstOrDefault();
        }

        /// <summary>
        /// Adds new Shift Note to list
        /// </summary>
        /// <param name="shiftNote"></param>
        public void AddNewShiftNote(ShiftNote shiftNote)
        {
            this.Notes.Add(shiftNote);
            OnPropertyChanged(nameof(this.TotalEffort));
        }

        public static async Task<int?> GetLastShiftId()
        {
            string queryToGetLastShift = $@"
SELECT TOP 1
    {Shift.idFieldName}
FROM {Shift.tableName}
ORDER BY {Shift.idFieldName} DESC";

            DataRowCollection lastShift = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(queryToGetLastShift));

            var lastShiftId = from DataRow shift in lastShift select shift.Field<int?>(Shift.idFieldName);

            return lastShiftId.FirstOrDefault();
        }

        public static async Task RemoveShiftByShiftId(int shiftId)
        {
            string queryToDeleteShift = $@"
DELETE FROM {Shift.tableName} 
WHERE Id = ?";

            int? affectedRow = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(
                queryToDeleteShift,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = shiftId }
                })
            );

            if ((int)affectedRow == 0)
            {
                Trace.TraceWarning($"There is no shift with id '{shiftId}'");
            }
        }
    }
}
