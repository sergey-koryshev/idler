namespace Idler.Tests.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Idler.Extensions;
    using Idler.Tests.Testing;
    using Moq;
    using NUnit.Framework;

    using TaskExtensions = Idler.Extensions.TaskExtensions;

    [TestFixture]
    public class TaskExtensionsTests : TestsBase
    {
        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall"/> method completes successfully when
        /// invoked with a completed task and no callbacks.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_TaskWithoutCallbacks_CompletesSuccessfully()
        {
            var task = Task.CompletedTask;

            var result = task.SafeAsyncCall();
            await result;

            result.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall"/> method successfully invokes
        /// the provided callback when the task completes successfully.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_TaskWithCallback_InvokesCallbackOnSuccess()
        {
            var task = Task.CompletedTask;
            bool callbackInvoked = false;
            Action<CancellationToken> callback = (_) => callbackInvoked = true;

            var result = TaskExtensions.SafeAsyncCall(task, callback, null, null);
            await result;

            callbackInvoked.Should().BeTrue();
            DispatcherMock.Verify(d => d.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>()), Times.Once);
        }

        /// <summary>
        /// Tests that the <see cref="TaskExtensions.SafeAsyncCall"/> method correctly sets the
        /// processing state when provided with a task and a delegate to update the processing state.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_SetProcessingSpecified_SetsProcessingStateCorrectly()
        {
            var task = Task.CompletedTask;
            bool? processingState = null;
            bool processingStarted = false;
            bool processingEnded = false;
            Action<bool> setProcessing = state => 
            {
                processingState = state;
                if (state)
                {
                    processingStarted = true;
                }
                else
                {
                    processingEnded = true;
                }
            };

            var result = task.SafeAsyncCall(setProcessing: setProcessing);
            await result;

            processingStarted.Should().BeTrue("Processing should start with true");
            processingEnded.Should().BeTrue("Processing should end with false");
            DispatcherMock.Verify(d => d.Invoke(It.IsAny<Delegate>(), It.Is<object[]>(args => 
                args.Length == 1 && (bool)args[0] == true)), Times.Once);
            DispatcherMock.Verify(d => d.Invoke(It.IsAny<Delegate>(), It.Is<object[]>(args => 
                args.Length == 1 && (bool)args[0] == false)), Times.Once);
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall"/> method correctly invokes the
        /// specified error callback when the provided task completes with an exception.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_TaskWithError_InvokesErrorCallback()
        {
            var exception = new InvalidOperationException("Test exception");
            var task = Task.FromException(exception);
            AggregateException capturedEx = null;
            Action<AggregateException> errorCallback = ex => {
                capturedEx = ex;
            };

            var result = task.SafeAsyncCall(errorCallback: errorCallback);
            await result;

            capturedEx.Should().NotBeNull();
            capturedEx.InnerException.Should().BeOfType<InvalidOperationException>();
            capturedEx.InnerException.Message.Should().Be("Test exception");
        }

        /// <summary>
        /// Tests that the <see cref="TaskExtensions.SafeAsyncCall"/> method correctly handles a task 
        /// that throws an <see cref="OperationCanceledException"/> without invoking the error callback.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_TaskWithOperationCanceled_DetectsCancellation()
        {
            var exception = new OperationCanceledException();
            var task = Task.FromException(exception);
            bool errorCallbackInvoked = false;
            Action<AggregateException> errorCallback = ex => 
            {
                errorCallbackInvoked = true;
            };

            var result = task.SafeAsyncCall(errorCallback: errorCallback);
            await result;

            errorCallbackInvoked.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall"/> method does not invoke the
        /// success callback when the provided task is canceled.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_TaskWithCancellation_DoesNotInvokeSuccessCallback()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var task = Task.FromCanceled(cts.Token);
            bool callbackInvoked = false;
            Action<CancellationToken> callback = (_) => callbackInvoked = true;

            var result = task.SafeAsyncCall(callback);
            await result;

            callbackInvoked.Should().BeFalse("Success callback should not be invoked on canceled task");
        }

        /// <summary>
        /// Ensures that a generic asynchronous task completes successfully without requiring callbacks.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_GenericTaskWithoutCallbacks_CompletesSuccessfully()
        {
            var task = Task.FromResult(108);

            var result = task.SafeAsyncCall();

            await result;
            result.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        /// <summary>
        /// Tests that the <see cref="TaskExtensions.SafeAsyncCall{T}"/> method correctly invokes
        /// the provided callback with the result of the asynchronous operation.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_GenericTaskWithCallback_InvokesCallbackWithResult()
        {
            var task = Task.FromResult(108);
            int? capturedResult = null;
            Action<int, CancellationToken> callback = (value, _) => capturedResult = value;

            var result = task.SafeAsyncCall(callback);
            await result;

            capturedResult.Should().Be(108);
            DispatcherMock.Verify(d => d.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>()), Times.Once);
        }

        /// <summary>
        /// Tests that the <see cref="TaskExtensions.SafeAsyncCall{T}"/> method correctly invokes
        /// the provided error callback when the asynchronous operation results in an exception.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_GenericTaskWithError_InvokesErrorCallback()
        {
            var exception = new InvalidOperationException("Test exception");
            var task = Task.FromException<int>(exception);
            AggregateException capturedEx = null;
            Action<AggregateException> errorCallback = ex => {
                capturedEx = ex;
            };

            var result = task.SafeAsyncCall(errorCallback: errorCallback);
            await result;

            capturedEx.Should().NotBeNull();
            capturedEx.InnerException.Should().BeOfType<InvalidOperationException>();
            capturedEx.InnerException.Message.Should().Be("Test exception");
        }

        /// <summary>
        /// Tests that the <see cref="TaskExtensions.SafeAsyncCall{T}"/> method correctly sets the processing
        /// state when invoked with a generic task and a <paramref name="setProcessing"/> callback.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_GenericTaskWithSetProcessing_SetsProcessingStateCorrectly()
        {
            var task = Task.FromResult(108);
            bool processingStarted = false;
            bool processingEnded = false;
            Action<bool> setProcessing = state => 
            {
                if (state) processingStarted = true;
                else processingEnded = true;
            };

            var result = task.SafeAsyncCall(setProcessing: setProcessing);
            await result;

            processingStarted.Should().BeTrue("Processing should start with true");
            processingEnded.Should().BeTrue("Processing should end with false");
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall"/> method propagate
        /// empty <see cref="CancellationToken"/> to specified callback when it's not passed to params.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_CancellationTokenNotProvided_EmptyCancellationTokenPropagated()
        {
            var task = Task.FromResult(108);
            CancellationToken? cancellationToken = null;
            
            Action<CancellationToken> callback = token =>
            {
                cancellationToken = token;
            };

            var result = task.SafeAsyncCall(callback: callback);
            await result;

            cancellationToken.Should().Be(CancellationToken.None);
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall{T}"/> method propagate
        /// empty <see cref="CancellationToken"/> to specified callback when it's not passed to params.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_T_CancellationTokenNotProvided_EmptyCancellationTokenPropagated()
        {
            var task = Task.FromResult(108);
            CancellationToken? cancellationToken = null;

            Action<int, CancellationToken> callback = (_, token) =>
            {
                cancellationToken = token;
            };

            var result = task.SafeAsyncCall(callback: callback);
            await result;

            cancellationToken.Should().Be(CancellationToken.None);
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall"/> method propagate
        /// empty <see cref="CancellationToken"/> to specified callback when it's not passed to params.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_CancellationTokenProvided_ProvidedCancellationTokenPropagated()
        {
            var task = Task.FromResult(108);
            CancellationToken cancellationToken = new CancellationToken();
            CancellationToken? cancellationTokenFromAction = null;

            Action<CancellationToken> callback = token =>
            {
                cancellationTokenFromAction = token;
            };

            var result = task.SafeAsyncCall(callback: callback, cancellationToken: cancellationToken);
            await result;

            cancellationTokenFromAction.Should().Be(cancellationToken);
        }

        /// <summary>
        /// Verifies that the <see cref="TaskExtensions.SafeAsyncCall{T}"/> method propagate
        /// empty <see cref="CancellationToken"/> to specified callback when it's not passed to params.
        /// </summary>
        [Test]
        public async Task SafeAsyncCall_T_CancellationTokenProvided_ProvidedCancellationTokenPropagated()
        {
            var task = Task.FromResult(108);
            CancellationToken cancellationToken = new CancellationToken();
            CancellationToken? cancellationTokenFromAction = null;

            Action<int, CancellationToken> callback = (_, token) =>
            {
                cancellationTokenFromAction = token;
            };

            var result = task.SafeAsyncCall(callback: callback, cancellationToken: cancellationToken);
            await result;

            cancellationTokenFromAction.Should().Be(cancellationToken);
        }
    }
}