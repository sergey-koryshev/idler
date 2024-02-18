namespace Idler.Models
{
    using System;

    public class MigrationVersionAttribute : Attribute
    {
        public int Version { get; set; }

        public MigrationVersionAttribute(int version)
        {
            this.Version = version;
        }
    }
}
