using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.Nuke.RevitVersions;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Components;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

using Logger = Serilog.Core.Logger;

class Build : NukeBuild, IHazSolution {
    static readonly AbsolutePath _appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    static readonly AbsolutePath _bim4EveryonePath = $"{_appData}/pyRevit/Extensions/BIM4Everyone.lib";

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] readonly string CommandName;
    [Parameter] readonly AbsolutePath Output = RootDirectory / "bin";

    /// <summary>
    /// Min Revit version.
    /// </summary>
    [Parameter("Min Revit version.")]
    readonly RevitVersion MinVersion = RevitVersion.Rv2016;

    /// <summary>
    /// Max Revit version.
    /// </summary>
    [Parameter("Max Revit version.")]
    readonly RevitVersion MaxVersion = RevitVersion.Rv2024;

    [Parameter("Build Revit versions.")] readonly RevitVersion[] RevitVersions = new RevitVersion[0];

    IEnumerable<RevitVersion> BuildRevitVersions;

    protected override void OnBuildInitialized() {
        base.OnBuildInitialized();
        BuildRevitVersions = RevitVersions.Length > 0
            ? RevitVersions
            : RevitVersion.GetRevitVersions(MinVersion, MaxVersion);
    }

    Target Clean => _ => _
        .Executes(() => {
            Output.CreateOrCleanDirectory();
            RootDirectory.GlobDirectories("**/bin", "**/obj")
                .Where(item => item != (RootDirectory / "build" / "bin"))
                .Where(item => item != (RootDirectory / "build" / "obj"))
                .DeleteDirectories();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() => {
            DotNetRestore(s => s
                .SetProjectFile(((IHazSolution) this).Solution));
        });

    Target DownloadBim4Everyone => _ => _
        .OnlyWhenStatic(() => !_bim4EveryonePath.DirectoryExists())
        .Executes(() => {
            Git($"clone https://github.com/dosymep/BIM4Everyone.git {_bim4EveryonePath}");
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(DownloadBim4Everyone)
        .Executes(() => {
            DotNetBuild(s => s
                .EnableForce()
                .DisableNoRestore()
                .SetConfiguration(Configuration)
                .SetProjectFile(CommandName)
                .When(IsServerBuild, _ => _
                    .EnableContinuousIntegrationBuild())
                .CombineWith(BuildRevitVersions, (settings, version) => {
                    return settings
                        .SetOutputDirectory(Output / version)
                        .SetProperty("RevitVersion", (int) version);
                }));
        });
}