using Build.Helpers;
using Build.Options;
using Build.Versioning;

using Microsoft.Extensions.Options;

using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<RestoreModule>]
public sealed class BuildSampleModule : Module {
    private readonly BuildOptions _options;
    private readonly BuildVersionFactory _versions;

    public BuildSampleModule(IOptions<BuildOptions> options, BuildVersionFactory versions) {
        _options = options.Value;
        _versions = versions;
    }

    protected override Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        var git = context.Git().Information;

        return ParallelHelpers.ForEachAsync(
            GetSampleProjects(),
            cancellationToken,
            (projectPath, token) => new ValueTask(BuildSampleProjectAsync(context, git, projectPath, token)),
            _options.MaxDegreeOfParallelism);
    }

    private async Task BuildSampleProjectAsync(
        IModuleContext context,
        IGitInformation git,
        string projectPath,
        CancellationToken cancellationToken) {
        string pluginName = Path.GetFileNameWithoutExtension(projectPath);
        string packageContentsPath = Path.Combine(Path.GetDirectoryName(projectPath)!, "PackageContents.xml");

        foreach(string revitVersion in _options.RevitVersions) {
            await BuildSampleVersionAsync(
                context,
                git,
                projectPath,
                pluginName,
                packageContentsPath,
                revitVersion,
                cancellationToken);
        }
    }

    private async Task BuildSampleVersionAsync(
        IModuleContext context,
        IGitInformation git,
        string projectPath,
        string pluginName,
        string packageContentsPath,
        string revitVersion,
        CancellationToken cancellationToken) {
        string bundleDirectory = _options.GetSampleBundleDirectory(revitVersion, pluginName);
        string componentDirectory = _options.GetSampleComponentDirectory(revitVersion, pluginName);

        if(Directory.Exists(bundleDirectory)) {
            Directory.Delete(bundleDirectory, recursive: true);
        }

        Directory.CreateDirectory(bundleDirectory);
        File.Copy(packageContentsPath, Path.Combine(bundleDirectory, "PackageContents.xml"));

        await context.DotNet()
            .Build(
                new DotNetBuildOptions {
                    ProjectSolution = projectPath,
                    Configuration = _options.Configuration,
                    NoRestore = false,
                    NoIncremental = true,
                    Arguments = ["--force"],
                    Properties = [
                        ("RevitVersion", revitVersion),
                        ("OutputPath", FileSystemHelpers.EnsureTrailingSlash(componentDirectory)),
                        .._versions.PropertiesFor(git, revitVersion),
                    ]
                },
                cancellationToken: cancellationToken);
    }

    private IEnumerable<string> GetSampleProjects() {
        var excludedProjects = SolutionFileHelpers.GetExcludedProjectPaths(_options.SolutionPath);

        return Directory.GetFiles(_options.SamplesDirectory, "*.csproj", SearchOption.AllDirectories)
            .Where(projectPath => {
                string? projectDirectory = Path.GetDirectoryName(projectPath);
                if(projectDirectory is null
                   || !File.Exists(Path.Combine(projectDirectory, "PackageContents.xml"))) {
                    return false;
                }

                return !excludedProjects.Contains(Path.GetFullPath(projectPath));
            });
    }
}
