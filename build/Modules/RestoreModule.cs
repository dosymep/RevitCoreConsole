using Build.Options;

using Microsoft.Extensions.Options;

using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<CleanModule>]
public sealed class RestoreModule : Module {
    private readonly BuildOptions _options;

    public RestoreModule(IOptions<BuildOptions> options) {
        _options = options.Value;
    }

    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        await context.DotNet()
            .Restore(
                new DotNetRestoreOptions { ProjectSolution = _options.SolutionPath },
                cancellationToken: cancellationToken);
    }
}
