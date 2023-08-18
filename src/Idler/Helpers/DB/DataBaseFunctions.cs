using Idler.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;

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
        private static readonly string shiftNote_categoryNameFieldName = "CategoryName";

        private const string noteCategories_tableName = "NoteCategories";
        private const string noteCategories_idFieldName = "Id";
        private const string noteCategories_nameFieldName = "Name";
        private const string noteCategories_hiddenFieldName = "Hidden";

        public static async Task<IEnumerable<Models.ShiftNote>> GetNotesByDates(DateTime from, DateTime to)
        {
            string queryToGetNotesByShiftId = $@"
SELECT 
    sn.{shiftNote_idFieldName}, 
    nc.{noteCategories_nameFieldName} AS {shiftNote_categoryNameFieldName}, 
    sn.{shiftNote_effortFiedlName},
    sn.{shiftNote_descriptionFieldName},
    sn.{shiftNote_startTimeFieldName}
FROM {shiftNote_tableName} sn INNER JOIN
    {noteCategories_tableName} nc
    ON sn.{shiftNote_categoryIdFieldName} = nc.{noteCategories_idFieldName}
WHERE {shiftNote_startTimeFieldName} BETWEEN Format(?,""mm/dd/yyyy"") AND Format(?,""mm/dd/yyyy"")";

            DataRowCollection notes = await Task.Run(async () => await DataBaseConnection.GetRowCollectionAsync(
                queryToGetNotesByShiftId,
                new List<System.Data.OleDb.OleDbParameter>()
                {
                    new System.Data.OleDb.OleDbParameter() { Value = from },
                    new System.Data.OleDb.OleDbParameter() { Value = to }
                })
            );

            return from DataRow note in notes select new Models.ShiftNote
            {
                Id = note.Field<int>(shiftNote_idFieldName),
                Category = note.Field<string>(shiftNote_categoryNameFieldName),
                Date = note.Field<DateTime>(shiftNote_startTimeFieldName),
                Effort = note.Field<decimal>(shiftNote_effortFiedlName),
                Description = note.Field<string>(shiftNote_descriptionFieldName),
            };
        }
    }
}
