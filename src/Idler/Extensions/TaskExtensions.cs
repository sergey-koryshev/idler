using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idler.Extensions
{
    public static class TaskExtensions
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void SafeFireAndForget(this Task task, bool continueOnCapturedContext = true, Action<System.Exception> onException = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException?.Invoke(ex);
            }
        }

        public static void SafeAsyncCall(this Task action, Action<bool> setProcessing = null)
        {
            setProcessing?.Invoke(true);

            action.ContinueWith((r) =>
            {
                setProcessing?.Invoke(false);

                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred: {action.Exception}");
                }
            });
        }

        public static void SafeAsyncCall<T>(this Task<T> action, Action<T> callback = null, Action<bool> setProcessing = null)
        {
            setProcessing?.Invoke(true);

            action.ContinueWith((r) =>
            {
                setProcessing?.Invoke(false);

                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred: {action.Exception}");
                }
                else
                {
                    callback?.Invoke(r.Result);
                }
            });
        }

    }
}
