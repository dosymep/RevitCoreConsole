using RevitEngine.Documents;

namespace RevitEngine.UnitTests.RevitEngine.Documents;

public class OpenDocumentOptionsTests {
    [Test]
    public async Task Defaults_MatchDocumentedContract() {
        var options = new OpenDocumentOptions();

        await Assert.That(options.Audit).IsFalse();
        await Assert.That(options.ModelPath).IsNull();
        await Assert.That(options.WorksetConfiguration).IsEqualTo("OpenAllWorksets");
    }

    [Test]
    public async Task Properties_CanBeSet() {
        var options = new OpenDocumentOptions {
            Audit = true,
            ModelPath = "C:\\model.rvt",
            WorksetConfiguration = "CloseAllWorksets",
        };

        await Assert.That(options.Audit).IsTrue();
        await Assert.That(options.ModelPath).IsEqualTo("C:\\model.rvt");
        await Assert.That(options.WorksetConfiguration).IsEqualTo("CloseAllWorksets");
    }
}
