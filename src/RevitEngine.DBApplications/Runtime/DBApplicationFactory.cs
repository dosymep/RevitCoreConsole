using System.Reflection;

using Autodesk.Revit.DB;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RevitEngine.DBApplications.Abstractions;
using RevitEngine.DBApplications.Manifests;

namespace RevitEngine.DBApplications.Runtime;

/// <summary>
/// Factory service that loads, instantiates, and configures <see cref="IExternalDBApplication"/> implementations.
/// </summary>
public sealed partial class DBApplicationFactory : IDBApplicationFactory {
    private readonly ILogger _logger;
    private readonly IDBApplicationManifest _manifest;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBApplicationFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostics.</param>
    /// <param name="manifest">The manifest parser service.</param>
    /// <param name="serviceProvider">The application service provider used to resolve dependencies for created DB applications.</param>
    public DBApplicationFactory(
        ILogger<DBApplicationFactory> logger,
        IDBApplicationManifest manifest,
        IServiceProvider serviceProvider) {
        _logger = logger;
        _manifest = manifest;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IExternalDBApplication Create(string componentPath) {
        var info = _manifest.Parse(componentPath);

        var assembly = Assembly.LoadFrom(info.FullAssemblyPath);
        LogAssemblyLoaded(_logger, info.FullAssemblyPath);

        var applicationType = assembly.GetType(info.FullClassName, throwOnError: true, ignoreCase: false)
                              ?? throw new InvalidOperationException(
                                  $"The type \"{info.FullClassName}\" was not found in \"{info.FullAssemblyPath}\".");

        if(!typeof(IExternalDBApplication).IsAssignableFrom(applicationType)) {
            LogApplicationTypeError(_logger, info.FullClassName);
            throw new InvalidOperationException(
                $"The type \"{info.FullClassName}\" must implement {nameof(IExternalDBApplication)}.");
        }

        var dbApplication = CreateDBApplication(info, applicationType);
        LogApplicationCreated(_logger, info.FullClassName);

        return dbApplication;
    }

    private IExternalDBApplication CreateDBApplication(DBApplicationInfo info, Type applicationType) {
        string appConfig = Path.ChangeExtension(info.FullAssemblyPath, ".json");
        if(File.Exists(appConfig)) {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(appConfig, optional: false, reloadOnChange: true)
                .Build();

            return (IExternalDBApplication) ActivatorUtilities.CreateInstance(_serviceProvider, applicationType, configuration)!;
        }

        return (IExternalDBApplication) ActivatorUtilities.CreateInstance(_serviceProvider, applicationType)!;
    }

    [LoggerMessage(LogLevel.Debug, "Loaded DB application assembly {AssemblyPath}.")]
    private static partial void LogAssemblyLoaded(ILogger logger, string assemblyPath);

    [LoggerMessage(LogLevel.Information, "Created DB application {FullClassName}.")]
    private static partial void LogApplicationCreated(ILogger logger, string? fullClassName);

    [LoggerMessage(LogLevel.Error, "Configured DB application type {FullClassName} does not implement IExternalDBApplication.")]
    private static partial void LogApplicationTypeError(ILogger logger, string? fullClassName);
}
