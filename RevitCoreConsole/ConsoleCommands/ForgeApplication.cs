using System.CommandLine;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class ForgeApplication : BaseCommand {
        public static readonly Option<string> BundlePathOption
            = new Option<string>(
                name: "/al",
                description: "Bundle option.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("forge_application")
                .AddParam(LicenseKeyOption)
                .AddParam(BundlePathOption)
                .SetHandler(new ForgeApplicationBinder())
                .SetDescription("Revit forge application (this command works like Forge RevitCoreConsole)");

        public string LicenseKey { get; set; }
        public string BundlePath { get; set; }

        protected override void ExecuteImpl() {
            throw new System.NotImplementedException();
        }
    }
}