namespace Idler.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    public static class TaskExtensions
    {
        /// <param name="callback">An optional callback to invoke if the operation completes successfully.</param>
        /// <inheritdoc cref="SafeAsyncCall{T}(Task{T}, Action{T}, Action{bool}, Action{AggregateException, bool})"/>
        public static Task SafeAsyncCall(this Task action, Action callback = null, Action<bool> setProcessing = null, Action<AggregateException, bool> errorCallback = null)
        {
            Action<object> callbackWrapper;

            if (callback == null)
            {
                callbackWrapper = null;
            }
            else
            {
                callbackWrapper = _ => callback();
            }

            Task<object> wrappedTask = action.ContinueWith(t => (object)null);
            
            return wrappedTask.SafeAsyncCall(callbackWrapper, setProcessing, errorCallback);
        }

        /// <summary>
        /// Executes an asynchronous operation and provides mechanisms for handling its result, errors, and processing
        /// state.
        /// </summary>
        /// <typeparam name="T">The type of the result produced by the asynchronous operation.</typeparam>
        /// <param name="action">The asynchronous operation to execute. Cannot be null.</param>
        /// <param name="callback">An optional callback to invoke with the result of the operation if it completes successfully.</param>
        /// <param name="setProcessing">An optional callback to indicate the processing state. Invoked with <see langword="true"/> before the
        /// operation starts and <see langword="false"/> after it completes.</param>
        /// <param name="errorCallback">An optional callback to handle errors. Invoked with the exception and a boolean indicating whether the
        /// operation was canceled.</param>
        /// <returns>A <see cref="Task"/> representing the continuation of the asynchronous operation.</returns>
        public static Task SafeAsyncCall<T>(this Task<T> action, Action<T> callback = null, Action<bool> setProcessing = null, Action<AggregateException, bool> errorCallback = null)
        {
            if (setProcessing != null)
            {
                Application.Current.Dispatcher.Invoke(setProcessing, true);
            }

            return action.ContinueWith((r) =>
            {
                if (action.IsFaulted)
                {
                    Trace.TraceError($"Error has been occurred: {action.Exception}");
                    if (errorCallback != null)
                    {
                        bool isCanceled = action.Exception.Flatten().InnerExceptions.Any(ex => ex is OperationCanceledException);
                        Application.Current.Dispatcher.Invoke(errorCallback, action.Exception.InnerException, isCanceled);
                    }
                }
                else if (action.IsCanceled) { /* Do nothing */ }
                else
                {
                    if (callback != null)
                    {
                        Application.Current.Dispatcher.Invoke(callback, r.Result);
                    }
                }

                if (setProcessing != null)
                {
                    Application.Current.Dispatcher.Invoke(setProcessing, false);
                }
            });
        }
    }
}
