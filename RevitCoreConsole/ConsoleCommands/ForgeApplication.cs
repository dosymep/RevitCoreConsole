using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Autodesk.ExchangeStore;

using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class ForgeApplication : BaseCommand<dosymep.Revit.Engine.RevitApplication> {
        public static readonly Option<string> BundlePathOption
            = new Option<string>(
                name: "/al",
                description: "Bundle option.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("forge")
                .AddParam(BundlePathOption)
                .SetHandler(new ForgeApplicationBinder())
                .SetDescription("Revit forge application (this command works like Forge RevitCoreConsole)");

        public string BundlePath { get; set; }

        protected override void ExecuteImpl(dosymep.Revit.Engine.RevitApplication application) {
            var tempName = Path.Combine(Path.GetTempPath(), "RevitCoreConsole", "Bundles");
            ZipFile.ExtractToDirectory(BundlePath, tempName);

            string bundlePath = Directory.GetDirectories(tempName)
                .FirstOrDefault(item =>
                    item.EndsWith(".bundle", StringComparison.CurrentCultureIgnoreCase));

            try {
                var contentPath = Path.Combine(bundlePath, "PackageContents.xml");
                var component = new RevitPackageContentsParser()
                    .FindComponentsEntry(contentPath,
                        new RunTimeInfo("Revit", "Win64", "R2018"))
                    .FirstOrDefault();


                var dbapplication = RevitAddinManifest.GetAddinManifest(component.ModuleName).AddinDBApplications
                    .FirstOrDefault();
                new RevitExternalTransformer(ModelPath, application)
                    .Transform(dbapplication)
                    .ExecuteExternalItem(new Dictionary<string, string>());
            } finally {
                Directory.Delete(Path.GetTempPath(), true);
            }
        }

        protected override dosymep.Revit.Engine.RevitApplication CreateApplication() {
            return CreateRevitApplication();
        }
    }
}