using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Models
{
    public class MigrationVersionAttribute : Attribute
    {
        public int Version { get; set; }

        public MigrationVersionAttribute(int version)
        {
            this.Version = version;
        }
    }
}
