namespace Idler.Helpers
{
    using System;
    using System.Threading;
    using Idler.Interfaces;

    /// <summary>
    /// Provides helper methods for managing the dispatcher.
    /// </summary>
    public static class DispatcherHelper
    {
        private static Lazy<IDispatcher> lazyDispatcher = new Lazy<IDispatcher>(() => new ApplicationDispatcher(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets the dispatcher instance.
        /// </summary>
        public static IDispatcher CurrentDispatcher => lazyDispatcher.Value;

        /// <summary>
        /// Sets the dispatcher.
        /// </summary>
        /// <remarks>
        /// The method is for unit testing purposes.
        /// </remarks>
        /// <param name="dispatcher">The dispatcher to use.</param>
        public static void SetDispatcher(IDispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            Interlocked.Exchange(ref lazyDispatcher, new Lazy<IDispatcher>(() => dispatcher, LazyThreadSafetyMode.ExecutionAndPublication));
        }

        /// <summary>
        /// Resets the dispatcher to the default implementation.
        /// </summary>
        /// <remarks>
        /// The method is for unit testing purposes.
        /// </remarks>
        public static void ResetDispatcher()
        {
            Interlocked.Exchange(ref lazyDispatcher, new Lazy<IDispatcher>(() => new ApplicationDispatcher(), LazyThreadSafetyMode.ExecutionAndPublication));
        }
    }
}
