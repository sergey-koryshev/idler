using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler
{
    /// <summary>
    /// Represents a single row from table Shift
    /// </summary>
    public class Shift : VMMVHelper
    {
        private const string tableName = "Shift";
        private const string idFieldName = "Id";
        private const string nameFieldName = "Name";

        private int? id;
        private string name;

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
        /// Initializes the shift with Id
        /// </summary>
        /// <param name="id">Id of shift</param>
        public Shift(int id)
        {
            this.Id = id;
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
        }
    }
}
