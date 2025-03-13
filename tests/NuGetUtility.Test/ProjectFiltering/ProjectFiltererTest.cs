using NuGetUtility.ProjectFiltering;

namespace NuGetUtility.Test.ProjectFiltering
{
    [TestFixture]
    class ProjectFiltererTest
    {
        private ProjectFilterer _filterer = null!;

        [SetUp]
        public void Setup()
        {
            _filterer = new ProjectFilterer();
        }

        [Test]
        public void FilterProjects_ExcludesSharedProjects_WhenIncludeSharedProjectsIsFalse()
        {
            var projects = new[] { "one.csproj", "two.shproj", "three.csproj", "four.SHPROJ" };

            var result = _filterer.FilterProjects(projects, false).ToArray();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Does.Contain("one.csproj"));
            Assert.That(result, Does.Contain("three.csproj"));
            Assert.That(result, Does.Not.Contain("two.shproj"));
            Assert.That(result, Does.Not.Contain("four.SHPROJ"));
        }

        [Test]
        public void FilterProjects_IncludesAllProjects_WhenIncludeSharedProjectsIsTrue()
        {
            var projects = new[] { "one.csproj", "two.shproj", "three.csproj", "four.SHPROJ" };

            var result = _filterer.FilterProjects(projects, true).ToArray();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result, Does.Contain("one.csproj"));
            Assert.That(result, Does.Contain("two.shproj"));
            Assert.That(result, Does.Contain("three.csproj"));
        }
    }
}
