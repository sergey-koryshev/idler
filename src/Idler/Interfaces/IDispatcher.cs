namespace Idler.Interfaces
{
    using System;
    using System.Windows.Threading;

    /// <summary>
    /// Interface for abstracting dispatcher operations.
    /// </summary>
    public interface IDispatcher
    {
        /// <inheritdoc cref="Dispatcher.Invoke(Delegate, object[])" />
        void Invoke(Delegate method, params object[] args);
    }
}
