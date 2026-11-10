using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using RevitEngine.AssemblyResolution;
using RevitEngine.Configuration;

namespace RevitEngine.UnitTests.RevitEngine.AssemblyResolution;

// These tests mutate the process-wide PATH environment variable, so they must not run
// in parallel with each other (or with anything else that reads/writes PATH).
[NotInParallel]
public class RevitAssemblyResolverTests {
    [Test]
    public async Task Install_EmptyApplicationPath_ThrowsInvalidOperationException() {
        var resolver = CreateResolver(applicationPath: "");

        await Assert.That(() => resolver.Install()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Install_MissingApplicationDirectory_ThrowsDirectoryNotFoundException() {
        string missingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var resolver = CreateResolver(missingDirectory);

        await Assert.That(() => resolver.Install()).Throws<DirectoryNotFoundException>();
    }

    [Test]
    public async Task Install_DirectoryWithoutRevitAssemblies_ThrowsInvalidOperationException() {
        string directory = CreateTempDirectory();
        try {
            var resolver = CreateResolver(directory);

            await Assert.That(() => resolver.Install()).Throws<InvalidOperationException>();
        } finally {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task Install_ValidRevitDirectory_AddsApplicationPathToPath() {
        string directory = CreateFakeRevitInstallation();
        using var cleanup = new ResolverTestCleanup(directory);

        var resolver = CreateResolver(directory);

        resolver.Install();

        string? updatedPath = Environment.GetEnvironmentVariable("PATH");
        await Assert.That(updatedPath).IsNotNull();
        await Assert.That(updatedPath!.Split(Path.PathSeparator)).Contains(directory);

        resolver.Uninstall();
    }

    [Test]
    public async Task Install_CalledTwice_DoesNotDuplicatePathEntry() {
        string directory = CreateFakeRevitInstallation();
        using var cleanup = new ResolverTestCleanup(directory);

        var resolver = CreateResolver(directory);

        resolver.Install();
        resolver.Install();

        string[] pathEntries = Environment.GetEnvironmentVariable("PATH")!.Split(Path.PathSeparator);
        await Assert.That(pathEntries.Count(entry =>
                entry.Equals(directory, StringComparison.OrdinalIgnoreCase)))
            .IsEqualTo(1);

        resolver.Uninstall();
    }

    [Test]
    public async Task Uninstall_WithoutInstall_DoesNotThrow() {
        var resolver = CreateResolver(applicationPath: "C:\\does-not-matter");

        await Assert.That(() => resolver.Uninstall()).ThrowsNothing();
    }

    [Test]
    public async Task Dispose_CallsUninstall_DoesNotThrow() {
        string directory = CreateFakeRevitInstallation();
        using var cleanup = new ResolverTestCleanup(directory);

        var resolver = CreateResolver(directory);
        resolver.Install();

        await Assert.That(() => resolver.Dispose()).ThrowsNothing();
    }

    private sealed class ResolverTestCleanup : IDisposable {
        private readonly string _directory;
        private readonly string _originalDirectory;
        private readonly string? _originalPath;

        public ResolverTestCleanup(string directory) {
            _directory = directory;
            _originalDirectory = Directory.GetCurrentDirectory();
            _originalPath = Environment.GetEnvironmentVariable("PATH");
        }

        public void Dispose() {
            Directory.SetCurrentDirectory(_originalDirectory);
            Environment.SetEnvironmentVariable("PATH", _originalPath);
            if(Directory.Exists(_directory)) {
                Directory.Delete(_directory, recursive: true);
            }
        }
    }

    private static RevitAssemblyResolver CreateResolver(string applicationPath) {
        var options = new RevitApplicationOptions {
            ApplicationGuid = Guid.NewGuid(),
            ApplicationName = "Test",
            ApplicationPath = applicationPath,
            LicenseKey = "test-license",
            VendorId = "test-vendor",
            VendorDescription = "Test Vendor",
            Startup = new RevitStartupOptions(),
        };

        return new RevitAssemblyResolver(
            NullLogger<RevitAssemblyResolver>.Instance,
            Microsoft.Extensions.Options.Options.Create(options));
    }

    private static string CreateTempDirectory() {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    private static string CreateFakeRevitInstallation() {
        string directory = CreateTempDirectory();
        File.WriteAllBytes(Path.Combine(directory, "RevitAPI.dll"), []);
        File.WriteAllBytes(Path.Combine(directory, "RevitNET.dll"), []);
        return directory;
    }
}
