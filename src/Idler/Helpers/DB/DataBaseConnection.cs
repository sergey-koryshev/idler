﻿namespace Idler.Helpers.DB
{
    using System;
    using System.Threading.Tasks;
    using System.Data.OleDb;
    using System.Data;
    using System.Diagnostics;
    using Idler.Properties;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.ComponentModel;
    using Idler.Extensions;

    /// <summary>
    /// Represents helper functions to work with Data Base
    /// </summary>
    public static class DataBaseConnection
    {
        private static OleDbConnectionStringBuilder connectionString;
        private static Task dataBaseInitialization;

        public static event EventHandler ConnectionStringChanged;

        public static bool DataBaseIsBusy = false;

        static DataBaseConnection()
        {
            Trace.TraceInformation("Initializing class 'DataBaseConnection'");
            dataBaseInitialization = Task.Run(async () => await InitializeDbConnection()).SafeAsyncCall(null, busy => DataBaseIsBusy = busy);

            Settings.Default.SettingsSaving += OnSettingsSaving;
        }

        private static void OnSettingsSaving(object sender, CancelEventArgs e)
        {
            if (Settings.Default.DataSource != connectionString.DataSource)
            {
                dataBaseInitialization = InitializeDbConnection().SafeAsyncCall(null, busy => DataBaseIsBusy = busy);
                ConnectionStringChanged?.Invoke(sender, new EventArgs());
            }
        }

        private static async Task InitializeDbConnection()
        {
            DataBaseConnection.connectionString = new OleDbConnectionStringBuilder()
            {
                Provider = "Microsoft.ACE.OLEDB.12.0",
                DataSource = Settings.Default.DataSource
            };

            bool createdDb = false;
            int currentSchemaVersion = 0;

            using (OleDbConnection connection = new OleDbConnection(connectionString.ToString()))
            {
                Trace.TraceInformation($"Checking if Data Base exists: {Properties.Settings.Default.DataSource}");
                
                try
                {
                    await connection.OpenAsync();
                    connection.Close();
                }
                catch (OleDbException ex)
                {
                    switch (ex.Errors[0].SQLState)
                    {
                        case "3024":
                            Trace.TraceInformation("Data Base doesn't exist, creating empty one");
                            new ADOX.Catalog().Create(connectionString.ToString());
                            createdDb = true;
                            break;
                        default:
                            throw;
                    }
                }

                try
                {
                    await connection.OpenAsync();

                    var tables = connection.GetSchema("TABLES");

                    if (createdDb || tables.Select("TABLE_NAME = 'ShiftNotes'").Length == 0)
                    {
                        currentSchemaVersion = 0;
                    }
                    else if (tables.Select("TABLE_NAME = 'SystemInfo'").Length == 0)
                    {
                        currentSchemaVersion = DataBaseMigrations.initialMigration;
                    }
                    else
                    {
                        currentSchemaVersion = await DataBaseFunctions.GetSchemaVersion();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            await new DataBaseMigrations().ApplyMigrations(currentSchemaVersion);
        }

        /// <summary>
        /// Executes non-query
        /// </summary>
        /// <param name="query">Text of query</param>
        /// <param name="returnIdentity">Determines if identity or count of affected rows will be returned</param>
        public static async Task<int?> ExecuteNonQueryAsync(string query, List<OleDbParameter> parameters = null, bool returnIdentity = false, bool force = false)
        {
            if (dataBaseInitialization != null && !dataBaseInitialization.IsCompleted && !force)
            {
                await dataBaseInitialization;
            }

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

        public static async Task<DataRowCollection> GetRowCollectionAsync(string query, List<OleDbParameter> parameters = null, bool force = false)
        {
            if (dataBaseInitialization != null && !dataBaseInitialization.IsCompleted && !force)
            {
                await dataBaseInitialization;
            }

            DataTable table = await DataBaseConnection.GetTableAsync(query, parameters, force);

            return table.Rows;
        }

        public static async Task<DataTable> GetTableAsync(string query, List<OleDbParameter> parameters = null, bool force = false)
        {
            if (dataBaseInitialization != null && !dataBaseInitialization.IsCompleted && !force)
            {
                await dataBaseInitialization;
            }

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
