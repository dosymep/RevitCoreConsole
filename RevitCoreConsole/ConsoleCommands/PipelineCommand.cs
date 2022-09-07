using System.CommandLine;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Autodesk;
using dosymep.Revit.Engine;
using dosymep.Revit.Engine.Pipelines;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

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
            RevitPipeline pipeline = RevitPipeline.CreateRevitPipeline(PipelineFile);
            context.OpenDocument(pipeline.OpenModelOptions);

            var transformer = new RevitExternalTransformer(pipeline.OpenModelOptions.ModelPath, context);
            var stepOptions = pipeline.StepOptions
                .Select(item => PipelineOptions.CreateOption(item, transformer));

            foreach(PipelineOptions pipelineOption in stepOptions) {
                pipelineOption.Value.ExecuteExternalItem(pipelineOption.Options.WithOptions);
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
                    RevitAddinManifest.GetAddinManifest(usesName + ".yaml").AddinItems.FirstOrDefault();
                return revitAddinItem?.Reduce<IRevitExternalItem, RevitAddinItem>(transformer);
            }
        }
    }
}