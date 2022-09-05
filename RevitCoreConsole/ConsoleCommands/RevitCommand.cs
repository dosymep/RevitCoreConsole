using System.Collections.Generic;
using System.CommandLine;

using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class RevitCommand : BaseCommand<dosymep.Revit.Engine.RevitContext> {
        public static readonly Command ConsoleCommand
            = new Command("revit")
                .AddParam(AssemblyPathOption)
                .AddParam(FullClassNameOption)
                .AddParam(JournalDataOption)
                .SetHandler(new RevitCommandBinder())
                .SetDescription("Revit db addin application");
        
        public string JournalData { get; set; }
        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }

        protected override void ExecuteImpl(dosymep.Revit.Engine.RevitContext context) {
            var revitAddin = new RevitAddinDBApplication() {AssemblyPath = AssemblyPath, FullClassName = FullClassName};
            new RevitExternalTransformer(ModelPath, context)
                .Transform(revitAddin)
                .ExecuteExternalItem(ReadJournalData(JournalData));
        }

        protected override dosymep.Revit.Engine.RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}