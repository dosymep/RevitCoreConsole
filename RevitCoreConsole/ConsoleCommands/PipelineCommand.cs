using System.CommandLine;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class PipelineCommand : BaseCommand<dosymep.Revit.Engine.RevitContext> {
        public static readonly Option<string> PipelineOption
            = new Option<string>(
                name: "/pipeline",
                description: "Pipeline file path.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("pipeline")
                .AddParam(PipelineOption)
                .SetHandler(new PipelineCommandBinder())
                .SetDescription("Pipeline command");

        public string Pipeline { get; set; }

        protected override void ExecuteImpl(dosymep.Revit.Engine.RevitContext context) {
            throw new System.NotImplementedException();
        }

        protected override dosymep.Revit.Engine.RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}