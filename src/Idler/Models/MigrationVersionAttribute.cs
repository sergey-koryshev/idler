namespace Idler.Models
{
    using System;

    public class MigrationVersionAttribute : Attribute
    {
        /// <summary>
        /// The version should have the following format: YYYYMMDDN,
        /// where N - number from 1 to 9.
        /// </summary>
        public int Version { get; set; }

        public MigrationVersionAttribute(int version)
        {
            this.Version = version;
        }
    }
}
