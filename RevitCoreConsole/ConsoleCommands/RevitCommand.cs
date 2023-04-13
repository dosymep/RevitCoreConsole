using System.Collections.Generic;
using System.CommandLine;

using dosymep.Revit.Engine;
using dosymep.Revit.Engine.CoreCommands;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class RevitCommand : BaseCommand<RevitContext>, IRevitCommand {
        public static readonly Command ConsoleCommand
            = new Command("revit")
                .AddParam(ModelPathOption)
                .AddParam(AssemblyPathOption)
                .AddParam(FullClassNameOption)
                .AddParam(JournalDataOption)
                .SetHandler(new RevitCommandBinder())
                .SetDescription("Revit db addin application");

        public string ModelPath { get; set; }

        public string JournalData { get; set; }
        
        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }
        
        public RevitAddinItem RevitAddinItem => new RevitAddinDBApplication() {
            AssemblyPath = AssemblyPath, FullClassName = FullClassName
        };

        protected override void ExecuteImpl(RevitContext context) {
            Logger.Information("Executing RevitCommand {@RevitCommand}", this);
            try {
                context.ExecuteRevitCommand(this);
            } finally {
                Logger.Information("Executed RevitCommand {@RevitCommand}", this);
            }
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}