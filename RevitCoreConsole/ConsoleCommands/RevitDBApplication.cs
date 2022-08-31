using System.Collections.Generic;
using System.CommandLine;

using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class RevitDBApplication : BaseCommand<dosymep.Revit.Engine.RevitApplication> {
        public static readonly Command ConsoleCommand
            = new Command("revit_dbapplication")
                .AddParam(AssemblyPathOption)
                .AddParam(LicenseKeyOption)
                .AddParam(FullClassNameOption)
                .AddParam(JournalDataOption)
                .SetHandler(new RevitDBApplicationBinder())
                .SetDescription("Revit db addin application");

        public string LicenseKey { get; set; }
        public string JournalData { get; set; }
        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }

        protected override void ExecuteImpl(dosymep.Revit.Engine.RevitApplication application) {
            var revitAddin = new RevitAddinDBApplication() {AssemblyPath = AssemblyPath, FullClassName = FullClassName};
            new RevitExternalTransformer(ModelPath, application)
                .Transform(revitAddin)
                .ExecuteExternalItem(ReadJournalData(JournalData));
        }

        protected override dosymep.Revit.Engine.RevitApplication CreateApplication() {
            return CreateRevitApplication(LicenseKey);
        }
    }
}