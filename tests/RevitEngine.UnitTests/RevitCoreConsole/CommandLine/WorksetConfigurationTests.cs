using RevitCoreConsole.CommandLine;

namespace RevitEngine.UnitTests.RevitCoreConsole.CommandLine;

public class WorksetConfigurationTests {
    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Parse_NullOrWhitespace_ReturnsOpenAllWorksets(string? input) {
        var result = WorksetConfiguration.Parse(input);

        await Assert.That(result).IsEqualTo(WorksetConfiguration.OpenAllWorksets);
    }

    [Test]
    [Arguments("OpenAllWorksets", "OpenAllWorksets")]
    [Arguments("openallworksets", "OpenAllWorksets")]
    [Arguments("CloseAllWorksets", "CloseAllWorksets")]
    [Arguments("closeallworksets", "CloseAllWorksets")]
    [Arguments("OpenLastViewed", "OpenLastViewed")]
    [Arguments("openlastviewed", "OpenLastViewed")]
    public async Task Parse_KnownValue_ReturnsMatchingConfiguration(string input, string expectedName) {
        var result = WorksetConfiguration.Parse(input);

        await Assert.That(result.Name).IsEqualTo(expectedName);
    }

    [Test]
    public async Task Parse_UnknownValue_ReturnsUnknown() {
        var result = WorksetConfiguration.Parse("not-a-real-workset-configuration");

        await Assert.That(result).IsEqualTo(WorksetConfiguration.Unknown);
    }

    [Test]
    public async Task GetConfigurations_ReturnsThreeSupportedConfigurations() {
        var configurations = WorksetConfiguration.GetConfigurations().ToList();

        await Assert.That(configurations.Count).IsEqualTo(3);
        await Assert.That(configurations).Contains(WorksetConfiguration.OpenAllWorksets);
        await Assert.That(configurations).Contains(WorksetConfiguration.CloseAllWorksets);
        await Assert.That(configurations).Contains(WorksetConfiguration.OpenLastViewed);
    }

    [Test]
    public async Task GetConfigurations_DoesNotContainUnknown() {
        var configurations = WorksetConfiguration.GetConfigurations();

        await Assert.That(configurations).DoesNotContain(WorksetConfiguration.Unknown);
    }

    [Test]
    public async Task Equals_SameName_AreEqual() {
        var first = WorksetConfiguration.Parse("OpenAllWorksets");
        var second = WorksetConfiguration.Parse("openallworksets");

        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task Equals_DifferentName_AreNotEqual() {
        var first = WorksetConfiguration.Parse("OpenAllWorksets");
        var second = WorksetConfiguration.Parse("CloseAllWorksets");

        await Assert.That(first.Equals(second)).IsFalse();
    }

    [Test]
    public async Task Equals_Null_ReturnsFalse() {
        var configuration = WorksetConfiguration.OpenAllWorksets;

        await Assert.That(configuration.Equals(null)).IsFalse();
    }
}
