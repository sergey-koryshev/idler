namespace Idler.Models
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents metadata of auto-categorization NLP model.
    /// </summary>
    [DataContract]
    public class AutoCategorizationModelMetadata
    {
        /// <summary>
        /// Gets or sets <see cref="DateTime"/> when the model was trained last time.
        /// </summary>
        [DataMember]
        public DateTime? TrainedOn { get; set; }
    }
}
