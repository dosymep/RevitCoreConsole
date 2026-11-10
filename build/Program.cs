using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using ModularPipelines;
using ModularPipelines.Extensions;

using Build;
using Build.Helpers;
using Build.Modules;
using Build.Options;
using Build.Versioning;

var builder = Pipeline.CreateBuilder(args);

builder.Services.AddOptions<BuildOptions>()
    .Configure(options => {
        options.SolutionRoot = new SolutionRootLocator()
            .Find(options.SolutionFileName, AppContext.BaseDirectory);

        string? configuration = Environment.GetEnvironmentVariable("CONFIGURATION");
        if(!string.IsNullOrWhiteSpace(configuration)) {
            options.Configuration = configuration.Trim();
        }

        string? revitVersions = Environment.GetEnvironmentVariable("REVIT_VERSIONS");
        if(!string.IsNullOrWhiteSpace(revitVersions)) {
            options.RevitVersions = revitVersions
                .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        string? maxParallelism = Environment.GetEnvironmentVariable("MAX_PARALLELISM");
        if(int.TryParse(maxParallelism, out int degree) && degree > 0) {
            options.MaxDegreeOfParallelism = degree;
        }
    });

builder.Services.AddSingleton(static services => {
    BuildOptions options = services.GetRequiredService<IOptions<BuildOptions>>().Value;
    return new BuildVersionFactory(options.SolutionRoot);
});

builder.AddModule<CleanModule>();
builder.AddModule<RestoreModule>();
builder.AddModule<BuildModule>();
builder.AddModule<BuildSampleModule>();
builder.AddModule<PackageModule>();
builder.AddModule<PackageSampleModule>();
builder.AddModule<TestsModule>();
builder.AddModule<RunConsoleModule>();

await builder.Build().RunAsync();
