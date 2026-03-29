namespace Idler.Tests.Managers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Idler.Helpers.DB;
    using Idler.Managers;
    using Idler.Tests.Testing;
    using NUnit.Framework;

    [TestFixture]
    public class AutoCompleteManagerTests : TestsBase
    {
        NoteCategory testCategory;

        [Test]
        public async Task GetSuggestion_NoMatchingDescription_NullReturned()
        {
            var actual = await new AutoCompleteManager().GetSuggestion("Not existing description", CancellationToken.None);
            actual.Should().BeNull();
        }

        [Test]
        public async Task GetSuggestion_MatchingDescriptionExists_AppropriateRecentDescriptionReturned()
        {
            var noteA = new ShiftNote(new ObservableCollection<ShiftNote>())
            {
                Effort = 1,
                Description = "Test Description A",
                StartTime = DateTime.Now
            };
            await DataBaseFunctions.CreateNote(noteA);
            var first = await new AutoCompleteManager().GetSuggestion("Test", CancellationToken.None);
            first.Should().Be(" Description A");

            var noteB = new ShiftNote(new ObservableCollection<ShiftNote>())
            {
                Effort = 1,
                Description = "Test Description B",
                StartTime = DateTime.Now
            };
            await DataBaseFunctions.CreateNote(noteB);
            var second = await new AutoCompleteManager().GetSuggestion("Test", CancellationToken.None);
            second.Should().Be(" Description B");
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            this.testCategory = new NoteCategory("Test Category", false);
            await DataBaseFunctions.CreateCategory(this.testCategory);
        }
    }
}
