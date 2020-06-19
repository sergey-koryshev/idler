using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    /// <summary>
    /// Represents a single row from table Shift
    /// </summary>
    public class Shift : MVVMHelper, IUpdatable
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

        public Shift() { }

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
                    if(this.Changed != true)
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
WHERE ID = {this.Id}";

            
            DataRowCollection shiftDetails = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(queryToGetshiftDetails));

            if (shiftDetails.Count == 0)
            {
                throw (new DataBaseRowNotFoundException($"There is no shift with {this.Id}", queryToGetshiftDetails));
            }
            else
            {
                this.Id = (int)shiftDetails[0][Shift.idFieldName];
                this.Name = (string)shiftDetails[0][Shift.nameFieldName];
            }

            this.Notes.Clear();

            int[] shiftNoteIds = await Task.Run(async () => await ShiftNote.GetNotesByShiftId((int)this.Id));

            foreach (int shiftNoteId in shiftNoteIds)
            {
                ShiftNote newNote = new ShiftNote();
                newNote.PropertyChanged += ShiftNotePropertyChangedHandler;
                newNote.Id = shiftNoteId;
                this.Notes.Add(newNote);
                await newNote.RefreshAsync();
            }

            this.PreviousShiftId = await Task.Run(async () => await this.GetPreviousShiftId());
            this.NextShiftId = await Task.Run(async () =>await this.GetNextShiftId());

            await base.RefreshAsync();

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
VALUES (
    '{this.Name}'
)";
                int? id = await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query, true));

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
    {Shift.nameFieldName} = '{this.Name}'
WHERE
    {Shift.idFieldName} = {this.Id}";

                await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query));
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
WHERE {Shift.idFieldName} < {this.Id}
ORDER BY {Shift.idFieldName} DESC";

            DataRowCollection previousShift = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(queryToGetPreviousShift));

            var previousShiftId = from DataRow shift in previousShift select shift.Field<int?>(Shift.idFieldName);

            return previousShiftId.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves id of next shift if it exists
        /// </summary>
        /// <returns>id or null</returns>
        public async  Task<int?> GetNextShiftId()
        {
            string queryToGetNextShift = $@"
SELECT TOP 1
    {Shift.idFieldName}
FROM {Shift.tableName}
WHERE {Shift.idFieldName} > {this.Id}";

            DataRowCollection nextShift = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(queryToGetNextShift));

            var nextShiftId = from DataRow shift in nextShift select shift.Field<int?>(Shift.idFieldName);

            return nextShiftId.FirstOrDefault();
        }

        /// <summary>
        /// Adds new Shift Note to list
        /// </summary>
        /// <param name="shiftNote"></param>
        public void AddNewShiftNote(ShiftNote shiftNote)
        {
            shiftNote.PropertyChanged += ShiftNotePropertyChangedHandler;
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
    }
}
