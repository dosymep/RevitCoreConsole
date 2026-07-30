using Build.Options;

using Microsoft.Extensions.Options;

using ModularPipelines.Context;
using ModularPipelines.Modules;

namespace Build.Modules;

public sealed class CleanModule : Module {
    private readonly BuildOptions _options;

    public CleanModule(IOptions<BuildOptions> options) {
        _options = options.Value;
    }

    protected override Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        DeleteDirectory(_options.BinDirectory);
        DeleteDirectory(_options.ArtifactsDirectory);
        DeleteDirectory(_options.TestResultsDirectory);
        
        DeleteBinAndObj(_options.SrcDirectory);
        DeleteBinAndObj(_options.SamplesDirectory);
        DeleteBinAndObj(_options.TestsDirectory);
      
        return Task.CompletedTask;
    }

    private static void DeleteDirectory(string directoryPath) {
        if(Directory.Exists(directoryPath)) {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    private static void DeleteBinAndObj(string rootDirectory) {
        if(!Directory.Exists(rootDirectory)) {
            return;
        }

        var directories = Directory
            .EnumerateDirectories(rootDirectory, "*", SearchOption.AllDirectories)
            .Where(static path => Path.GetFileName(path) is "bin" or "obj")
            .OrderBy(static path => path.Length)
            .ToList();

        foreach(string directory in directories) {
            if(Directory.Exists(directory)) {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
