namespace Idler.Models
{
    using Microsoft.ML.Data;

    /// <summary>
    /// Represents a data model for training machine learning algorithms, containing a description and a category
    /// identifier.
    /// </summary>
    public class TrainData
    {
        /// <summary>
        /// Gets or sets the description associated with the data entry.
        /// </summary>
        [LoadColumn(0)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the category associated with the data.
        /// </summary>
        [LoadColumn(1)]
        public int CategoryId { get; set; }
    }
}
