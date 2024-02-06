using Idler.Interfaces;
using Idler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Helpers.DB.Migrations
{
    [MigrationVersion(202006011)]
    public class InitialMigration : IMigration
    {
        public string[] Queries => new string[]
        {
             @"
CREATE TABLE ShiftNotes (
    Id AUTOINCREMENT PRIMARY KEY,
    ShiftId INT,
	Effort NUMERIC(3,2), 
	Description VARCHAR(255),
    CategoryId INT,
    StartTime DATETIME,
    EndTime DATETIME
);",

             @"
CREATE TABLE NoteCategories (
    Id AUTOINCREMENT PRIMARY KEY,
	Name VARCHAR(255),
	Hidden BIT
);"
        };
    }
}
