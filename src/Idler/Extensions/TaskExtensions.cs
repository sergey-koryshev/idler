namespace Idler.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Idler.Helpers;

    public static class TaskExtensions
    {
        /// <param name="callback">An optional callback to invoke if the operation completes successfully.</param>
        /// <inheritdoc cref="SafeAsyncCall{T}(Task{T}, Action{T}, Action{bool}, Action{AggregateException})"/>
        public static Task SafeAsyncCall(this Task action, Action<CancellationToken> callback = null, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null, CancellationToken? cancellationToken = null)
        {
            return SafeAsyncCallInternal(
                action,
                success: () => callback?.Invoke(cancellationToken ?? CancellationToken.None),
                setProcessing,
                errorCallback);
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
        /// <param name="errorCallback">An optional callback to handle errors. Invoked with the exception.</param>
        /// <returns>A <see cref="Task"/> representing the continuation of the asynchronous operation.</returns>
        public static Task SafeAsyncCall<T>(this Task<T> action, Action<T, CancellationToken> callback = null, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null, CancellationToken? cancellationToken = null)
        {
            return SafeAsyncCallInternal(
                action,
                success: () => callback?.Invoke(action.Result, cancellationToken ?? CancellationToken.None),
                setProcessing,
                errorCallback);
        }

        /// <summary>
        /// Internal helper method that handles the common logic for both Task and Task{T} SafeAsyncCall methods.
        /// </summary>
        /// <param name="action">The task to process.</param>
        /// <param name="success">Action to execute on successful completion.</param>
        /// <param name="setProcessing">Processing state callback.</param>
        /// <param name="errorCallback">Error handling callback.</param>
        /// <returns>A continuation task.</returns>
        private static Task SafeAsyncCallInternal(Task action, Action success, Action<bool> setProcessing, Action<AggregateException> errorCallback)
        {
            if (setProcessing != null)
            {
                DispatcherHelper.CurrentDispatcher.Invoke(setProcessing, true);
            }

            return action.ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    bool isCanceled = t.Exception.Flatten().InnerExceptions.Any(ex => ex is OperationCanceledException);
                    if (isCanceled)
                    {
                        Trace.TraceWarning($"Task was canceled due to OperationCanceledException.");
                    }
                    else
                    {
                        Trace.TraceError($"Error has occurred: {t.Exception}");
                        if (errorCallback != null)
                        {
                            DispatcherHelper.CurrentDispatcher.Invoke(errorCallback, t.Exception);
                        }
                    }
                }
                else if (t.IsCanceled)
                {
                    Trace.TraceWarning($"Task was canceled.");
                }
                else if (success != null)
                {
                    DispatcherHelper.CurrentDispatcher.Invoke(success);
                }

                if (setProcessing != null)
                {
                    DispatcherHelper.CurrentDispatcher.Invoke(setProcessing, false);
                }

                return t;
            }).Unwrap();
        }
    }
}
