// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.ComponentModel;

namespace NuGetUtility;

public enum OutputColumnType
{
    [Description("Package")]
    Package,
    PackageId = Package, // Json name
    [Description("Version")]
    Version,
    PackageVersion = Version, // Json name
    [Description("License Information Origin")]
    LicenseInformationOrigin,
    [Description("License Expression")]
    LicenseExpression,
    [Description("License Url")]
    LicenseUrl,
    [Description("Copyright")]
    Copyright,
    [Description("Authors")]
    Authors,
    [Description("Package Project Url")]
    PackageProjectUrl,
    [Description("Error")]
    Error,
    ValidationErrors = Error, // Json name, group "Error" and "ErrorContext"
    [Description("Error Context")]
    ErrorContext,
}
