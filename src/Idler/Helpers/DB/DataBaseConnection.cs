using System;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Diagnostics;
using System.Configuration;
using Idler.Properties;
using System.Collections.Generic;
using Idler.Interfaces;
using System.Data.Common;

namespace Idler.Helpers.DB
{
    /// <summary>
    /// Represents helper functions to work with Data Base
    /// </summary>
    public static class DataBaseConnection
    {
        private static OleDbConnectionStringBuilder connectionString;

        // TODO: need to identify the database has been created
        public static Task createDataBaseTask;

        static DataBaseConnection()
        {
            Trace.TraceInformation("Initializing class 'DataBaseConnection'");

            DataBaseConnection.connectionString = new OleDbConnectionStringBuilder()
            {
                Provider = "Microsoft.ACE.OLEDB.12.0",
                DataSource = Settings.Default.DataSource
            };

            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                Trace.TraceInformation($"Checking if Data Base exists: {Properties.Settings.Default.DataSource}");
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
                            Trace.TraceInformation("Data Base doesn't exist, creating empty one");
                            DataBaseConnection.createDataBaseTask =  DataBaseConnection.CreateEmptyDataBase();
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
        public static async Task CreateEmptyDataBase()
        {
            var dataBase = new ADOX.Catalog();
            dataBase.Create(connectionString.ToString());

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

            await DataBaseConnection.ExecuteNonQueryAsync(initializeShiftNotesTableQuery).ConfigureAwait(false);
            await DataBaseConnection.ExecuteNonQueryAsync(initializeNoteCategoriesTableQuery).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes non-query
        /// </summary>
        /// <param name="query">Text of query</param>
        /// <param name="returnIdentity">Determines if identity or count of affected rows will be returned</param>
        public static async Task<int?> ExecuteNonQueryAsync(string query, List<OleDbParameter> parameters = null, bool returnIdentity = false)
        {
            int? result = null;

            Trace.TraceInformation($"Connection string: {DataBaseConnection.connectionString}");

            using (OleDbConnection connection = new OleDbConnection(DataBaseConnection.connectionString.ToString()))
            {
                await connection.OpenAsync();

                using (DbCommand command = new OleDbCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            if(parameter.Value == null)
                            {
                                parameter.Value = DBNull.Value;
                            }

                            command.Parameters.Add(parameter);
                        }
                    }

                    Trace.TraceInformation($"Executing query: {query}");

                    result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                    if (returnIdentity)
                    {
                        command.Parameters.Clear();
                        command.CommandText = "SELECT @@IDENTITY";
                        object indentity = await command.ExecuteScalarAsync().ConfigureAwait(false);
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

        public static async Task<DataRowCollection> GetRowCollectionAsync(string query, List<OleDbParameter> parameters = null)
        {
            DataTable table = await DataBaseConnection.GetTableAsync(query, parameters);

            return table.Rows;
        }

        public static async Task<DataTable> GetTableAsync(string query, List<OleDbParameter> parameters = null)
        {
            DataTable table = new DataTable();

            Trace.TraceInformation($"Connection string: {DataBaseConnection.connectionString}");

            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            if (parameter.Value == null)
                            {
                                parameter.Value = DBNull.Value;
                            }

                            command.Parameters.Add(parameter);
                        }
                    }

                    Trace.TraceInformation($"Executing query: {query}");

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
                connection.Close();
            }

            return table;
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
