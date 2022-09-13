using System.Collections.Generic;
using System.CommandLine;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class RevitCommand : BaseCommand<RevitContext> {
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

        protected override void ExecuteImpl(RevitContext context) {
            Logger.Information("Executing RevitCommand {@RevitCommand}", this);
            try {
                var revitAddin =
                    new RevitAddinDBApplication() {AssemblyPath = AssemblyPath, FullClassName = FullClassName};
                new RevitExternalTransformer(ModelPath, context)
                    .Transform(revitAddin)
                    .ExecuteExternalItem(ReadJournalData(JournalData));
            } finally {
                Logger.Information("Executed RevitCommand {@RevitCommand}", this);
            }
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}