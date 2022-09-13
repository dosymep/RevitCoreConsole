using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;

using dosymep.Autodesk.FileInfo;
using dosymep.Revit.Engine;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace RevitCoreConsole.ConsoleCommands.Binders {
    internal abstract class BaseCommandBinder<T> : BinderBase<T>
        where T : BaseCommand, new() {
        protected override T GetBoundValue(BindingContext bindingContext) {
            T value = new T() {
                Logger = CreateLogger(),
                LanguageCode =
                    LanguageCode.GetLanguageCode(
                        bindingContext.ParseResult.GetValueForOption(BaseCommand.LanguageCodeOption)),
            };

            return GetBoundValueImpl(value, bindingContext);
        }

        protected abstract T GetBoundValueImpl(T value, BindingContext bindingContext);

        private ILogger CreateLogger() {
            var localFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RevitCoreConsole", RevitContext.RevitVersion, "RevitCoreConsole_.log");

            RollingInterval rollingInterval = RollingInterval.Infinite;
            int fileSizeLimitBytes = 500000000;
            bool rollOnFileSizeLimit = true;
            int retainedFileCountLimit = 99;
            string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {PluginName} "
                                    + "{{\"RevitVersion\": \"{RevitVersion}\" \"$type\": \"RevitCoreConsole\"}} "
                                    + "{Message}{NewLine}{Exception}";

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithProperty("PluginName", "RevitCoreConsole")
                .Enrich.WithProperty("RevitVersion", RevitContext.RevitVersion)
                .WriteTo.File(localFileName, rollingInterval: rollingInterval,
                    fileSizeLimitBytes: fileSizeLimitBytes, rollOnFileSizeLimit: rollOnFileSizeLimit,
                    retainedFileCountLimit: retainedFileCountLimit, outputTemplate: outputTemplate)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .MinimumLevel.Verbose();

            return loggerConfiguration.CreateLogger();
        }
    }
}