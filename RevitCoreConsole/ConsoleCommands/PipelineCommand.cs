using System.CommandLine;

using dosymep.Revit.Engine;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class PipelineCommand : BaseCommand<RevitContext> {
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

        protected override void ExecuteImpl(RevitContext context) {
            throw new System.NotImplementedException();
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}