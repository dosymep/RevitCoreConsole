using RevitEngine.Configuration;

namespace RevitEngine.UnitTests.RevitEngine.Configuration;

public class RevitApiOptionsTests {
    [Test]
    public async Task Defaults_AreAllTrue() {
        var options = new RevitApiOptions();

        await Assert.That(options.OverwriteExistingFiles).IsTrue();
        await Assert.That(options.ReplaceExistingSymbols).IsTrue();
        await Assert.That(options.IgnoreLinkedFilesOnSave).IsTrue();
        await Assert.That(options.ForceMultiUndoOperation).IsTrue();
        await Assert.That(options.UpdateSharedFamilies).IsTrue();
        await Assert.That(options.UpdateFamilyParameters).IsTrue();
        await Assert.That(options.IgnoreMissingUpdaters).IsTrue();
        await Assert.That(options.AcceptForeignFiles).IsTrue();
    }

    [Test]
    public async Task Init_CanOverrideEachOption() {
        var options = new RevitApiOptions {
            OverwriteExistingFiles = false,
            ReplaceExistingSymbols = false,
            IgnoreLinkedFilesOnSave = false,
            ForceMultiUndoOperation = false,
            UpdateSharedFamilies = false,
            UpdateFamilyParameters = false,
            IgnoreMissingUpdaters = false,
            AcceptForeignFiles = false,
        };

        await Assert.That(options.OverwriteExistingFiles).IsFalse();
        await Assert.That(options.ReplaceExistingSymbols).IsFalse();
        await Assert.That(options.IgnoreLinkedFilesOnSave).IsFalse();
        await Assert.That(options.ForceMultiUndoOperation).IsFalse();
        await Assert.That(options.UpdateSharedFamilies).IsFalse();
        await Assert.That(options.UpdateFamilyParameters).IsFalse();
        await Assert.That(options.IgnoreMissingUpdaters).IsFalse();
        await Assert.That(options.AcceptForeignFiles).IsFalse();
    }
}
