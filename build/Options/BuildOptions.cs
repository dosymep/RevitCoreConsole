namespace Build.Options;

public sealed class BuildOptions {
    public string Configuration { get; set; } = "Release";

    public string[] RevitVersions { get; set; } = [
        "2018", "2019", "2020", "2021", "2022", "2023", "2024", "2025", "2026", "2027"
    ];

    /// <summary>
    /// Max parallel work items for version/project fan-out. Default: processor count.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Math.Max(1, Environment.ProcessorCount / 4);

    public string SolutionFileName { get; set; } = "RevitCoreConsole.slnx";

    public string SolutionRoot { get; set; } = string.Empty;

    public string SolutionPath => Path.Combine(SolutionRoot, SolutionFileName);

    public string SrcDirectory => Path.Combine(SolutionRoot, "src");

    public string SamplesDirectory => Path.Combine(SolutionRoot, "samples");

    public string TestsDirectory => Path.Combine(SolutionRoot, "tests");

    public string TestsFilesDirectory => Path.Combine(SolutionRoot, "build", "tests_files");

    public string BinDirectory => Path.Combine(SolutionRoot, "bin");

    public string ArtifactsDirectory => Path.Combine(SolutionRoot, "artifacts");
    public string TestResultsDirectory => Path.Combine(SolutionRoot, "TestResults");

    public string GetConsoleOutputDirectory(string revitVersion) {
        return Path.Combine(BinDirectory, revitVersion, "RevitCoreConsole");
    }

    public string GetSampleBundleDirectory(string revitVersion, string pluginName) {
        return Path.Combine(BinDirectory, revitVersion, $"{pluginName}.bundle");
    }

    public string GetSampleComponentDirectory(string revitVersion, string pluginName) {
        return Path.Combine(GetSampleBundleDirectory(revitVersion, pluginName), revitVersion);
    }

    public string GetConsoleArchivePath(string revitVersion) {
        return Path.Combine(ArtifactsDirectory, $"RevitCoreConsole.{revitVersion}.zip");
    }

    public string GetSampleArchivePath(string pluginName) {
        return Path.Combine(ArtifactsDirectory, $"{pluginName}.zip");
    }
}
