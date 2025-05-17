namespace Idler.Models
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a background task with a name and associated <see cref="Task"/>.
    /// </summary>
    public class BackgroundTask
    {
        /// <summary>
        /// Gets or sets the name of the background task.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Task"/> associated with this background task.
        /// </summary>
        public Task Action { get; set; }
    }
}
