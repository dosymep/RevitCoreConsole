using Build.Helpers;
using Build.Options;

using Microsoft.Extensions.Options;

using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<BuildModule>]
public sealed class TestsModule : Module {
    private readonly BuildOptions _options;

    public TestsModule(IOptions<BuildOptions> options) {
        _options = options.Value;
    }

    protected override ModuleConfiguration Configure() => ModuleConfiguration.Create()
        .WithSkipWhen(GitHubActionsSkip.WhenRunningOnGitHubActions)
        .Build();

    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        string testProjectPath = Path.Combine(
            _options.TestsDirectory,
            "RevitEngine.UnitTests",
            "RevitEngine.UnitTests.csproj");

        foreach(string revitVersion in _options.RevitVersions) {
            await RunTestsAsync(context, testProjectPath, revitVersion, cancellationToken);
        }
    }

    private async Task RunTestsAsync(
        IModuleContext context,
        string testProjectPath,
        string revitVersion,
        CancellationToken cancellationToken) {
        // Use classic `dotnet test <csproj>` — ModularPipelines DotNetTestOptions
        // maps to MTP `--project` and breaks MSBuild `-p:` properties (MSB1001).
        await context.Shell.Command.ExecuteCommandLineTool(
            new ExecutableCommandOptions {
                Tool = "dotnet",
                Arguments = [
                    "test",
                    testProjectPath,
                    "--configuration", _options.Configuration,
                    $"-p:RevitVersion={revitVersion}",
                ]
            },
            cancellationToken: cancellationToken);
    }
}
