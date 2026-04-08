namespace Idler.Tests.Helpers
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Idler.Extensions;
    using Idler.Helpers;
    using Idler.Interfaces;
    using Idler.Tests.Testing;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DispatcherHelperTests : TestsBase
    {
        [TearDown]
        public void TearDown()
        {
            DispatcherHelper.ResetDispatcher();
        }

        /// <summary>
        /// Verifies that the <see cref="DispatcherHelper.SetDispatcher"/> method throws an
        /// <see cref="ArgumentNullException"/> when the <paramref name="dispatcher"/> parameter is null.
        /// </summary>
        [Test]
        public void SetDispatcher_ThrowsArgumentNullException_WhenDispatcherIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => DispatcherHelper.SetDispatcher(null));
        }

        /// <summary>
        /// Tests the thread safety of the <see cref="DispatcherHelper.SetDispatcher"/> method
        /// when accessed concurrently from multiple threads.
        /// </summary>
        [Test]
        public void SetDispatcher_ConcurrentAccess_NoErrorsThrown()
        {
            var tasks = new Task[10];
            var mocks = new Mock<IDispatcher>[10];
            var results = new bool[10];

            for (int i = 0; i < 10; i++)
            {
                mocks[i] = new Mock<IDispatcher>();
                int index = i;
                mocks[i].Setup(d => d.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>()))
                    .Callback<Delegate, object[]>((method, args) =>
                    {
                        results[index] = true;
                        ((Action)method)();
                    });
            }

            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() =>
                {
                    DispatcherHelper.SetDispatcher(mocks[index].Object);
                    Task.CompletedTask.SafeAsyncCall((_) => { });
                });
            }

            Task.WaitAll(tasks);
            results.Should().Contain(true);
        }
    }
}
