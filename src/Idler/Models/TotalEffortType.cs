using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Models
{
    public enum TotalEffortType
    {
        [DarkerColor(0xaaaaaa)]
        None = 0xffffff,

        [DarkerColor(0xfec500)]
        Parttime = 0xfff8e0,

        [DarkerColor(0x15d6a0)]
        CompleteShift = 0xc3f9ea,

        [DarkerColor(0xf35934)]
        Overtime = 0xfde4de
    }
}
