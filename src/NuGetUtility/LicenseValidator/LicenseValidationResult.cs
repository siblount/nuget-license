// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.LicenseValidator
{
    public record LicenseValidationResult(string PackageId,
        INuGetVersion PackageVersion,
        string? PackageProjectUrl,
        string? License,
        LicenseInformationOrigin LicenseInformationOrigin,
        List<ValidationError>? ValidationErrors = null)
    {
        public List<ValidationError> ValidationErrors { get; } = ValidationErrors ?? new List<ValidationError>();

        public string? License { get; set; } = License;
        public LicenseInformationOrigin LicenseInformationOrigin { get; set; } = LicenseInformationOrigin;
    }
}
