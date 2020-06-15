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

            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                try
                {
                    connection.Open();
                    connection.Close();
                }
                catch (OleDbException ex)
                {
                    switch (ex.Errors[0].SQLState)
                    {
                        case "3024":
                            DataBaseConnection.CreateEmptyDataBase();
                            break;
                        default:
                            throw;
                    }
                }
            }
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
    Id AUTOINCREMENT PRIMARY KEY,
	Name VARCHAR(255)
)";

            string initializeShiftNotesTableQuery = @"
CREATE TABLE ShiftNotes (
    Id AUTOINCREMENT PRIMARY KEY,
    ShiftId INT,
	Effort NUMERIC(3,2), 
	Description VARCHAR(255),
    CategoryId INT,
    StartTime DATETIME,
    EndTime DATETIME
)";

            string initializeNoteCategoriesTableQuery = @"
CREATE TABLE NoteCategories (
    Id AUTOINCREMENT PRIMARY KEY,
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
        public static int? ExecuteNonQuery(string query, bool returnIdentity = false)
        {
            int? result = null;

            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                connection.Open();

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    result = command.ExecuteNonQuery();

                    if (returnIdentity)
                    {
                        command.Parameters.Clear();
                        command.CommandText = "SELECT @@IDENTITY";
                        object indentity = command.ExecuteScalar();
                        if (indentity == null)
                        {
                            result = null;
                        }
                        else
                        {
                            result = (int)indentity;
                        }
                    }
                }

                connection.Close();
            }

            return result;
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

    /// <summary>
    /// Represents SQL-exception
    /// </summary>
    public class SqlException : Exception
    {
        /// <summary>
        /// Affected query
        /// </summary>
        public string Query { get; set; }

        public SqlException(string message) : base(message) { }

        public SqlException(string message, string query) : base(message)
        {
            this.Query = query;
        }

        public SqlException(string message, string query, Exception innerException) : base(message, innerException)
        {
            this.Query = query;
        }

        // TODO: implement overriding method ToString()
    }

    /// <summary>
    /// Occurs when DataRow is not found in DataBase 
    /// </summary>
    public class DataBaseRowNotFoundException : SqlException
    {
        public DataBaseRowNotFoundException(string message, string query) : base(message, query) { }
    }

}
