using System;
using System.CommandLine;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.AutodeskApps;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine;
using dosymep.Revit.Engine.CoreCommands;
using dosymep.Revit.Engine.Pipelines;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;
using dosymep.SimpleServices;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class PipelineCommand : BaseCommand<RevitContext>, IPipelineCommand {
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
                context.ExecutePipelineCommand(this);
            } finally {
                Logger.Information("Executed PipelineCommand {@PipelineCommand}", this);
            }
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}