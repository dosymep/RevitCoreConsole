using System;
using System.CommandLine;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Autodesk;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine;
using dosymep.Revit.Engine.Pipelines;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;
using dosymep.SimpleServices;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class PipelineCommand : BaseCommand<RevitContext> {
        public static readonly Option<string> PipelineFileOption
            = new Option<string>(
                name: "/pipeline",
                description: "Pipeline file path.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("pipeline")
                .AddParam(PipelineFileOption)
                .SetHandler(new PipelineCommandBinder())
                .SetDescription("Pipeline command");

        public string PipelineFile { get; set; }

        protected override void ExecuteImpl(RevitContext context) {
            Logger.Information("Executing PipelineCommand {@PipelineCommand}", this);
            try {
                RevitPipeline pipeline = RevitPipeline.CreateRevitPipeline(PipelineFile);
                Logger.Debug("Loaded pipeline {@RevitPipelineSteps}", pipeline.StepOptions);

                context.OpenDocument(pipeline.OpenModelOptions);
                Logger.Debug("Opened Document {@OpenModelOptions}", pipeline.OpenModelOptions);

                var transformer = new RevitExternalTransformer(pipeline.OpenModelOptions.ModelPath, context);
                var stepOptions = pipeline.StepOptions
                    .Select(item => PipelineOptions.CreateOption(item, transformer));

                ServicesProvider.LoadInstanceCore(context.Application);
                foreach(PipelineOptions pipelineOption in stepOptions) {
                    pipelineOption.Value.ExecuteExternalItem(pipelineOption.Options.WithOptions);
                }
            } finally {
                Logger.Information("Executed PipelineCommand {@PipelineCommand}", this);
            }
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
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