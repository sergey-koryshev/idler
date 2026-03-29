namespace Idler.Tests
{
    using System.Threading.Tasks;
    using Idler.Helpers.DB;
    using Idler.Tests.Testing;
    using NUnit.Framework;

    [SetUpFixture]
    public class FixtureSetup
    {
        public FixtureSetup() { }

        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestsHelper.SetMockedDispatcher();
            await DataBaseFunctions.RemoveAllNotes();
            await DataBaseFunctions.RemoveAllCategories();
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            TestsHelper.SetMockedDispatcher();
            await DataBaseFunctions.RemoveAllNotes();
            await DataBaseFunctions.RemoveAllCategories();
        }
    }
}
