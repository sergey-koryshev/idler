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
            Application.Current.Dispatcher.Invoke(method, args);
        }
    }
}
