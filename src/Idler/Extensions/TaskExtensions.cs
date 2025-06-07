namespace Idler.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Idler.Helpers;
    using Idler.Interfaces;

    public static class TaskExtensions
    {
        private static Lazy<IDispatcher> lazyDispatcher = new Lazy<IDispatcher>(() => new ApplicationDispatcher(), LazyThreadSafetyMode.ExecutionAndPublication);

        private static IDispatcher CurrentDispatcher => lazyDispatcher.Value;

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

        /// <param name="callback">An optional callback to invoke if the operation completes successfully.</param>
        /// <inheritdoc cref="SafeAsyncCall{T}(Task{T}, Action{T}, Action{bool}, Action{AggregateException})"/>
        public static Task SafeAsyncCall(this Task action, Action callback = null, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null)
        {
            return SafeAsyncCallInternal(
                action,
                success: () => callback?.Invoke(),
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
        public static Task SafeAsyncCall<T>(this Task<T> action, Action<T> callback = null, Action<bool> setProcessing = null, Action<AggregateException> errorCallback = null)
        {
            return SafeAsyncCallInternal(
                action,
                success: () => callback?.Invoke(action.Result),
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
                CurrentDispatcher.Invoke(setProcessing, true);
            }

            return action.ContinueWith((r) =>
            {
                if (action.IsFaulted)
                {
                    bool isCanceled = action.Exception.Flatten().InnerExceptions.Any(ex => ex is OperationCanceledException);
                    if (isCanceled)
                    {
                        Trace.TraceWarning($"Task was canceled due to OperationCanceledException.");
                    }
                    else
                    {
                        Trace.TraceError($"Error has occurred: {action.Exception}");
                        if (errorCallback != null)
                        {
                            CurrentDispatcher.Invoke(errorCallback, action.Exception);
                        }
                    }
                }
                else if (action.IsCanceled)
                {
                    Trace.TraceWarning($"Task was canceled.");
                }
                else if (success != null)
                {
                    CurrentDispatcher.Invoke(success);
                }

                if (setProcessing != null)
                {
                    CurrentDispatcher.Invoke(setProcessing, false);
                }
            });
        }
    }
}
