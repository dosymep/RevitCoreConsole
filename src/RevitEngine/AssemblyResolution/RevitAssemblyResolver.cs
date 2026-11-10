using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RevitEngine.Abstractions;
using RevitEngine.Configuration;

namespace RevitEngine.AssemblyResolution;

/// <summary>
/// Resolves Autodesk Revit assemblies from the configured installation directory.
/// </summary>
public sealed partial class RevitAssemblyResolver : IRevitAssemblyResolver, IDisposable {
    private bool _isInstalled;
    private string? _previousDirectory;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a resolver for the configured Revit installation.
    /// </summary>
    /// <param name="logger">Logger used for resolver lifecycle and resolution events.</param>
    /// <param name="options">Configured Revit context options.</param>
    public RevitAssemblyResolver(
        ILogger<RevitAssemblyResolver> logger,
        IOptions<RevitApplicationOptions> options) {
        _logger = logger;
        ApplicationPath = options.Value.ApplicationPath;
    }

    /// <summary>
    /// Gets the Revit installation directory used for assembly resolution.
    /// </summary>
    public string ApplicationPath { get; }

    /// <summary>
    /// Updates environment paths.
    /// </summary>
    public void Install() {
        if(_isInstalled) {
            LogInstallSkipped(_logger, ApplicationPath);
            return;
        }

        LogInstallStarted(_logger, ApplicationPath);
        ValidateApplicationPath();
        AddToPath();

        _previousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(ApplicationPath);

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        _isInstalled = true;
        LogInstallCompleted(_logger, ApplicationPath);
    }

    /// <summary>
    /// Removes the resolver from the current application domain.
    /// </summary>
    public void Uninstall() {
        if(!_isInstalled) {
            LogUninstallSkipped(_logger, ApplicationPath);
            return;
        }

        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        RestorePreviousDirectory();
        _isInstalled = false;
        LogUninstallCompleted(_logger, ApplicationPath);
    }

    private void RestorePreviousDirectory() {
        if(_previousDirectory is null) {
            return;
        }

        if(Directory.Exists(_previousDirectory)) {
            Directory.SetCurrentDirectory(_previousDirectory);
        }

        _previousDirectory = null;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args) {
        var assemblyName = new AssemblyName(args.Name);
        string assemblyPath = Path.Combine(ApplicationPath, assemblyName.Name + ".dll");

        if(!File.Exists(assemblyPath)) {
            LogAssemblyNotFound(_logger, assemblyName.Name, assemblyPath);
            return null;
        }

        LogAssemblyResolved(_logger, assemblyName.Name, assemblyPath);
        return Assembly.LoadFrom(assemblyPath);
    }

    private void AddToPath() {
        string? currentPath = Environment.GetEnvironmentVariable("PATH");

        bool? contains = currentPath?
            .Split(Path.PathSeparator)
            .Contains(ApplicationPath, StringComparer.OrdinalIgnoreCase);

        if(contains == true) {
            LogPathAlreadyConfigured(_logger, ApplicationPath);
            return;
        }

        Environment.SetEnvironmentVariable("PATH", $"{currentPath}{Path.PathSeparator}{ApplicationPath}");
        LogPathUpdated(_logger, ApplicationPath);
    }

    private void ValidateApplicationPath() {
        if(string.IsNullOrWhiteSpace(ApplicationPath)) {
            LogApplicationPathInvalid(_logger, "The application path is empty.");
            throw new InvalidOperationException(
                "Revit application path is not configured.");
        }

        if(!Directory.Exists(ApplicationPath)) {
            LogApplicationPathInvalid(_logger, "The application directory does not exist.");
            throw new DirectoryNotFoundException(ApplicationPath);
        }

        string apiPath = Path.Combine(ApplicationPath, "RevitAPI.dll");
        string netPath = Path.Combine(ApplicationPath, "RevitNET.dll");

        if(!File.Exists(apiPath)
           || !File.Exists(netPath)) {
            LogApplicationPathInvalid(_logger, "Required Revit API assemblies are missing.");
            throw new InvalidOperationException(
                $"The directory '{ApplicationPath}' is not a Revit installation directory.");
        }
    }

    #region IDisposable

    /// <inheritdoc />
    public void Dispose() {
        Uninstall();
    }

    #endregion

    [LoggerMessage(LogLevel.Information, "Starting Revit assembly resolver installation for {ApplicationPath}.")]
    private static partial void LogInstallStarted(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Information, "Revit assembly resolver installed for {ApplicationPath}.")]
    private static partial void LogInstallCompleted(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Debug, "Revit assembly resolver installation skipped because it is already installed for {ApplicationPath}.")]
    private static partial void LogInstallSkipped(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Information, "Revit assembly resolver uninstalled for {ApplicationPath}.")]
    private static partial void LogUninstallCompleted(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Debug, "Revit assembly resolver uninstallation skipped because it is not installed for {ApplicationPath}.")]
    private static partial void LogUninstallSkipped(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Debug, "PATH already contains the Revit application path {ApplicationPath}.")]
    private static partial void LogPathAlreadyConfigured(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Information, "Added Revit application path {ApplicationPath} to PATH.")]
    private static partial void LogPathUpdated(ILogger logger, string applicationPath);

    [LoggerMessage(LogLevel.Debug, "Revit application path validation failed: {Reason}")]
    private static partial void LogApplicationPathInvalid(ILogger logger, string reason);

    [LoggerMessage(LogLevel.Debug, "Resolved Revit assembly {AssemblyName} from {AssemblyPath}.")]
    private static partial void LogAssemblyResolved(ILogger logger, string? assemblyName, string assemblyPath);

    [LoggerMessage(LogLevel.Debug, "Could not resolve Revit assembly {AssemblyName}; file not found at {AssemblyPath}.")]
    private static partial void LogAssemblyNotFound(ILogger logger, string? assemblyName, string assemblyPath);
}
