namespace Idler.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;

    public static class TaskExtensions
    {
        public static Task SafeAsyncCall(this Task action, Action callback = null, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null)
        {
            setProcessing?.Invoke(true);

            return action.ContinueWith((r) =>
            {
                setProcessing?.Invoke(false);

                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred: {action.Exception}");
                    if (errorCallback != null)
                    {
                        Application.Current.Dispatcher.Invoke(errorCallback, action.Exception);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        Application.Current.Dispatcher.Invoke(callback);
                    }
                }
            });
        }

        public static Task SafeAsyncCall<T>(this Task<T> action, Action<T> callback = null, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null)
        {
            setProcessing?.Invoke(true);

            return action.ContinueWith((r) =>
            {
                setProcessing?.Invoke(false);

                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred: {action.Exception}");
                    if (errorCallback != null)
                    {
                        Application.Current.Dispatcher.Invoke(errorCallback, action.Exception);
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => callback(r.Result));
                    }
                }
            });
        }
    }
}
