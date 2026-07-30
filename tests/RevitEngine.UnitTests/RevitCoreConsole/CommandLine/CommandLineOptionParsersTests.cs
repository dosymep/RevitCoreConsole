using System.CommandLine;

using RevitCoreConsole.CommandLine;

using ResourceStrings = RevitCoreConsole.Resources.Resources;

namespace RevitEngine.UnitTests.RevitCoreConsole.CommandLine;

public class CommandLineOptionParsersTests {
    private const string RvtModelFileName = "sample-model.rvt";
    private const string RfaModelFileName = "sample-family.rfa";
    private const string ZipBundleFileName = "sample-bundle.zip";

    #region ParseModelPath

    [Test]
    public async Task ParseModelPath_MissingValue_AddsRequiredError() {
        var result = ParseModelPathOption(["/i", ""]);

        await Assert.That(result.Errors.Select(item => item.Message))
            .Contains(ResourceStrings.ModelPathRequiredError);
    }

    [Test]
    [Arguments("RSN://server/model")]
    [Arguments("BIM 360://account/model")]
    [Arguments("rsn://server/model")]
    public async Task ParseModelPath_RemotePath_IsAcceptedWithoutFileChecks(string remotePath) {
        var result = ParseModelPathOption(["/i", remotePath]);

        await Assert.That(result.Errors).IsEmpty();
        await Assert.That(GetModelPathValue(result)).IsEqualTo(remotePath);
    }

    [Test]
    public async Task ParseModelPath_WrongExtension_AddsExtensionError() {
        string path = CreateTempFile("sample-model.txt");
        try {
            var result = ParseModelPathOption(["/i", path]);

            await Assert.That(result.Errors.Select(item => item.Message))
                .Contains(ResourceStrings.ModelPathExtensionError);
        } finally {
            File.Delete(path);
        }
    }

    [Test]
    public async Task ParseModelPath_MissingFile_AddsNotFoundError() {
        string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".rvt");

        var result = ParseModelPathOption(["/i", missingPath]);

        await Assert.That(result.Errors.Select(item => item.Message))
            .Contains(ResourceStrings.ModelPathNotFoundError);
    }

    [Test]
    [Arguments(".rvt")]
    [Arguments(".rfa")]
    [Arguments(".RVT")]
    public async Task ParseModelPath_ExistingSupportedFile_ParsesSuccessfully(string extension) {
        string path = CreateTempFile("sample-model" + extension);
        try {
            var result = ParseModelPathOption(["/i", path]);

            await Assert.That(result.Errors).IsEmpty();
            await Assert.That(GetModelPathValue(result)).IsEqualTo(path);
        } finally {
            File.Delete(path);
        }
    }

    #endregion

    #region ParseBundlePath

    [Test]
    public async Task ParseBundlePath_MissingValue_AddsRequiredError() {
        var result = ParseBundlePathOption(["/al", ""]);

        await Assert.That(result.Errors.Select(item => item.Message))
            .Contains(ResourceStrings.BundlePathRequiredError);
    }

    [Test]
    public async Task ParseBundlePath_MissingPath_AddsNotFoundError() {
        string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");

        var result = ParseBundlePathOption(["/al", missingPath]);

        await Assert.That(result.Errors.Select(item => item.Message))
            .Contains(ResourceStrings.BundlePathNotFoundError);
    }

    [Test]
    public async Task ParseBundlePath_ExistingDirectory_ParsesWithoutExtensionCheck() {
        string directoryPath = CreateTempDirectory();
        try {
            var result = ParseBundlePathOption(["/al", directoryPath]);

            await Assert.That(result.Errors).IsEmpty();
        } finally {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Test]
    public async Task ParseBundlePath_ExistingFileWithWrongExtension_AddsExtensionError() {
        string filePath = CreateTempFile("sample-bundle.rar");
        try {
            var result = ParseBundlePathOption(["/al", filePath]);

            await Assert.That(result.Errors.Select(item => item.Message))
                .Contains(ResourceStrings.BundlePathExtensionError);
        } finally {
            File.Delete(filePath);
        }
    }

    [Test]
    public async Task ParseBundlePath_ExistingZipFile_ParsesSuccessfully() {
        string filePath = CreateTempFile(ZipBundleFileName);
        try {
            var result = ParseBundlePathOption(["/al", filePath]);

            await Assert.That(result.Errors).IsEmpty();
        } finally {
            File.Delete(filePath);
        }
    }

    #endregion

    #region ParseLanguageCode

    [Test]
    [Arguments("ENU")]
    [Arguments("RUS")]
    [Arguments("enu")]
    public async Task ParseLanguageCode_KnownCode_ParsesSuccessfully(string code) {
        var result = ParseLanguageCodeOption(["/l", code]);

        await Assert.That(result.Errors).IsEmpty();
    }

    [Test]
    public async Task ParseLanguageCode_UnknownCode_AddsError() {
        var result = ParseLanguageCodeOption(["/l", "not-a-real-language"]);

        await Assert.That(result.Errors.Select(item => item.Message))
            .Contains(ResourceStrings.LanguageCodeError);
    }

    #endregion

    #region ParseWorksetConfiguration

    [Test]
    [Arguments("OpenAllWorksets")]
    [Arguments("CloseAllWorksets")]
    [Arguments("openlastviewed")]
    public async Task ParseWorksetConfiguration_KnownValue_ParsesSuccessfully(string value) {
        var result = ParseWorksetOption(["/w", value]);

        await Assert.That(result.Errors).IsEmpty();
    }

    [Test]
    public async Task ParseWorksetConfiguration_UnknownValue_AddsError() {
        var result = ParseWorksetOption(["/w", "not-a-real-workset"]);

        await Assert.That(result.Errors.Select(item => item.Message))
            .Contains(ResourceStrings.WorksetsError);
    }

    #endregion

    #region helpers

    private static ParseResult ParseModelPathOption(string[] args) {
        Option<string> modelPathOption = new("/input", "/i") {
            Required = true,
            CustomParser = CommandLineOptionParsers.ParseModelPath,
        };

        return new RootCommand { modelPathOption }.Parse(args);
    }

    private static string? GetModelPathValue(ParseResult result) {
        var option = result.CommandResult.Command.Options
            .OfType<Option<string>>()
            .First(item => item.Name == "/input");
        return result.GetValue(option);
    }

    private static ParseResult ParseBundlePathOption(string[] args) {
        Option<FileInfo> bundlePathOption = new("/app-bundle", "/al") {
            Required = true,
            CustomParser = CommandLineOptionParsers.ParseBundlePath,
        };

        return new RootCommand { bundlePathOption }.Parse(args);
    }

    private static ParseResult ParseLanguageCodeOption(string[] args) {
        Option<LanguageCode> languageCodeOption = new("/lang", "/l") {
            CustomParser = CommandLineOptionParsers.ParseLanguageCode,
        };

        return new RootCommand { languageCodeOption }.Parse(args);
    }

    private static ParseResult ParseWorksetOption(string[] args) {
        Option<WorksetConfiguration> worksetsOption = new("/worksets", "/w") {
            CustomParser = CommandLineOptionParsers.ParseWorksetConfiguration,
        };

        return new RootCommand { worksetsOption }.Parse(args);
    }

    private static string CreateTempFile(string fileName) {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_" + fileName);
        File.WriteAllBytes(path, []);
        return path;
    }

    private static string CreateTempDirectory() {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        return path;
    }

    #endregion
}
