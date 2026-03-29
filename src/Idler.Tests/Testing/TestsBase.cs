namespace Idler.Tests.Testing
{
    using System;
    using Idler.Extensions;
    using Idler.Interfaces;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public abstract class TestsBase
    {
        protected Mock<IDispatcher> DispatcherMock { get; set; }

        [SetUp]
        public void TestsBaseSetUp()
        {
            this.DispatcherMock = TestsHelper.SetMockedDispatcher();
        }

        [OneTimeSetUp]
        public void TestsBaseOneTimeSetUp()
        {
            this.DispatcherMock = TestsHelper.SetMockedDispatcher();
        }
    }
}
