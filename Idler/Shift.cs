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
    public class Shift : VMMVHelper, IUpdatable
    {
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
                OnPropertyChanged(nameof(this.Id));
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
                OnPropertyChanged(nameof(this.Name));
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
                OnPropertyChanged(nameof(this.Notes));
            }
        }

        /// <summary>
        /// Gets id of previous shift
        /// </summary>
        public int? PreviousShiftId
        {
            get => this.previousShiftId;
            private set
            {
                this.previousShiftId = value;
                OnPropertyChanged(nameof(this.PreviousShiftId));
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
                OnPropertyChanged(nameof(this.NextShiftId));
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

        /// <summary>
        /// Initializes the shift with Id
        /// </summary>
        /// <param name="id">Id of shift</param>
        public Shift(int id)
        {
            this.Id = id;

            this.Refresh();

            foreach (int shiftNoteId in ShiftNote.GetNotesByShiftId((int)this.Id))
            {
                ShiftNote newNote = new ShiftNote(shiftNoteId);
                newNote.PropertyChanged += ShiftNotePropertyChangedHandler;
                this.Notes.Add(newNote);
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
            }
        }

        /// <summary>
        /// Initializes the shift without Id
        /// </summary>
        /// <param name="name">Name of shift</param>
        public Shift(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Retrieves values from DataBase
        /// </summary>
        public void Refresh()
        {
            string queryToGetshiftDetails = $@"
SELECT *
FROM {Shift.tableName}
WHERE ID = {this.Id}";

            DataRowCollection shiftDetails = DataBaseConnection.GetRowCollection(queryToGetshiftDetails);

            if (shiftDetails.Count == 0)
            {
                throw (new DataBaseRowNotFoundException($"There is no shift with {this.Id}", queryToGetshiftDetails));
            }
            else
            {
                this.Id = (int)shiftDetails[0][Shift.idFieldName];
                this.Name = (string)shiftDetails[0][Shift.nameFieldName];
            }

            foreach (IUpdatable note in this.Notes)
            {
                note.Refresh();
            }

            this.PreviousShiftId = this.GetPreviousShiftId();
            this.NextShiftId = this.GetNextShiftId();
        }

        /// <summary>
        /// Saves all changes in properties in DataBase
        /// </summary>
        public void Update()
        {
            if (this.Id == null)
            {
                string query = $@"
INSERT INTO {Shift.tableName} ({Shift.nameFieldName})
VALUES (
    '{this.Name}'
)";
                int? id = DataBaseConnection.ExecuteNonQuery(query, true);

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

                DataBaseConnection.ExecuteNonQuery(query);
            }

            foreach (IUpdatable note in this.Notes)
            {
                // TODO: only changed notes must be updated
                note.Update();
            }
        }

        /// <summary>
        /// Retrieves id of previous shift if it exists
        /// </summary>
        /// <returns>id or null</returns>
        public int? GetPreviousShiftId()
        {
            string queryToGetPreviousShift = $@"
SELECT TOP 1
    {Shift.idFieldName}
FROM {Shift.tableName}
WHERE {Shift.idFieldName} < {this.Id}
ORDER BY {Shift.idFieldName} DESC";

            DataRowCollection previousShift = DataBaseConnection.GetRowCollection(queryToGetPreviousShift);

            var previousShiftId = from DataRow shift in previousShift select shift.Field<int?>(Shift.idFieldName);

            return previousShiftId.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves id of next shift if it exists
        /// </summary>
        /// <returns>id or null</returns>
        public int? GetNextShiftId()
        {
            string queryToGetNextShift = $@"
SELECT TOP 1
    {Shift.idFieldName}
FROM {Shift.tableName}
WHERE {Shift.idFieldName} > {this.Id}";

            DataRowCollection nextShift = DataBaseConnection.GetRowCollection(queryToGetNextShift);

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
            shiftNote.ShiftId = (int)this.Id;
            this.Notes.Add(shiftNote);
            OnPropertyChanged(nameof(this.TotalEffort));
        }
    }
}
