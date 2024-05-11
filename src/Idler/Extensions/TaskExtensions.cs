namespace Idler.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static Task SafeAsyncCall(this Task action, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null)
        {
            setProcessing?.Invoke(true);

            return action.ContinueWith((r) =>
            {
                setProcessing?.Invoke(false);

                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred: {action.Exception}");
                    errorCallback?.Invoke(action.Exception);
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
                    errorCallback?.Invoke(action.Exception);
                }
                else
                {
                    callback?.Invoke(r.Result);
                }
            });
        }
    }
}
