using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using Idler.Properties;

namespace Idler
{
    /// <summary>
    /// Represents helper functions to work with Data Base
    /// </summary>
    public static class DataBaseConnection
    {
        private static OleDbConnectionStringBuilder connectionString;

        static DataBaseConnection()
        {
            DataBaseConnection.connectionString = new OleDbConnectionStringBuilder()
            {
                Provider = Settings.Default.ProviderName,
                DataSource = Settings.Default.DataSource
            };
        }

        /// <summary>
        /// Creates new Data Base
        /// </summary>
        public static void CreateEmptyDataBase()
        {
            var dataBase = new ADOX.Catalog();
            dataBase.Create(connectionString.ToString());

            string initializeShiftTableQuery = @"
CREATE TABLE Shift (
    Id INT,
	Name VARCHAR(255),
	PRIMARY KEY (Id)
)";

            string initializeShiftNotesTableQuery = @"
CREATE TABLE ShiftNotes (
    Id INT,
    ShiftId INT,
	Effort NUMERIC(2,2), 
	Description VARCHAR(255),
    CategoryId INT,
    StartTime DATETIME,
    EndTime DATETIME,
	PRIMARY KEY (Id)
)";

            string initializeNoteCategoriesTableQuery = @"
CREATE TABLE NoteCategories (
    Id INT,
	Name VARCHAR(255),
	Hidden BIT,
	PRIMARY KEY (Id)
)";

            DataBaseConnection.ExecuteQuery(initializeShiftTableQuery);
            DataBaseConnection.ExecuteQuery(initializeShiftNotesTableQuery);
            DataBaseConnection.ExecuteQuery(initializeNoteCategoriesTableQuery);
        }

        /// <summary>
        /// Executes query
        /// </summary>
        /// <param name="query">Text of query</param>
        public static void ExecuteQuery(string query)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                connection.Open();

                new OleDbCommand(query, connection).ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
