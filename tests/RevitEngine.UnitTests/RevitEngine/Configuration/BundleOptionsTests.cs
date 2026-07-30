using RevitEngine.Configuration;

namespace RevitEngine.UnitTests.RevitEngine.Configuration;

public class BundleOptionsTests {
    [Test]
    public async Task BundlePath_RoundTripsThroughInitializer() {
        var options = new BundleOptions { BundlePath = "C:\\bundles\\my-bundle.zip" };

        await Assert.That(options.BundlePath).IsEqualTo("C:\\bundles\\my-bundle.zip");
    }
}
