// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using NuGetUtility.Wrapper.NuGetWrapper.Versioning;

namespace NuGetUtility.Wrapper.MsBuildWrapper
{
    public class MsBuildAbstraction : IMsBuildAbstraction
    {
        private const string CollectPackageReferences = "CollectPackageReferences";
        private readonly string? _buildConfiguration;
        private readonly string? _platform;
        private ProjectCollection? _projectCollection;

        public ProjectCollection ProjectCollection => _projectCollection ??= InitializeProjectCollection(_buildConfiguration, _platform);

        public MsBuildAbstraction(string? buildConfiguration = null, string? platform = null)
        {
            RegisterMsBuildLocatorIfNeeded();
            _buildConfiguration = buildConfiguration;
            _platform = platform;
        }

        public IEnumerable<PackageReference> GetPackageReferencesFromProjectForFramework(IProject project,
            string framework)
        {
            var globalProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TargetFramework", framework }
            };
            var newProject = new ProjectInstance(project.FullPath, globalProperties, null);
            newProject.Build(new[] { CollectPackageReferences }, new List<ILogger>(), out IDictionary<string, TargetResult>? targetOutputs);

            return targetOutputs.First(e => e.Key.Equals(CollectPackageReferences))
                .Value.Items.Select(p =>
                    new PackageReference(p.ItemSpec,
                        string.IsNullOrEmpty(p.GetMetadata("version"))
                            ? null
                            : new WrappedNuGetVersion(p.GetMetadata("version"))));
        }

        public IProject GetProject(string projectPath)
        {
#if !NETFRAMEWORK
            if (projectPath.EndsWith("vcxproj"))
            {
                throw new MsBuildAbstractionException($"Please use the .net Framework version to analyze c++ projects (Project: {projectPath})");
            }
#endif

            Project project = ProjectCollection.LoadProject(projectPath);

            return new ProjectWrapper(project);
        }

        public IEnumerable<string> GetProjectsFromSolution(string inputPath)
        {
            string absolutePath = Path.IsPathRooted(inputPath) ? inputPath : Path.Combine(Environment.CurrentDirectory, inputPath);
            var sln = SolutionFile.Parse(absolutePath);
            return sln.ProjectsInOrder.Select(p => p.AbsolutePath);
        }

        private static void RegisterMsBuildLocatorIfNeeded()
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        private static ProjectCollection InitializeProjectCollection(string? buildConfiguration, string? platform)
        {
            ProjectCollection projectCollection = ProjectCollection.GlobalProjectCollection;
            if (buildConfiguration != null)
            {
                projectCollection.SetGlobalProperty("Configuration", buildConfiguration);
            }
            if (platform != null)
            {
                projectCollection.SetGlobalProperty("Platform", platform);
            }
            return projectCollection;
        }
    }
}
