using System.IO.Compression;

using Autodesk.ExchangeStore;

using Microsoft.Extensions.Options;

using RevitEngine.Abstractions;
using RevitEngine.Configuration;
using RevitEngine.Runtime;

namespace RevitEngine.Bundles;

/// <summary>
/// Extracts components from a Design Automation bundle.
/// </summary>
public sealed class Bundle : IBundle {
    private readonly BundleOptions _options;

    /// <summary>
    /// Initializes a bundle service.
    /// </summary>
    /// <param name="options">The bundle options.</param>
    public Bundle(IOptions<BundleOptions> options) {
        _options = options.Value;
    }

    /// <inheritdoc/>
    public string ExtractBundleComponent() {
        string extractionPath = Path.Combine(Path.GetTempPath(), "RevitCoreConsole", "Bundles");
        ZipFile.ExtractToDirectory(_options.BundlePath, extractionPath);

        string? bundlePath = Directory.GetDirectories(extractionPath)
            .FirstOrDefault(static directoryPath =>
                directoryPath.EndsWith(".bundle", StringComparison.CurrentCultureIgnoreCase));

        if(string.IsNullOrEmpty(bundlePath)) {
            throw new InvalidOperationException("The bundle doesn't contain a .bundle folder.");
        }

        string contentPath = Path.Combine(bundlePath, "PackageContents.xml");
        RunTimeInfo runTimeInfo = new("Revit", "Win64", "R" + RevitApplicationHost.RevitVersion);
     
        var componentEntry = new RevitPackageContentsParser()
            .FindComponentsEntry(contentPath, runTimeInfo)
            .FirstOrDefault();

        if(componentEntry is null) {
            throw new InvalidOperationException(
                $"The bundle doesn't contain component for revit version {RevitApplicationHost.RevitVersion}.");
        }

        return componentEntry.ModuleName;
    }
}
