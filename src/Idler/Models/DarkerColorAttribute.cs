using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Models
{
    public class DarkerColorAttribute : Attribute
    {
        public int Color { get; set; }

        public DarkerColorAttribute(int color)
        {
            this.Color = color;
        }
    }
}
