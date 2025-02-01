using System;
using System.Collections.Generic;
using System.Data;
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

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = from },
                    new System.Data.OleDb.OleDbParameter() { Value = to }
                })
            );

            return from DataRow note in notes select new Models.ShiftNote
            {
                Id = note.Field<int>(shiftNote_idFieldName),
                Category = note.Field<string>("CategoryName"),
                Date = note.Field<DateTime>(shiftNote_startTimeFieldName),
                Effort = note.Field<decimal>(shiftNote_effortFiedlName),
                Description = note.Field<string>(shiftNote_descriptionFieldName),
            };
        }
    
        public static async Task<DateTime?> GetNextDate(DateTime currentDate)
        {
            string query = $@"
SELECT TOP 1 DateValue({shiftNote_startTimeFieldName}) AS {shiftNote_startTimeFieldName}
FROM {shiftNote_tableName}
WHERE DateValue({shiftNote_startTimeFieldName}) > DateValue(?)
ORDER BY {shiftNote_startTimeFieldName} ASC;";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = currentDate }
                })
            );

            var nextDate = from DataRow note in notes
                           select note.Field<DateTime>(shiftNote_startTimeFieldName);

            return nextDate.Count() > 0 ? nextDate.First() as DateTime? : null;
        }

        public static async Task<DateTime?> GetPreviousDate(DateTime currentDate)
        {
            string query = $@"
SELECT TOP 1 DateValue({shiftNote_startTimeFieldName}) AS {shiftNote_startTimeFieldName}
FROM {shiftNote_tableName}
WHERE DateValue({shiftNote_startTimeFieldName}) < DateValue(?)
ORDER BY {shiftNote_startTimeFieldName} DESC;";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = currentDate }
                })
            );

            var previousDate = from DataRow note in notes
                               select note.Field<DateTime>(shiftNote_startTimeFieldName);

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

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>
                {
                    new System.Data.OleDb.OleDbParameter() { Value = month },
                    new System.Data.OleDb.OleDbParameter() { Value = year }
                })
            );

            var result = (from DataRow note in notes
                          select new
                          {
                              Date = note.Field<DateTime>(shiftNote_startTimeFieldName),
                              TotalEffort = note.Field<decimal>("TotalEffort")
                          }).ToDictionary(r => r.Date.Date, r => r.TotalEffort);
            return result;
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

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                query,
                new List<System.Data.OleDb.OleDbParameter>
                {
                    new System.Data.OleDb.OleDbParameter() { Value = date.Date.Day },
                    new System.Data.OleDb.OleDbParameter() { Value = date.Date.Month },
                    new System.Data.OleDb.OleDbParameter() { Value = date.Date.Year }
                })
            );

            var result = (from DataRow note in notes
                         select note.Field<decimal>("TotalEffort")).ToList();
            return result.FirstOrDefault();
        }

        public static async Task<int> GetSchemaVersion()
        {
            string query = $@"
SELECT TOP 1 {systemInfo_schemaVersionFieldName}
FROM {systemInfo_tableName};";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(query, force: true));

            var result = (from DataRow note in notes
                          select note.Field<int>(systemInfo_schemaVersionFieldName)).Single();
            return result;
        }

        public static async Task UpdateSchemaVersion(int version)
        {
            string query = $@"
UPDATE {systemInfo_tableName} SET {systemInfo_schemaVersionFieldName} = ? WHERE {systemInfo_idFieldName} = 1";

            await Task.Run(async () => await DataBaseConnection.ExecuteNonQueryAsync(query, new List<System.Data.OleDb.OleDbParameter>
            {
                new System.Data.OleDb.OleDbParameter() { Value = version }
            }, force: true));
        }

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

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(query));

            return from DataRow note in notes
                   select new Models.TrainData
                   {
                       CategoryId = note.Field<int>(shiftNote_categoryIdFieldName),
                       Description = note.Field<string>(shiftNote_descriptionFieldName)
                   };
        }
    }
}
