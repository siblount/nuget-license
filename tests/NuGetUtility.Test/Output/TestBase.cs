// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using Bogus;
using NuGetUtility.LicenseValidator;
using NuGetUtility.Output;
using NuGetUtility.Test.Extensions;
using NuGetUtility.Test.Helper.ShuffelledEnumerable;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Test.Output
{
    public abstract class TestBase
    {
        protected TestBase(bool includeCopyright, bool includeAuthors, bool includeLicenseUrl)
        {
            _includeCopyright = includeCopyright;
            _includeAuthors = includeAuthors;
            _includeLicenseUrl = includeLicenseUrl;

            OmittedOutputFieldsFaker = new Faker<IEnumerable<LicenseValidationResultProperties>>().CustomInstantiator(f => GetOmittedFields(f)).UseSeed(1234567);
        }

        protected Faker<IEnumerable<LicenseValidationResultProperties>> OmittedOutputFieldsFaker;
        protected Faker<LicenseValidationResult> LicenseValidationErrorFaker = null!;
        protected Faker<LicenseValidationResult> ValidatedLicenseFaker = null!;
        private readonly bool _includeCopyright;
        private readonly bool _includeAuthors;
        private readonly bool _includeLicenseUrl;
        private readonly VerifySettings _verifySettings = new VerifySettings();

        [SetUp]
        public void SetUp()
        {
            ValidatedLicenseFaker = new Faker<LicenseValidationResult>().CustomInstantiator(f =>
                    new LicenseValidationResult(f.Name.JobTitle(),
                        new NuGetVersion(f.System.Semver()),
                        GetNullable(f, f.Internet.Url),
                        GetNullable(f, f.Hacker.Phrase),
                        _includeLicenseUrl ? GetNullable(f, f.Hacker.Phrase) : null,
                        _includeCopyright ? GetNullable(f, f.Hacker.Phrase) : null,
                        _includeAuthors ? GetNullable(f, () => string.Join(",", Enumerable.Repeat(true, f.Random.Int(0, 10)).Select(_ => f.Person.FullName))) : null,
                        f.Random.Enum<LicenseInformationOrigin>()))
                .UseSeed(8675309);
            LicenseValidationErrorFaker = new Faker<LicenseValidationResult>().CustomInstantiator(f =>
                    new LicenseValidationResult(f.Name.JobTitle(),
                        new NuGetVersion(f.System.Semver()),
                        GetNullable(f, f.Internet.Url),
                        GetNullable(f, f.Hacker.Phrase),
                        _includeLicenseUrl ? GetNullable(f, f.Hacker.Phrase) : null,
                        _includeCopyright ? GetNullable(f, f.Hacker.Phrase) : null,
                        _includeAuthors ? GetNullable(f, () => string.Join(",", Enumerable.Repeat(true, f.Random.Int(0, 10)).Select(_ => f.Person.FullName))) : null,
                        f.Random.Enum<LicenseInformationOrigin>(),
                        GetErrorList(f).ToList()))
                .UseSeed(9078345);
            _verifySettings.UseDirectory("VerifyResults");
        }
        protected abstract IOutputFormatter CreateUut(IEnumerable<LicenseValidationResultProperties> ignoredFields);

        private static T? GetNullable<T>(Faker faker, Func<T> getter) where T : class
        {
            if (faker.Random.Bool())
            {
                return null;
            }
            return getter();
        }

        private static IEnumerable<ValidationError> GetErrorList(Faker faker)
        {
            int itemCount = faker.Random.Int(3, 10);
            for (int i = 0; i < itemCount; i++)
            {
                yield return new ValidationError(faker.Name.FirstName(), faker.Internet.Url());
            }
        }

        private static IEnumerable<LicenseValidationResultProperties> GetOmittedFields(Faker faker)
        {
            foreach (LicenseValidationResultProperties type in Enum.GetValues(typeof(LicenseValidationResultProperties)))
            {
                if (faker.Random.Bool())
                {
                    yield return type;
                }
            }
        }

        [Test]
        public async Task ValidatedLicensesWithErrors_Should_PrintCorrectOutput(
            [Values(0, 1, 5, 20, 100)] int validCount,
            [Values(1, 3, 5, 20)] int errorCount)
        {
            IOutputFormatter uut = CreateUut(OmittedOutputFieldsFaker.Generate());
            using var stream = new MemoryStream();
            var result = LicenseValidationErrorFaker.GenerateForever()
                .Take(errorCount)
                .Concat(ValidatedLicenseFaker.GenerateForever().Take(validCount))
                .Shuffle(971234)
                .ToList();
            await uut.Write(stream, result);

            await Verify(stream.AsString(), _verifySettings).HashParameters();
        }

        [Test]
        public async Task ValidatedLicenses_Should_PrintCorrectOutput(
            [Values(0, 1, 5, 20, 100)] int validatedLicenseCount)
        {
            IOutputFormatter uut = CreateUut(Enumerable.Empty<LicenseValidationResultProperties>());
            using var stream = new MemoryStream();
            var validated = ValidatedLicenseFaker.GenerateForever().Take(validatedLicenseCount).ToList();
            await uut.Write(stream, validated);

            await Verify(stream.AsString(), _verifySettings).HashParameters();
        }

        [Test]
        public async Task ValidatedLicensesWithOmittedValues_Should_PrintCorrectOutput(
            [Values(0, 1, 5, 20, 100)] int validCount,
            [Values(0, 1, 3, 5, 20)] int errorCount)
        {
            IOutputFormatter uut = CreateUut(OmittedOutputFieldsFaker.Generate());
            using var stream = new MemoryStream();
            var result = LicenseValidationErrorFaker.GenerateForever()
                .Take(errorCount)
                .Concat(ValidatedLicenseFaker.GenerateForever().Take(validCount))
                .Shuffle(971234)
                .ToList();
            await uut.Write(stream, result);

            await Verify(stream.AsString(), _verifySettings).HashParameters();
        }

        private class NuGetVersion : INuGetVersion
        {
            private readonly string _version;

            public NuGetVersion(string version)
            {
                _version = version;
            }

            public int CompareTo(INuGetVersion? other) => throw new NotImplementedException();

            public override string ToString()
            {
                return _version;
            }
        }
    }
}
