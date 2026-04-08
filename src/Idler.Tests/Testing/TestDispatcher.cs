namespace Idler.Tests.Testing
{
    using System;
    using System.Windows.Threading;
    using Idler.Interfaces;

    public class TestDispatcher : IDispatcher
    {
        private readonly Dispatcher dispatcher;

        public TestDispatcher(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void Invoke(Delegate method, params object[] args)
        {
            this.dispatcher.Invoke(method, args);
        }
    }
}
