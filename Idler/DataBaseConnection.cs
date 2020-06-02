using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using Idler.Properties;
using System.Data;

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
    Id AUTOINCREMENT(0,1) PRIMARY KEY,
	Name VARCHAR(255)
)";

            string initializeShiftNotesTableQuery = @"
CREATE TABLE ShiftNotes (
    Id AUTOINCREMENT(0,1) PRIMARY KEY,
    ShiftId INT,
	Effort NUMERIC(2,2), 
	Description VARCHAR(255),
    CategoryId INT,
    StartTime DATETIME,
    EndTime DATETIME
)";

            string initializeNoteCategoriesTableQuery = @"
CREATE TABLE NoteCategories (
    Id AUTOINCREMENT(0,1) PRIMARY KEY,
	Name VARCHAR(255),
	Hidden BIT
)";

            DataBaseConnection.ExecuteNonQuery(initializeShiftTableQuery);
            DataBaseConnection.ExecuteNonQuery(initializeShiftNotesTableQuery);
            DataBaseConnection.ExecuteNonQuery(initializeNoteCategoriesTableQuery);
        }

        /// <summary>
        /// Executes non-query
        /// </summary>
        /// <param name="query">Text of query</param>
        public static void ExecuteNonQuery(string query)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                connection.Open();

                new OleDbCommand(query, connection).ExecuteNonQuery();

                connection.Close();
            }
        }

        public static DataRowCollection GetRowCollection(string query)
        {
            DataTable table = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                connection.Open();

                OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                adapter.Fill(table);

                connection.Close();
            }

            return table.Rows;
        }
    }


}
