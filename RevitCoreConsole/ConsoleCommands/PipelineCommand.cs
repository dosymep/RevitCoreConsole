using System.CommandLine;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class PipelineCommand : BaseCommand {
        public static readonly Option<string> PipelineOption
            = new Option<string>(
                name: "/pipeline",
                description: "Pipeline file path.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("pipeline")
                .AddParam(LicenseKeyOption)
                .AddParam(PipelineOption)
                .SetHandler(new PipelineCommandBinder())
                .SetDescription("Pipeline command");

        public string Pipeline { get; set; }
        public string LicenseKey { get; set; }

        protected override void ExecuteImpl() {
            throw new System.NotImplementedException();
        }
    }
}