using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Autodesk.ExchangeStore;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.Engine;
using dosymep.Revit.Engine.RevitExternals;
using dosymep.Revit.FileInfo.RevitAddins;

using RevitCoreConsole.ConsoleCommands.Binders;

namespace RevitCoreConsole.ConsoleCommands {
    internal class ForgeCommand : BaseCommand<RevitContext> {
        public static readonly Option<string> BundlePathOption
            = new Option<string>(
                name: "/al",
                description: "Bundle option.") {IsRequired = true};

        public static readonly Command ConsoleCommand
            = new Command("forge")
                .AddParam(ModelPathOption)
                .AddParam(BundlePathOption)
                .SetHandler(new ForgeCommandBinder())
                .SetDescription("Revit forge application (this command works like Forge RevitCoreConsole)");

        public string ModelPath { get; set; }
        public string BundlePath { get; set; }

        public RunTimeInfo RunTimeInfo
            => new RunTimeInfo("Revit", "Win64", "R" + RevitContext.RevitVersion);

        protected override void ExecuteImpl(RevitContext context) {
            Logger.Information("Executing ForgeCommand {@ForgeCommand}", this);
            try {
                var tempName = Path.Combine(Path.GetTempPath(), "RevitCoreConsole", "Bundles");
                ZipFile.ExtractToDirectory(BundlePath, tempName);
                Logger.Debug("Extracted bundle to {@TempName}", tempName);

                string bundlePath = Directory.GetDirectories(tempName)
                    .FirstOrDefault(item =>
                        item.EndsWith(".bundle", StringComparison.CurrentCultureIgnoreCase));

                if(string.IsNullOrEmpty(bundlePath)) {
                    throw new InvalidOperationException("The bundle doesn't contain .bundle folder.");
                }

                try {
                    var contentPath = Path.Combine(bundlePath, "PackageContents.xml");
                    var component = new RevitPackageContentsParser()
                        .FindComponentsEntry(contentPath, RunTimeInfo)
                        .FirstOrDefault();

                    if(component is null) {
                        throw new InvalidOperationException(
                            $"The bundle doesn't contain component for revit version {RevitContext.RevitVersion}.");
                    }

                    Logger.Debug("Loaded component {@Component}", component);
                    var dbapplication = RevitAddinManifest.GetAddinManifest(component.ModuleName)
                        .AddinDBApplications.FirstOrDefault();

                    if(dbapplication is null) {
                        throw new InvalidOperationException(
                            $"The bundle doesn't contain component module \"{component.ModuleName}\".");
                    }

                    Logger.Debug("Loaded DBApplication {@DBApplication}", dbapplication);
                    ServicesProvider.LoadInstanceCore(context.Application);
                    new RevitExternalTransformer(ModelPath, context)
                        .Transform(dbapplication)
                        .ExecuteExternalItem(new Dictionary<string, string>());
                } finally {
                    try {
                        Directory.Delete(tempName, true);
                        Logger.Debug("Removed extracted bundle {@TempName}", tempName);
                    } catch(Exception ex) {
                        Logger.Debug(ex, "Can't remove extracted bundle {@TempName}", tempName);
                    }
                }
            } finally {
                Logger.Information("Executed ForgeCommand {@ForgeCommand}", this);
            }
        }

        protected override RevitContext CreateApplication() {
            return CreateRevitApplication();
        }
    }
}