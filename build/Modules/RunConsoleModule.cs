using System.Collections.Concurrent;
using System.Diagnostics;

using Build.Helpers;
using Build.Options;

using Microsoft.Extensions.Options;

using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.Modules;

namespace Build.Modules;

[DependsOn<PackageModule>]
[DependsOn<PackageSampleModule>]
public sealed class RunConsoleModule : Module {
    private readonly BuildOptions _options;

    public RunConsoleModule(IOptions<BuildOptions> options) {
        _options = options.Value;
    }

    protected override ModuleConfiguration Configure() => ModuleConfiguration.Create()
        .WithSkipWhen(GitHubActionsSkip.WhenRunningOnGitHubActions)
        .Build();

    protected override async Task ExecuteModuleAsync(IModuleContext context, CancellationToken cancellationToken) {
        ConsoleRunJob[] jobs = [.. PrepareJobs()];
        var runningProcesses = new ConcurrentDictionary<int, Process>();

        await using var registration = cancellationToken.Register(
            static state => KillAll((ConcurrentDictionary<int, Process>) state!),
            runningProcesses);

        try {
            await ParallelHelpers.ForEachAsync(
                jobs,
                cancellationToken,
                (job, token) => new ValueTask(RunConsoleAsync(job, runningProcesses, token)),
                _options.MaxDegreeOfParallelism);
        } finally {
            KillAll(runningProcesses);
        }
    }

    private static async Task RunConsoleAsync(
        ConsoleRunJob job,
        ConcurrentDictionary<int, Process> runningProcesses,
        CancellationToken cancellationToken) {
        string stdoutPath = Path.Combine(job.WorkDirectory, "stdout.log");
        string stderrPath = Path.Combine(job.WorkDirectory, "stderr.log");

        var startInfo = new ProcessStartInfo {
            FileName = job.ConsoleExe,
            WorkingDirectory = Path.GetDirectoryName(job.ConsoleExe),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("/i");
        startInfo.ArgumentList.Add(job.ModelPath);
        startInfo.ArgumentList.Add("/al");
        startInfo.ArgumentList.Add(job.BundleZipPath);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start '{job.ConsoleExe}'.");

        runningProcesses[process.Id] = process;

        try {
            await using FileStream stdoutFile = File.Create(stdoutPath);
            await using FileStream stderrFile = File.Create(stderrPath);

            Task stdoutCopy = process.StandardOutput.BaseStream.CopyToAsync(stdoutFile, cancellationToken);
            Task stderrCopy = process.StandardError.BaseStream.CopyToAsync(stderrFile, cancellationToken);

            try {
                await process.WaitForExitAsync(cancellationToken);
                await Task.WhenAll(stdoutCopy, stderrCopy);
            } catch(OperationCanceledException) {
                TryKill(process);
                throw;
            }

            if(process.ExitCode != 0) {
                throw new InvalidOperationException(
                    $"RevitCoreConsole exited with code {process.ExitCode}. See '{stdoutPath}' and '{stderrPath}'.");
            }
        } finally {
            runningProcesses.TryRemove(process.Id, out _);
        }
    }

    private static void KillAll(ConcurrentDictionary<int, Process> runningProcesses) {
        foreach(var pair in runningProcesses.ToArray()) {
            if(runningProcesses.TryRemove(pair.Key, out var process)) {
                TryKill(process);
            }
        }
    }

    private static void TryKill(Process process) {
        try {
            if(!process.HasExited) {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(milliseconds: 5_000);
            }
        } catch(InvalidOperationException) {
        } catch(NotSupportedException) {
        } catch(System.ComponentModel.Win32Exception) {
        }
    }

    private IEnumerable<ConsoleRunJob> PrepareJobs() {
        if(!Directory.Exists(_options.ArtifactsDirectory)) {
            yield break;
        }

        string[] bundleZipPaths = Directory.GetFiles(_options.ArtifactsDirectory, "*.zip")
            .Where(static path => !Path.GetFileName(path)
                .StartsWith("RevitCoreConsole.", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach(string revitVersion in _options.RevitVersions) {
            if(!File.Exists(@$"C:\Program Files\Autodesk\Revit {revitVersion}\Revit.exe")) {
                continue;
            }

            string? sourceModelPath = ResolveSourceModelPath(revitVersion);
            if(sourceModelPath is null) {
                continue;
            }

            string consoleExe = Path.Combine(
                _options.GetConsoleOutputDirectory(revitVersion),
                "RevitCoreConsole.exe");

            string runsRoot = Path.Combine(_options.BinDirectory, revitVersion, "runs");
            RecreateDirectory(runsRoot);

            foreach(string bundleZipPath in bundleZipPaths) {
                yield return PrepareJob(consoleExe, sourceModelPath, bundleZipPath, runsRoot);
            }
        }
    }

    private static ConsoleRunJob PrepareJob(
        string consoleExe,
        string sourceModelPath,
        string sourceBundleZipPath,
        string runsRoot) {
        string runName = Path.GetFileNameWithoutExtension(sourceBundleZipPath);
        string workDirectory = Path.Combine(runsRoot, runName);
        RecreateDirectory(workDirectory);

        string modelPath = Path.Combine(workDirectory, Path.GetFileName(sourceModelPath));
        string bundleZipPath = Path.Combine(workDirectory, Path.GetFileName(sourceBundleZipPath));

        File.Copy(sourceModelPath, modelPath, overwrite: true);
        File.Copy(sourceBundleZipPath, bundleZipPath, overwrite: true);

        return new ConsoleRunJob(consoleExe, workDirectory, modelPath, bundleZipPath);
    }

    private string? ResolveSourceModelPath(string revitVersion) {
        string sourceDirectory = Path.Combine(_options.TestsFilesDirectory, revitVersion);
        if(!Directory.Exists(sourceDirectory)) {
            return null;
        }

        string[] sourceFiles = Directory.GetFiles(sourceDirectory, "*.rvt")
            .Where(static path => !IsBackupFile(path))
            .ToArray();

        if(sourceFiles.Length == 0) {
            return null;
        }

        string? preferredFile =
            FindPreferredModel(sourceFiles, "rac_basic_sample_project.rvt")
            ?? FindPreferredModel(sourceFiles, "rac_advanced_sample_project.rvt")
            ?? Path.GetFileName(sourceFiles[0]);

        return Path.Combine(sourceDirectory, preferredFile!);
    }

    private static void RecreateDirectory(string directoryPath) {
        if(Directory.Exists(directoryPath)) {
            Directory.Delete(directoryPath, recursive: true);
        }

        Directory.CreateDirectory(directoryPath);
    }

    private static string? FindPreferredModel(IEnumerable<string> sourceFiles, string fileName) {
        return sourceFiles
            .Select(Path.GetFileName)
            .FirstOrDefault(name =>
                name is not null
                && name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBackupFile(string path) {
        string fileName = Path.GetFileNameWithoutExtension(path);
        int lastDotIndex = fileName.LastIndexOf('.');
        if(lastDotIndex < 0) {
            return false;
        }

        string suffix = fileName[(lastDotIndex + 1)..];
        return suffix.Length == 4 && suffix.All(char.IsDigit);
    }

    private sealed record ConsoleRunJob(
        string ConsoleExe,
        string WorkDirectory,
        string ModelPath,
        string BundleZipPath);
}
