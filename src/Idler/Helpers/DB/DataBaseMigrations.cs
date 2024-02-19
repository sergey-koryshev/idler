namespace Idler.Helpers.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Idler.Interfaces;
    using Idler.Models;

    public class DataBaseMigrations
    {
        public const int initialMigration = 202006011;

        private const string migrationsNameSpace = "Idler.Helpers.DB.Migrations";
        private const int systemInfoTableMigration = 202402051;

        private Dictionary<int, Type> migrations = new Dictionary<int, Type>(0);

        public DataBaseMigrations()
        { 
            this.LoadMigrations();
        }

        public async Task ApplyMigrations(int previousSchemaVersion)
        {
            var targetMigrations = this.migrations.Where(p => p.Key > previousSchemaVersion).OrderBy(p => p.Key).ToList();

            foreach (var targetMigration in targetMigrations)
            {
                IMigration migration = (IMigration)Activator.CreateInstance(targetMigration.Value);

                foreach (string query in migration.Queries)
                {
                    await DataBaseConnection.ExecuteNonQueryAsync(query, force: true);
                }
                
                if (targetMigration.Key > systemInfoTableMigration)
                {
                    await DataBaseFunctions.UpdateSchemaVersion(targetMigration.Key);
                }
            }
        }

        private void LoadMigrations()
        {
            this.migrations = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, migrationsNameSpace, StringComparison.Ordinal) && t.GetInterfaces().Contains(typeof(IMigration)))
                .Select(t => new { Type = t, VersionAttribute = t.GetCustomAttribute<MigrationVersionAttribute>() })
                .Where(x => x.VersionAttribute != null)
                .ToDictionary(x => x.VersionAttribute.Version, x => x.Type);
        }
    }
}
