using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Autodesk.ExchangeStore;
using Autodesk.Revit.DB;

using dosymep.Autodesk;
using dosymep.Revit.Engine.CoreCommands;
using dosymep.Revit.Engine.Pipelines;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;
using dosymep.SimpleServices;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace dosymep.Revit.Engine {
    /// <summary>
    /// Revit context extensions.
    /// </summary>
    public static class RevitContextExtensions {
        public static void ExecutePipelineCommand(this IRevitContext revitContext, IPipelineCommand pipelineCommand) {
            ILoggerService loggerService = revitContext.GetPlatformService<ILoggerService>();
            
            RevitPipeline pipeline = RevitPipeline.CreateRevitPipeline(pipelineCommand.PipelineFile);
            loggerService.Debug("Loaded pipeline {@RevitPipelineSteps}", pipeline.StepOptions);

            revitContext.OpenDocument(pipeline.OpenModelOptions);
            loggerService.Debug("Opened Document {@OpenModelOptions}", pipeline.OpenModelOptions);

            var transformer = new RevitExternalTransformer(pipeline.OpenModelOptions.ModelPath, revitContext);
            var stepOptions = pipeline.StepOptions
                .Select(item => PipelineOptions.CreateOption(item, transformer));

            foreach(PipelineOptions pipelineOption in stepOptions) {
                pipelineOption.Value.ExecuteExternalItem(pipelineOption.Options.WithOptions);
            }
        }
        
        public static void ExecuteRevitCommand(this IRevitContext revitContext, IRevitCommand revitCommand) {
            IRevitExternalItem revitExternalItem =
                revitCommand.RevitAddinItem.Reduce<IRevitExternalItem, RevitAddinItem>(
                    new RevitExternalTransformer(revitCommand.ModelPath, revitContext));
            revitExternalItem.ExecuteExternalItem(ReadJournalData(revitCommand.JournalData));
        }

        public static void ExecuteForgeCommand(this IRevitContext revitContext, IForgeCommand forgeCommand) {
            ILoggerService loggerService = revitContext.GetPlatformService<ILoggerService>();

            var tempName = Path.Combine(Path.GetTempPath(), "RevitCoreConsole", "Bundles");
            ZipFile.ExtractToDirectory(forgeCommand.BundlePath, tempName);
            loggerService.Debug("Extracted bundle to {@TempName}", tempName);

            string bundlePath = Directory.GetDirectories(tempName)
                .FirstOrDefault(item =>
                    item.EndsWith(".bundle", StringComparison.CurrentCultureIgnoreCase));

            if(string.IsNullOrEmpty(bundlePath)) {
                throw new InvalidOperationException("The bundle doesn't contain .bundle folder.");
            }

            try {
                var contentPath = Path.Combine(bundlePath, "PackageContents.xml");
                var component = new RevitPackageContentsParser()
                    .FindComponentsEntry(contentPath, forgeCommand.RunTimeInfo)
                    .FirstOrDefault();

                if(component is null) {
                    throw new InvalidOperationException(
                        $"The bundle doesn't contain component for revit version {RevitContext.RevitVersion}.");
                }

                loggerService.Debug("Loaded component {@Component}", component);
                var dbapplication = RevitAddinManifest.GetAddinManifest(component.ModuleName)
                    .AddinDBApplications.FirstOrDefault();

                if(dbapplication is null) {
                    throw new InvalidOperationException(
                        $"The bundle doesn't contain component module \"{component.ModuleName}\".");
                }

                loggerService.Debug("Loaded DBApplication {@DBApplication}", dbapplication);
                IRevitExternalItem revitExternalItem =
                    dbapplication.Reduce<IRevitExternalItem, RevitAddinItem>(
                        new RevitExternalTransformer(forgeCommand.ModelPath, revitContext));
                revitExternalItem.ExecuteExternalItem(new Dictionary<string, string>());
            } finally {
                try {
                    Directory.Delete(tempName, true);
                    loggerService.Debug("Removed extracted bundle {@TempName}", tempName);
                } catch(Exception ex) {
                    loggerService.Debug(ex, "Can't remove extracted bundle {@TempName}", tempName);
                }
            }
        }

        public static IDictionary<string, string> ReadJournalData(string journalData) {
            if(string.IsNullOrEmpty(journalData)) {
                return new Dictionary<string, string>();
            }

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<Dictionary<string, string>>(journalData);
        }
        
        private class PipelineOptions {
            public static PipelineOptions CreateOption(PipelineStepOptions options,
                RevitExternalTransformer transformer) {
                return new PipelineOptions() {
                    Options = options, Value = GetRevitExternalItem(options.UsesName, transformer)
                };
            }

            public IRevitExternalItem Value { get; set; }
            public PipelineStepOptions Options { get; set; }

            private static IRevitExternalItem GetRevitExternalItem(string usesName,
                RevitExternalTransformer transformer) {
                var revitAddinItem =
                    RevitAddinManifest.GetAddinManifest(Path.Combine("plugins", usesName + ".addin"))
                        .AddinItems.FirstOrDefault();
                return revitAddinItem?.Reduce<IRevitExternalItem, RevitAddinItem>(transformer);
            }
        }
    }
}