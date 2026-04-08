namespace Idler.Helpers
{
    using Idler.Interfaces;
    using System;
    using System.Windows;

    /// <summary>
    /// Default implementation of IDispatcher that uses the Application.Current.Dispatcher.
    /// </summary>
    public class ApplicationDispatcher : IDispatcher
    {
        public void Invoke(Delegate method, params object[] args)
        {
            if (Application.Current?.Dispatcher == null)
            {
                throw new InvalidOperationException("No dispatcher available. Ensure that the application is properly initialized.");
            }

            Application.Current.Dispatcher.Invoke(method, args);
        }
    }
}
