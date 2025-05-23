namespace Idler.Models
{
    using Microsoft.ML.Data;

    /// <summary>
    /// Represents the result of a prediction made by the NLP model.
    /// </summary>
    public class PredictionResult
    {
        /// <summary>
        /// Gets or sets the predicted category identifier based on the model's output.
        /// </summary>
        [ColumnName("PredictedLabel")]
        public int PredictedCategoryId { get; set; }
    }
}
