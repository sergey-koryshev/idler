using Idler.Interfaces;
using Idler.Models;

namespace Idler.Helpers.DB.Migrations
{
    [MigrationVersion(202402052)]
    public class AddColumnSortOrder : IMigration
    {
        public string[] Queries => new string[] {
            @"
ALTER TABLE ShiftNotes ADD COLUMN SortOrder INTEGER;"
        };
    }
}
