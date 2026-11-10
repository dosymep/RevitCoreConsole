using System.Reflection;

using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RevitEngine.Abstractions;
using RevitEngine.Configuration;
using RevitEngine.Extensions;

namespace RevitEngine.Runtime;

/// <summary>
/// Initializes and closes a Revit application instance.
/// </summary>
public sealed partial class RevitApplicationHost : IRevitApplicationHost, IDisposable {
    private readonly ILogger _logger;
    private readonly RevitApplicationOptions _options;

    /// <summary>
    /// Creates revit application.
    /// </summary>
    public RevitApplicationHost(ILogger<RevitApplicationHost> logger, IOptions<RevitApplicationOptions> options) {
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the version of Revit targeted by the current application instance.
    /// </summary>
    public string Version => RevitVersion;

    /// <summary>
    /// Gets the Revit version targeted by the current build.
    /// </summary>
#if REVIT2016
    public const string RevitVersion = "2016";
#elif REVIT2017
    public const string RevitVersion = "2017";
#elif REVIT2018
    public const string RevitVersion = "2018";
#elif REVIT2019
    public const string RevitVersion = "2019";
#elif REVIT2020
    public const string RevitVersion = "2020";
#elif REVIT2021
    public const string RevitVersion = "2021";
#elif REVIT2022
    public const string RevitVersion = "2022";
#elif REVIT2023
    public const string RevitVersion = "2023";
#elif REVIT2024
    public const string RevitVersion = "2024";
#elif REVIT2025
    public const string RevitVersion = "2025";
#elif REVIT2026
    public const string RevitVersion = "2026";
#elif REVIT2027
    public const string RevitVersion = "2027";
#endif

    /// <summary>
    /// Gets the installation directory of the Revit application.
    /// </summary>
    public string ApplicationPath => _options.ApplicationPath;

    /// <summary>
    /// Gets the initialized Revit product, or <see langword="null"/> before initialization and after closing.
    /// </summary>
    public Product? RevitProduct { get; private set; }

    /// <summary>
    /// Initializes Revit and returns its application API object.
    /// </summary>
    /// <returns>The initialized Revit application.</returns>
    public Application Initialize() {
        LogInitializationStarted(_logger, RevitVersion, _options.ApplicationPath);

        RevitProduct = Product.GetInstalledProduct();
        RevitProduct.ApplyStartupSettings(_options.Startup);

        var clientId = new ClientApplicationId(_options.ApplicationGuid, _options.ApplicationName, _options.VendorId);
        clientId.VendorDescription = _options.VendorDescription;
        clientId.SetClientApplicationVer(
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0");

        try {
#if REVIT2022_OR_GREATER
            RevitProduct.Initialize_ForAutodeskInternalUseOnly(clientId, _options.LicenseKey);
#else
            RevitProduct.Init(clientId, _options.LicenseKey);
#endif
        } catch(global::Autodesk.Revit.Exceptions.ArgumentException ex) when(ex.ParamName.Equals("clientData")) {
            LogInitializationFailed(_logger, ex, RevitVersion);
            throw new InvalidOperationException($"The \"{_options.LicenseKey}\" license key is not valid.", ex);
        } catch(global::Autodesk.Revit.Exceptions.ArgumentException ex) when(ex.ParamName.Equals("id")) {
            LogInitializationFailed(_logger, ex, RevitVersion);
            throw new InvalidOperationException($"The \"{_options.LicenseKey}\" license key is not valid.", ex);
        }

        if(_options.Startup.ApplyApiSettings) {
            RevitProduct.ApplyApiSettings(_options.Startup.Api);
        }

        LogInitializationCompleted(_logger, RevitVersion);
        return RevitProduct.Application;
    }

    /// <summary>
    /// Closes the initialized Revit application.
    /// </summary>
    public void Close() {
        if(RevitProduct is null) {
            LogCloseSkipped(_logger);
            return;
        }

        LogClosing(_logger, RevitVersion);
        RevitProduct?.Exit();
        RevitProduct?.Dispose();
        RevitProduct = null;
        LogClosed(_logger, RevitVersion);
    }

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose() {
        Close();
    }

    #endregion

    [LoggerMessage(LogLevel.Information, "Starting Revit {RevitVersion} initialization from {ApplicationPath}.")]
    private static partial void LogInitializationStarted(ILogger logger, string revitVersion, string applicationPath);

    [LoggerMessage(LogLevel.Information, "Revit {RevitVersion} initialized successfully.")]
    private static partial void LogInitializationCompleted(ILogger logger, string revitVersion);

    [LoggerMessage(LogLevel.Error, "Revit {RevitVersion} initialization failed because the client data is invalid.")]
    private static partial void LogInitializationFailed(ILogger logger, Exception exception, string revitVersion);

    [LoggerMessage(LogLevel.Information, "Closing Revit {RevitVersion}.")]
    private static partial void LogClosing(ILogger logger, string revitVersion);

    [LoggerMessage(LogLevel.Information, "Revit {RevitVersion} closed successfully.")]
    private static partial void LogClosed(ILogger logger, string revitVersion);

    [LoggerMessage(LogLevel.Debug, "Revit context close skipped because it was not initialized.")]
    private static partial void LogCloseSkipped(ILogger logger);
}
