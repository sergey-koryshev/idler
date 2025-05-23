// -----------------------------------------------------------------------------
//  NlpModelStatus.cs
//  Defines the status flags for NLP model lifecycle states.
// -----------------------------------------------------------------------------

namespace Idler.Models
{
    using System;

    /// <summary>
    /// Represents the various status flags for an NLP model's lifecycle.
    /// Supports bitwise operations to allow combination of multiple states.
    /// </summary>
    [Flags]
    public enum NlpModelStatus
    {
        /// <summary>
        /// No status is set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The model is currently loading.
        /// </summary>
        Loading = 1,

        /// <summary>
        /// The model has finished loading.
        /// </summary>
        Loaded = 1 << 1,

        /// <summary>
        /// The model is currently training.
        /// </summary>
        Training = 1 << 2,

        /// <summary>
        /// The model has finished training.
        /// </summary>
        Trained = 1 << 3,

        /// <summary>
        /// The model is in progress (either loading or training).
        /// </summary>
        InProgress = Loading | Training,

        /// <summary>
        /// The model has is ready to use (either loaded or trained).
        /// </summary>
        Completed = Loaded | Trained
    }
}
