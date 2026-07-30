using Build.Helpers;
using Build.Options;

using Microsoft.Extensions.Options;

using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<BuildModule>]
public sealed class PackageModule : Module {
    private readonly BuildOptions _options;

    public PackageModule(IOptions<BuildOptions> options) {
        _options = options.Value;
    }

    protected override Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        Directory.CreateDirectory(_options.ArtifactsDirectory);

        return ParallelHelpers.ForEachAsync(
            _options.RevitVersions,
            cancellationToken,
            (revitVersion, token) => new ValueTask(PackageConsoleAsync(revitVersion, token)),
            _options.MaxDegreeOfParallelism);
    }

    private Task PackageConsoleAsync(string revitVersion, CancellationToken cancellationToken) {
        return FileSystemHelpers.CreateZipAsync(
            _options.GetConsoleOutputDirectory(revitVersion),
            _options.GetConsoleArchivePath(revitVersion),
            cancellationToken);
    }
}
