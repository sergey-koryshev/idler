namespace Idler.Helpers.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Idler.Extensions;
    using Idler.Properties;

    /// <summary>
    /// Represents helper functions to work with Data Base
    /// </summary>
    public class DataBaseConnection
    {
        private static readonly Lazy<DataBaseConnection> instance = new Lazy<DataBaseConnection>(() => new DataBaseConnection());
        private OleDbConnectionStringBuilder connectionString;
        private volatile CancellationTokenSource dataBaseInitializationCts;
        private volatile Task dataBaseInitialization;

        /// <summary>
        /// Occurs when the connection string is changed.
        /// </summary>
        public event EventHandler ConnectionStringChanged;

        /// <summary>
        /// Indicates whether the DataBase is initializing or not.
        /// </summary>
        public bool IsDataBaseInitializing { get; private set; } = false;

        /// <summary>
        /// Gets the singleton instance of the <see cref="DataBaseConnection"/> class.
        /// </summary>
        public static DataBaseConnection Instance => instance.Value;

        private DataBaseConnection()
        {
            var cts = new CancellationTokenSource();
            Interlocked.Exchange(ref dataBaseInitializationCts, cts);
            Interlocked.Exchange(ref dataBaseInitialization, Task.Run(async () => await InitializeDbConnection(cts.Token)).SafeAsyncCall(null, busy => IsDataBaseInitializing = busy));
            Settings.Default.SettingsSaving += OnSettingsSaving;
        }

        private void OnSettingsSaving(object sender, CancelEventArgs e)
        {
            if (this.connectionString != null && !string.Equals(this.connectionString.DataSource.Trim(), Settings.Default.DataSource.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                CancellationTokenSource oldCts = Interlocked.Exchange(ref dataBaseInitializationCts, new CancellationTokenSource());
                oldCts?.Cancel();
                oldCts?.Dispose();

                Interlocked.Exchange(ref dataBaseInitialization, Task.Run(async () => await InitializeDbConnection(this.dataBaseInitializationCts.Token)).SafeAsyncCall(null, busy => IsDataBaseInitializing = busy));
                ConnectionStringChanged?.Invoke(sender, EventArgs.Empty);
            }
        }

        private async Task InitializeDbConnection(CancellationToken cancellationToken)
        {
            Trace.TraceInformation("Initializing DB connection");

            this.connectionString = new OleDbConnectionStringBuilder()
            {
                Provider = InternalSettings.Default.DataBaseProvider,
                DataSource = Settings.Default.DataSource
            };

            bool createdDb = false;
            int currentSchemaVersion = 0;

            using (OleDbConnection connection = new OleDbConnection(this.connectionString.ConnectionString))
            {
                Trace.TraceInformation($"Checking if Data Base exists: {Settings.Default.DataSource}");
                var catalog = new ADOX.Catalog();

                try
                {
                    await connection.OpenAsync(cancellationToken);
                    connection.Close();
                }
                catch (OleDbException ex)
                {
                    switch (ex.Errors[0].SQLState)
                    {
                        case "3024":
                            Trace.TraceInformation("Data Base doesn't exist, creating empty one");
                            catalog.Create(this.connectionString.ConnectionString);
                            createdDb = true;
                            break;
                        default:
                            throw;
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(catalog);
                }

                Trace.TraceInformation("Getting current schema version");
                await connection.OpenAsync(cancellationToken);
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

                Trace.TraceInformation($"Current schema version is '{currentSchemaVersion}'");
            }

            await new DataBaseMigrations().ApplyMigrations(currentSchemaVersion, cancellationToken);
        }

        /// <summary>
        /// Executes SQL statement which is not supposed to return array of rows.
        /// </summary>
        /// <param name="query">Text of query.</param>
        /// <param name="force">Skips checking whether the DB is initializing.</param>
        /// <returns>Identity of last affected row.</returns>
        public async Task<int?> ExecuteNonQueryAsync(string query, List<OleDbParameter> parameters = null, bool force = false)
        {
            if (this.dataBaseInitialization != null && !this.dataBaseInitialization.IsCompleted && !force)
            {
                await this.dataBaseInitialization;
            }

            using (OleDbConnection connection = new OleDbConnection(this.connectionString.ConnectionString))
            {
                await connection.OpenAsync();

                using (DbCommand command = new OleDbCommand(query, connection))
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

                    object result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                    if (result != null && result != DBNull.Value)
                    {
                        int lastInsertedId = Convert.ToInt32(result);

                        if (lastInsertedId > 0)
                        {
                            return lastInsertedId;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Executes SQL query.
        /// </summary>
        /// <param name="query">Text of query.</param>
        /// <param name="mapper">Mapper method to convert DB object to <typeparamref name="T"/>.</param>
        /// <param name="parameters">List of parameters to seed in the provided query.</param>
        /// <param name="force">Skips checking whether the DB is initializing.</param>
        /// <returns>List of objects.</returns>
        public async Task<List<T>> ExecuteQueryAsync<T>(string query, Func<DbDataReader, T> mapper, List<OleDbParameter> parameters = null, bool force = false)
        {
            if (this.dataBaseInitialization != null && !this.dataBaseInitialization.IsCompleted && !force)
            {
                await this.dataBaseInitialization;
            }

            using (var connection = new OleDbConnection(this.connectionString.ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new OleDbCommand(query, connection))
                {
                    if (parameters != null)
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
                    }

                    Trace.TraceInformation($"Executing query: {query}");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var results = new List<T>();

                        while (await reader.ReadAsync())
                        {
                            results.Add(mapper(reader));
                        }

                        return results;
                    }
                }
            }
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
