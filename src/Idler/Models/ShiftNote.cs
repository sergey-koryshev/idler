using MiniExcelLibs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Models
{
    public class ShiftNote
    {
        public int Id { get; set; }

        public string Category { get; set; }

        public decimal Effort { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }
    }
}
