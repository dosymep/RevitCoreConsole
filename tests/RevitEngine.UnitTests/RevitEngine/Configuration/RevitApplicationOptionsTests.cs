using RevitEngine.Configuration;

namespace RevitEngine.UnitTests.RevitEngine.Configuration;

public class RevitApplicationOptionsTests {
    [Test]
    public async Task Properties_RoundTripThroughInitializer() {
        var applicationGuid = Guid.NewGuid();
        var startup = new RevitStartupOptions();

        var options = new RevitApplicationOptions {
            ApplicationGuid = applicationGuid,
            ApplicationName = "MyApp",
            ApplicationPath = "C:\\Program Files\\Autodesk\\Revit 2025",
            LicenseKey = "license-key",
            VendorId = "vendor-id",
            VendorDescription = "Vendor Description",
            Startup = startup,
        };

        await Assert.That(options.ApplicationGuid).IsEqualTo(applicationGuid);
        await Assert.That(options.ApplicationName).IsEqualTo("MyApp");
        await Assert.That(options.ApplicationPath).IsEqualTo("C:\\Program Files\\Autodesk\\Revit 2025");
        await Assert.That(options.LicenseKey).IsEqualTo("license-key");
        await Assert.That(options.VendorId).IsEqualTo("vendor-id");
        await Assert.That(options.VendorDescription).IsEqualTo("Vendor Description");
        await Assert.That(options.Startup).IsSameReferenceAs(startup);
    }
}
