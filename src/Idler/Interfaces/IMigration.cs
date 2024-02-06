using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Interfaces
{
    public interface IMigration
    {
        string[] Queries { get; }
    }
}
