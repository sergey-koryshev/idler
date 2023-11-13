using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Models
{
    public enum TotalEffortType
    {
        None = 0xffffff,
        Parttime = 0xfff8e0,
        CompleteShift = 0xc3f9ea,
        Overtime = 0xfde4de
    }
}
