namespace Idler.Models
{
    using Microsoft.ML.Data;

    public class AutoCompleteInputData
    {
        [ColumnName("Text")]
        public string Description { get; set; }
    }
}
