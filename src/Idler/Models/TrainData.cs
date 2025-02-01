using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Models
{
    public class TrainData
    {
        [LoadColumn(0)]
        public string Description { get; set; }

        [LoadColumn(1)]
        public int CategoryId { get; set; }
    }
}
