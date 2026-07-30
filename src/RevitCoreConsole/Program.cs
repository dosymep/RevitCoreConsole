using System.CommandLine;
using System.Net.Mime;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RevitCoreConsole.Resources;
using RevitCoreConsole.Services;
using RevitCoreConsole.CommandLine;

using RevitEngine.Abstractions;
using RevitEngine.AssemblyResolution;
using RevitEngine.Bundles;
using RevitEngine.Configuration;
using RevitEngine.DBApplications.Abstractions;
using RevitEngine.DBApplications.Manifests;
using RevitEngine.DBApplications.Runtime;
using RevitEngine.Documents;
using RevitEngine.Runtime;

using Serilog;
using Serilog.Debugging;

using Spectre.Console;

using Color = Spectre.Console.Color;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

// Define options for the command-line interface.
// These options mimic the Autodesk Revit Design Automation (Forge) environment parameters,
// allowing local testing and execution of DB applications in headless mode.

SelfLog.Enable(Console.Out);

var appName = new FigletText("Console") {
    Color = Color.Blue,
    Justification = Justify.Center,
};

string assVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "v1.0.0";

var version = new Text(
    $"Version {assVersion}",
    new Style(Color.Grey)) { Justification = Justify.Center };

AnsiConsole.Write(appName);
AnsiConsole.Write(version);
AnsiConsole.WriteLine();

// One log file per RevitCoreConsole process/session (no rolling).
string sessionLogPath = Path.Combine(
    "logs",
    $"RevitCoreConsole-{DateTime.Now:yyyyMMdd-HHmmss}-{Environment.ProcessId}.log");

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(sessionLogPath)
    .CreateLogger();

Log.Information("Session log file {SessionLogPath}", Path.GetFullPath(sessionLogPath));

// Option representing the input Revit document path.
// Maps to the "/i" parameter used by Revit Core Console.
Option<string> modelPathOption = new(name: "/input", aliases: ["/i"]) {
    Required = true,
    HelpName = Resources.ModelPathHelpName,
    Description = Resources.ModelPathDescription,
    CustomParser = CommandLineOptionParsers.ParseModelPath,
};

// Option representing the Design Automation AppBundle or add-in path.
// Maps to the "/al" parameter, which is required for loading the external DB application.
Option<FileInfo> bundlePathOption = new(name: "/app-bundle", aliases: ["/al"]) {
    Required = true,
    HelpName = Resources.BundlePathHelpName,
    Description = Resources.BundlePathDescription,
    CustomParser = CommandLineOptionParsers.ParseBundlePath,
};

// Option representing the Revit session language code.
// Maps to the "/l" parameter to configure the UI/API locale.
Option<LanguageCode> languageCodeOption = new(name: "/lang", aliases: ["/l"]) {
    Required = false,
    HelpName = Resources.LanguageCodeHelpName,
    Description = Resources.LanguageCodeDescription,
    CustomParser = CommandLineOptionParsers.ParseLanguageCode,
};

// Option controlling whether Revit audits the input document while opening it.
// The short alias is intended for compatibility with the Revit Core Console-style CLI.
Option<bool> auditOption = new(name: "/audit", aliases: ["/a"]) {
    Required = false,
    Description = Resources.AuditDescription,
};

// Option selecting which worksets Revit opens in the input document.
// The parser validates the value against the supported WorksetConfiguration definitions.
Option<WorksetConfiguration> worksetsOption = new(name: "/worksets", aliases: ["/w"]) {
    Required = false,
    HelpName = Resources.WorksetsHelpName,
    Description = Resources.WorksetsDescription,
    DefaultValueFactory = _ => WorksetConfiguration.OpenAllWorksets,
    CustomParser = CommandLineOptionParsers.ParseWorksetConfiguration,
};

// Register completion values from the same definitions used by the parser.
// This keeps suggestions synchronized with the accepted command-line values.
worksetsOption.CompletionSources.Add(
    [.. WorksetConfiguration.GetConfigurations().Select(configuration => configuration.Name)]);

// Register auto-completion values for all supported Revit language codes
languageCodeOption.CompletionSources.Add([.. LanguageCode.GetLanguageCodes().Select(item => item.Code)]);

// Define the root command of the CLI, explaining its purpose as a local Forge RevitCoreConsole equivalent.
RootCommand rootCommand = new(description: Resources.RootCommandDescription) {
    modelPathOption,
    bundlePathOption,
    languageCodeOption,
    auditOption,
    worksetsOption,
};

// Define the execution handler for the root command.
rootCommand.SetAction(parseResult => {
    string? modelPath = parseResult.GetValue(modelPathOption);
    var bundlePath = parseResult.GetValue(bundlePathOption);
    
    var languageCode = parseResult.GetValue(languageCodeOption);
    bool audit = parseResult.GetValue(auditOption);
    var worksets = parseResult.GetValue(worksetsOption);

    RunApsService(modelPath, bundlePath, languageCode, audit, worksets);
});

return await rootCommand.Parse(args).InvokeAsync();

void RunApsService(
    string? modelPath,
    FileInfo? bundlePath,
    LanguageCode? languageCode,
    bool audit,
    WorksetConfiguration? worksets) {
    try {
        Log.Information("Starting host");

        var openDocumentOptions = new OpenDocumentOptions {
            ModelPath = modelPath,
            Audit = audit,
            WorksetConfiguration = worksets?.Name,
        };

        ExecuteService<ApsService>(args, openDocumentOptions, bundlePath, languageCode);

        Log.Information("Host terminated gracefully");
    } catch(Exception ex) {
        Log.Fatal(ex, "Host terminated unexpectedly");
        throw;
    } finally {
        Log.CloseAndFlush();
    }
}

void ExecuteService<T>(
    string[] args,
    OpenDocumentOptions openDocumentOptions,
    FileInfo? bundlePath,
    LanguageCode? languageCode)
    where T : class, ISyncService {
    var builder = Host.CreateApplicationBuilder(args);

    var env = builder.Environment;

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

    if(languageCode is not null && languageCode != LanguageCode.Unknown) {
        builder.Configuration["RevitApplicationOptions:Startup:LanguageType"] = languageCode.FullCode;
    }
    
    builder.Services.AddSerilog(dispose: false);
    builder.Services.AddSingleton(openDocumentOptions);
    builder.Services.AddSingleton<ISyncService, T>();
    builder.Services.AddSingleton<IRevitApplicationHost, RevitApplicationHost>();
    builder.Services.AddSingleton<IRevitAssemblyResolver, RevitAssemblyResolver>();
    
    builder.Services.AddSingleton<IBundle, Bundle>();
    builder.Services.AddSingleton<IDesignAutomation, DesignAutomation>();
    
    builder.Services.AddSingleton<IDBApplicationFactory, DBApplicationFactory>();
    builder.Services.AddSingleton<IDBApplicationManifest, DBApplicationManifest>();
    
    builder.Services.AddOptions<BundleOptions>()
        .Configure(options => options.BundlePath = bundlePath!.FullName);
    
    builder.Services.AddOptions<RevitApplicationOptions>()
        .Bind(builder.Configuration.GetSection(nameof(RevitApplicationOptions)));

    using var host = builder.Build();
    host.Start();

    var revitAssemblyResolver = host.Services.GetRequiredService<IRevitAssemblyResolver>();
    revitAssemblyResolver.Install();

    try {
        var syncService = host.Services.GetRequiredService<ISyncService>();
        syncService.SyncExecute();
    } finally {
        revitAssemblyResolver.Uninstall();
    }
}
