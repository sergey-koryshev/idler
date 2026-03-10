namespace Idler.Models
{
    using Microsoft.ML.Data;

    public class AutoCompletePredictionResult
    {
        [ColumnName("PredictedLabel")]
        public string Completion { get; set; }

        [ColumnName("Score")]
        public float[] Scores { get; set; }
    }
}
