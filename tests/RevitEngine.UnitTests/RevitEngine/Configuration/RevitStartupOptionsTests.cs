using RevitEngine.Configuration;
using RevitEngine.Runtime;

namespace RevitEngine.UnitTests.RevitEngine.Configuration;

public class RevitStartupOptionsTests {
    [Test]
    public async Task GetDefaultJournalPath_ExpandsEnvironmentVariables_AndContainsRevitVersion() {
        string path = RevitStartupOptions.GetDefaultJournalPath();

        await Assert.That(path).DoesNotContain("%");
        await Assert.That(path).Contains(RevitApplicationHost.RevitVersion);
        await Assert.That(path).Contains("Journals");
    }

    [Test]
    public async Task GetDefaultSettingsFileLocation_ExpandsEnvironmentVariables_AndContainsRevitVersion() {
        string path = RevitStartupOptions.GetDefaultSettingsFileLocation();

        await Assert.That(path).DoesNotContain("%");
        await Assert.That(path).Contains(RevitApplicationHost.RevitVersion);
    }

    [Test]
    public async Task Defaults_MatchDocumentedContract() {
        var options = new RevitStartupOptions();

        await Assert.That(options.JournalName).IsNull();
        await Assert.That(options.EnableIFC).IsFalse();
        await Assert.That(options.ApplyApiSettings).IsFalse();
        await Assert.That(options.LanguageType).IsEqualTo("English_USA");
        await Assert.That(options.Api).IsNotNull();
        await Assert.That(options.JournalPath).IsEqualTo(RevitStartupOptions.GetDefaultJournalPath());
        await Assert.That(options.SettingsFileLocation).IsEqualTo(RevitStartupOptions.GetDefaultSettingsFileLocation());
    }
}
