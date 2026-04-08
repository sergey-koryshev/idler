namespace Idler.Tests.Testing
{
    using System;
    using Idler.Helpers;
    using Idler.Interfaces;
    using Moq;

    public class TestsHelper
    {
        public static Mock<IDispatcher> SetMockedDispatcher()
        {
            var dispatcherMock = new Mock<IDispatcher>();
            dispatcherMock.Setup(d => d.Invoke(It.IsAny<Delegate>(), It.IsAny<object[]>()))
                .Callback<Delegate, object[]>((method, args) =>
                {
                    if (method is Action action && (args == null || args.Length == 0))
                    {
                        action();
                    }
                    else if (method is Action<bool> actionBool && args?.Length == 1 && args[0] is bool boolArg)
                    {
                        actionBool(boolArg);
                    }
                    else if (method is Action<int> actionInt && args?.Length == 1 && args[0] is int intArg)
                    {
                        actionInt(intArg);
                    }
                    else if (method is Action<AggregateException> actionEx && args?.Length == 1 && args[0] is AggregateException exArg)
                    {
                        actionEx(exArg);
                    }
                    else if (method is Action<object> actionObj && args?.Length == 1)
                    {
                        actionObj(args[0]);
                    }
                    else if (method is Action<AggregateException, bool> actionExBool && args?.Length == 2 &&
                            args[0] is AggregateException exArg2 && args[1] is bool boolArg2)
                    {
                        actionExBool(exArg2, boolArg2);
                    }
                });
            DispatcherHelper.SetDispatcher(dispatcherMock.Object);
            return dispatcherMock;
        }
    }
}
