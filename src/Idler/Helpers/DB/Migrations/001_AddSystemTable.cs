using Idler.Interfaces;
using Idler.Models;

namespace Idler.Helpers.DB.Migrations
{
    [MigrationVersion(202402051)]
    public class AddSystemInfoTable : IMigration
    {
        public string[] Queries => new string[] {
                @"
CREATE TABLE SystemInfo (
    Id INTEGER NOT NULL,
    SchemaVersion INTEGER NOT NULL
);",
                @"
INSERT INTO SystemInfo (Id, SchemaVersion) VALUES (1, 202402051);"
        };
    }
}
