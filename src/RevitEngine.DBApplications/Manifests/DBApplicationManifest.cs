using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using RevitEngine.DBApplications.Abstractions;

namespace RevitEngine.DBApplications.Manifests;

/// <summary>
/// Default implementation of <see cref="IDBApplicationManifest"/> that parses standard Revit .addin XML files.
/// </summary>
public sealed partial class DBApplicationManifest : IDBApplicationManifest {
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBApplicationManifest"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic messages.</param>
    public DBApplicationManifest(ILogger<DBApplicationManifest> logger) {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public DBApplicationInfo Parse(string manifestPath) {
        if(string.IsNullOrWhiteSpace(manifestPath)) {
            LogManifestError(_logger, "The manifest path is empty.");
            throw new ArgumentException("The manifest path cannot be empty.", nameof(manifestPath));
        }

        LogManifestLoading(_logger, manifestPath);
        var addinManifest = XDocument.Load(manifestPath);
        var dbApplicationEntry = addinManifest
            .Descendants("AddIn")
            .FirstOrDefault(static addInElement =>
                string.Equals(
                    (string?) addInElement.Attribute("Type"),
                    "DBApplication",
                    StringComparison.OrdinalIgnoreCase));

        if(dbApplicationEntry is null) {
            LogManifestError(_logger, manifestPath);
            throw new InvalidOperationException(
                $"The bundle doesn't contain manifest module \"{manifestPath}\".");
        }

        string? assemblyPath = dbApplicationEntry.Element("Assembly")?.Value.Trim();
        if(string.IsNullOrWhiteSpace(assemblyPath)) {
            LogManifestError(_logger, "The .addin manifest doesn't contain an Assembly element.");
            throw new InvalidOperationException("The .addin manifest doesn't contain an Assembly element.");
        }

        string? fullClassName = dbApplicationEntry.Element("FullClassName")?.Value.Trim();
        if(string.IsNullOrWhiteSpace(fullClassName)) {
            LogManifestError(_logger, "The .addin manifest doesn't contain a FullClassName element.");
            throw new InvalidOperationException("The .addin manifest doesn't contain a FullClassName element.");
        }

        string fullAssemblyPath = Path.GetFullPath(
            Path.Combine(
                Path.GetDirectoryName(manifestPath)!,
                assemblyPath));
        LogManifestEntry(_logger, fullClassName, fullAssemblyPath);

        if(!File.Exists(fullAssemblyPath)) {
            LogAssemblyError(_logger, fullAssemblyPath);
            throw new FileNotFoundException(
                $"The assembly specified by the .addin manifest was not found: \"{fullAssemblyPath}\".",
                fullAssemblyPath);
        }

        return new DBApplicationInfo() {
            FullClassName = fullClassName!,
            FullAssemblyPath = fullAssemblyPath
        };
    }

    [LoggerMessage(LogLevel.Debug, "Loading .addin manifest from {ManifestPath}.")]
    private static partial void LogManifestLoading(ILogger logger, string manifestPath);

    [LoggerMessage(LogLevel.Debug, "Resolved .addin entry {FullClassName} from assembly {AssemblyPath}.")]
    private static partial void LogManifestEntry(ILogger logger, string? fullClassName, string assemblyPath);
    
    [LoggerMessage(LogLevel.Error, "DB application manifest error: {Message}.")]
    private static partial void LogManifestError(ILogger logger, string message);
    
    [LoggerMessage(LogLevel.Error, "DB application assembly was not found at {AssemblyPath}.")]
    private static partial void LogAssemblyError(ILogger logger, string assemblyPath);
}
