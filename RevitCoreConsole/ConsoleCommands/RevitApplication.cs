using System.CommandLine;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class RevitApplication : BaseCommand<dosymep.Revit.Engine.RevitApplication> {
        public static readonly Command ConsoleCommand
            = new Command("revit_application")
                .AddParam(AssemblyPathOption)
                .AddParam(LicenseKeyOption)
                .AddParam(FullClassNameOption)
                .AddParam(JournalDataOption)
                .SetHandler(new RevitApplicationBinder())
                .SetDescription("Revit addin application");

        public string LicenseKey { get; set; }
        public string JournalData { get; set; }
        public string AssemblyPath { get; set; }
        public string FullClassName { get; set; }

        protected override void ExecuteImpl(dosymep.Revit.Engine.RevitApplication application) {
            throw new System.NotImplementedException();
        }

        protected override dosymep.Revit.Engine.RevitApplication CreateApplication() {
            return CreateRevitApplication(LicenseKey);
        }
    }
}