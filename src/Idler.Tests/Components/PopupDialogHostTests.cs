namespace Idler.Tests.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Idler.Components;
    using Idler.Components.PopupDialogHostControl;
    using Idler.Helpers;
    using Idler.Tests.Testing;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PopupDialogHostTests : TestsBase
    {
        [Apartment(ApartmentState.STA)]
        [Test]
        public void ShowPopUp_IClosableDialog_InvokesCallbackOnClose()
        {
            DispatcherHelper.SetDispatcher(new TestDispatcher(Dispatcher.CurrentDispatcher));

            var host = new PopupDialogHost();
            var contextMock = new Mock<IClosableDialog>();
            var actionOnClose = Task.FromResult(108);
            contextMock.Setup(c => c.OnDialogClosing()).Returns(actionOnClose);
            var dialog = new Control()
            {
                DataContext = contextMock.Object
            };
            host.ShowPopUp("Test", dialog);

            var popup = host.Content as PopUpWrapper;
            popup.Close();

            host.Dispatcher.Invoke(() => {}, DispatcherPriority.Background); // ensures the dispatcher did the job

            using (AssertionScope.Current)
            {
                contextMock.Verify(c => c.OnDialogClosing(), Times.Once);
                host.Content.Should().BeNull();
            }
        }
    }
}
