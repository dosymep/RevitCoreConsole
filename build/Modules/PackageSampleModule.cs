using Build.Helpers;
using Build.Options;

using Microsoft.Extensions.Options;

using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<BuildSampleModule>]
public sealed class PackageSampleModule : Module {
    private readonly BuildOptions _options;

    public PackageSampleModule(IOptions<BuildOptions> options) {
        _options = options.Value;
    }

    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        Directory.CreateDirectory(_options.ArtifactsDirectory);

        string stagingRoot = Path.Combine(_options.ArtifactsDirectory, ".staging");
        try {
            await ParallelHelpers.ForEachAsync(
                GetBuiltPluginNames(),
                cancellationToken,
                (pluginName, token) => new ValueTask(PackagePluginAsync(stagingRoot, pluginName, token)),
                _options.MaxDegreeOfParallelism);
        } finally {
            if(Directory.Exists(stagingRoot)) {
                Directory.Delete(stagingRoot, recursive: true);
            }
        }
    }

    private async Task PackagePluginAsync(
        string stagingRoot,
        string pluginName,
        CancellationToken cancellationToken) {
        string bundleDirectory = Path.Combine(stagingRoot, $"{pluginName}.bundle");
        Directory.CreateDirectory(bundleDirectory);

        foreach(string revitVersion in _options.RevitVersions) {
            StagePluginVersion(bundleDirectory, pluginName, revitVersion);
        }

        await FileSystemHelpers.CreateZipAsync(
            bundleDirectory,
            _options.GetSampleArchivePath(pluginName),
            cancellationToken);
    }

    private void StagePluginVersion(string bundleDirectory, string pluginName, string revitVersion) {
        string versionBundleDirectory = _options.GetSampleBundleDirectory(revitVersion, pluginName);
        if(!Directory.Exists(versionBundleDirectory)) {
            return;
        }

        string manifestPath = Path.Combine(versionBundleDirectory, "PackageContents.xml");
        string stagedManifestPath = Path.Combine(bundleDirectory, "PackageContents.xml");
        if(File.Exists(manifestPath) && !File.Exists(stagedManifestPath)) {
            File.Copy(manifestPath, stagedManifestPath);
        }

        string componentDirectory = Path.Combine(versionBundleDirectory, revitVersion);
        if(Directory.Exists(componentDirectory)) {
            FileSystemHelpers.CopyDirectory(
                componentDirectory,
                Path.Combine(bundleDirectory, revitVersion));
        }
    }

    private IEnumerable<string> GetBuiltPluginNames() {
        var pluginNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach(string revitVersion in _options.RevitVersions) {
            string versionBinDirectory = Path.Combine(_options.BinDirectory, revitVersion);
            if(!Directory.Exists(versionBinDirectory)) {
                continue;
            }

            foreach(string bundleDirectory in Directory.GetDirectories(versionBinDirectory, "*.bundle")) {
                string bundleName = Path.GetFileName(bundleDirectory);
                pluginNames.Add(bundleName[..^".bundle".Length]);
            }
        }

        return pluginNames.Order(StringComparer.OrdinalIgnoreCase);
    }
}
