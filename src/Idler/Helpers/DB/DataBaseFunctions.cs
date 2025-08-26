using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Idler.Helpers.DB
{
    public static class DataBaseFunctions
    {
        private static readonly string shiftNote_tableName = "ShiftNotes";
        private static readonly string shiftNote_idFieldName = "Id";
        private static readonly string shiftNote_startTimeFieldName = "StartTime";
        private static readonly string shiftNote_effortFiedlName = "Effort";
        private static readonly string shiftNote_descriptionFieldName = "Description";
        private static readonly string shiftNote_categoryIdFieldName = "CategoryId";
        private static readonly string shiftNote_sortOrderFieldName = "SortOrder";

        private const string noteCategories_tableName = "NoteCategories";
        private const string noteCategories_idFieldName = "Id";
        private const string noteCategories_nameFieldName = "Name";
        private const string noteCategories_hiddenFieldName = "Hidden";

        private const string systemInfo_tableName = "SystemInfo";
        private const string systemInfo_idFieldName = "Id";
        private const string systemInfo_schemaVersionFieldName = "SchemaVersion";

        public static async Task<IEnumerable<Models.ShiftNote>> GetNotesByDates(DateTime from, DateTime to)
        {
            string query = $@"
SELECT 
    sn.{shiftNote_idFieldName}, 
    nc.{noteCategories_nameFieldName} AS CategoryName, 
    sn.{shiftNote_effortFiedlName},
    sn.{shiftNote_descriptionFieldName},
    sn.{shiftNote_startTimeFieldName}
FROM {shiftNote_tableName} sn INNER JOIN
    {noteCategories_tableName} nc
    ON sn.{shiftNote_categoryIdFieldName} = nc.{noteCategories_idFieldName}
WHERE {shiftNote_startTimeFieldName} BETWEEN DateValue(?) AND DateValue(?)
ORDER BY DateValue(sn.{shiftNote_startTimeFieldName}), sn.{shiftNote_sortOrderFieldName};";

            return await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(
                query,
                (r) =>
                {
                    return new Models.ShiftNote
                    {
                        Id = r.GetInt32(r.GetOrdinal(shiftNote_idFieldName)),
                        Category = r.GetString(r.GetOrdinal("CategoryName")),
                        Date = r.GetDateTime(r.GetOrdinal(shiftNote_startTimeFieldName)),
                        Effort = r.GetDecimal(r.GetOrdinal(shiftNote_effortFiedlName)),
                        Description = r.GetString(r.GetOrdinal(shiftNote_descriptionFieldName))
                    };
                },
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = from },
                    new System.Data.OleDb.OleDbParameter() { Value = to }
                })
            );
        }
    
        public static async Task<DateTime?> GetNextDate(DateTime currentDate)
        {
            string query = $@"
SELECT TOP 1 DateValue({shiftNote_startTimeFieldName}) AS {shiftNote_startTimeFieldName}
FROM {shiftNote_tableName}
WHERE DateValue({shiftNote_startTimeFieldName}) > DateValue(?)
ORDER BY {shiftNote_startTimeFieldName} ASC;";

            var nextDate = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(
                query,
                (r) => r.GetDateTime(r.GetOrdinal(shiftNote_startTimeFieldName)),
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = currentDate }
                })
            );

            return nextDate.Count() > 0 ? nextDate.First() as DateTime? : null;
        }

        public static async Task<DateTime?> GetPreviousDate(DateTime currentDate)
        {
            string query = $@"
SELECT TOP 1 DateValue({shiftNote_startTimeFieldName}) AS {shiftNote_startTimeFieldName}
FROM {shiftNote_tableName}
WHERE DateValue({shiftNote_startTimeFieldName}) < DateValue(?)
ORDER BY {shiftNote_startTimeFieldName} DESC;";

            var previousDate = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(
                query,
                (r) => r.GetDateTime(r.GetOrdinal(shiftNote_startTimeFieldName)),
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = currentDate }
                })
            );

            return previousDate.Count() > 0 ? previousDate.First() as DateTime? : null;
        }

        public static async Task<Dictionary<DateTime, decimal>> GetMonthlyTotalEffort(int month, int year)
        {
            string query = $@"
SELECT 
    DateValue(sn.{shiftNote_startTimeFieldName}) AS {shiftNote_startTimeFieldName},
    SUM(sn.{shiftNote_effortFiedlName}) AS TotalEffort
FROM {shiftNote_tableName} sn
WHERE MONTH(sn.{shiftNote_startTimeFieldName}) = ? 
    AND YEAR(sn.{shiftNote_startTimeFieldName}) = ?
GROUP BY DateValue(sn.{shiftNote_startTimeFieldName});";

           var result = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(
                query,
                (r) => new
                {
                    Date = r.GetDateTime(r.GetOrdinal(shiftNote_startTimeFieldName)),
                    TotalEffort = r.GetDecimal(r.GetOrdinal("TotalEffort"))
                },
                new List<System.Data.OleDb.OleDbParameter>
                {
                    new System.Data.OleDb.OleDbParameter() { Value = month },
                    new System.Data.OleDb.OleDbParameter() { Value = year }
                })
            );

            return result.ToDictionary(r => r.Date.Date, r => r.TotalEffort);
        }

        public static async Task<decimal> GetTotalEffortByDate(DateTime date)
        {
            string query = $@"
SELECT
    SUM(sn.{shiftNote_effortFiedlName}) AS TotalEffort
FROM {shiftNote_tableName} sn
WHERE DAY(sn.{shiftNote_startTimeFieldName}) = ?
    AND MONTH(sn.{shiftNote_startTimeFieldName}) = ? 
    AND YEAR(sn.{shiftNote_startTimeFieldName}) = ?
GROUP BY DateValue(sn.{shiftNote_startTimeFieldName});";

            var efforts = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(
                query,
                (r) => r.GetDecimal(0),
                new List<System.Data.OleDb.OleDbParameter>
                {
                    new System.Data.OleDb.OleDbParameter() { Value = date.Date.Day },
                    new System.Data.OleDb.OleDbParameter() { Value = date.Date.Month },
                    new System.Data.OleDb.OleDbParameter() { Value = date.Date.Year }
                })
            );

            return efforts.FirstOrDefault();
        }

        public static async Task<int> GetSchemaVersion()
        {
            string query = $@"
SELECT TOP 1 {systemInfo_schemaVersionFieldName}
FROM {systemInfo_tableName};";

            var result = await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(query, (r) => r.GetInt32(0), force: true));

            return result.Single();
        }

        public static async Task UpdateSchemaVersion(int version)
        {
            string query = $@"
UPDATE {systemInfo_tableName} SET {systemInfo_schemaVersionFieldName} = ? WHERE {systemInfo_idFieldName} = 1";

            await Task.Run(async () => await DataBaseConnection.Instance.ExecuteNonQueryAsync(query, new List<System.Data.OleDb.OleDbParameter>
            {
                new System.Data.OleDb.OleDbParameter() { Value = version }
            }, force: true));
        }

        /// <summary>
        /// Retrieves a collection of train data records from the database.
        /// </summary>
        /// <remarks>
        /// This method executes a database query to fetch train data, including category IDs and
        /// descriptions, for categories that are not marked as hidden. The results are ordered by the start time and
        /// sort order.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{T}"/>
        /// of <see cref="Models.TrainData"/> objects, where each object includes the category ID and description.
        /// </returns>
        public static async Task<IEnumerable<Models.TrainData>> GetTrainData()
        {
            string query = $@"
SELECT 
    sn.{shiftNote_categoryIdFieldName}, 
    sn.{shiftNote_descriptionFieldName}
FROM {shiftNote_tableName} sn INNER JOIN
    {noteCategories_tableName} nc
    ON sn.{shiftNote_categoryIdFieldName} = nc.{noteCategories_idFieldName}
WHERE nc.{noteCategories_hiddenFieldName} = FALSE
ORDER BY DateValue(sn.{shiftNote_startTimeFieldName}), sn.{shiftNote_sortOrderFieldName};";

            return await Task.Run(async () => await DataBaseConnection.Instance.ExecuteQueryAsync(query, (r) => new Models.TrainData
            {
                CategoryId = r.GetInt32(r.GetOrdinal(shiftNote_categoryIdFieldName)),
                Description = r.GetString(r.GetOrdinal(shiftNote_descriptionFieldName))
            }));
        }
    }
}
