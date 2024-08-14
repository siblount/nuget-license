// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NSubstitute;
using NuGetUtility.Extensions;
using NuGetUtility.Wrapper.MsBuildWrapper;

namespace NuGetUtility.Test.Extensions
{
    [TestFixture]
    public class ProjectExtensionsTest
    {
        [SetUp]
        public void SetUp()
        {
            _project = Substitute.For<IProject>();
        }

        private IProject _project = null!;

        [Test]
        public void HasNugetPackagesReferenced_Should_ReturnTrue_If_PackageReferenceCountIsMoreThanZero(
            [Values(1, 50, 999)] int referenceCount)
        {
            _project.GetPackageReferenceCount().Returns(referenceCount);

            bool result = _project.HasNuGetPackagesReferenced();

            Assert.That(result, Is.True);
        }

        [Test]
        public void HasNugetPackagesReferences_Should_ReturnTrue_If_ProjectHasPackagesConfigFileReferenced()
        {
            _project.GetPackageReferenceCount().Returns(0);
            _project.GetEvaluatedIncludes().Returns(new List<string> { "packages.config" });

            bool result = _project!.HasNuGetPackagesReferenced();

            Assert.That(result, Is.True);
        }

        [Test]
        public void
            HasNugetPackagesReferenced_Should_ReturnFalse_If_PackageReferenceCountIsZeroOrLess_And_ProjectHasNoPackagesConfigFileReferenced(
                [Values(-9999, -50, 0)] int referenceCount)
        {
            _project.GetPackageReferenceCount().Returns(referenceCount);
            _project.GetEvaluatedIncludes().Returns(Enumerable.Empty<string>());

            bool result = _project!.HasNuGetPackagesReferenced();

            Assert.That(result, Is.False);
        }

        [TestCase(new string?[] { null }, false)]
        [TestCase(new string?[] { null, null }, false)]
        [TestCase(new string?[] { null, "not-packages.config" }, false)]
        [TestCase(new string?[] { "not-packages.config" }, false)]
        [TestCase(new string?[] { "not-packages.config", "other-not-packages.config" }, false)]
        [TestCase(new string?[] { "packages.config" }, true)]
        [TestCase(new string?[] { "packages.config", null }, true)]
        [TestCase(new string?[] { null, "packages.config" }, true)]
        [TestCase(new string?[] { "not-packages.config", "packages.config" }, true)]
        [TestCase(new string?[] { null, "not-packages.config", "packages.config" }, true)]
        public void HasPackagesConfigFile_Should_ReturnCorrectValue(string?[] evaluatedIncludes, bool expectedResult)
        {
            _project.GetEvaluatedIncludes().Returns(evaluatedIncludes);

            bool result = _project.HasPackagesConfigFile();

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase()]
        public void GetPackagesConfigPath_Should_Return_CorrectPath()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            _project.FullPath.Returns(path);

            string result = _project.GetPackagesConfigPath();

            Assert.That(result, Is.EqualTo(Path.Combine(Path.GetDirectoryName(path)!, "packages.config")));
        }
    }
}
