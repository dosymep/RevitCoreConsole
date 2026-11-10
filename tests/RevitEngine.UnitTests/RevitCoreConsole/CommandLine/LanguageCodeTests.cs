using System.Globalization;

using RevitCoreConsole.CommandLine;

namespace RevitEngine.UnitTests.RevitCoreConsole.CommandLine;

public class LanguageCodeTests {
    [Test]
    [Arguments("RUS", "RUS")]
    [Arguments("rus", "RUS")]
    [Arguments("Russian", "RUS")]
    [Arguments("RUSSIAN", "RUS")]
    [Arguments("ENU", "ENU")]
    [Arguments("enu", "ENU")]
    [Arguments("English_USA", "ENU")]
    public async Task GetLanguageCode_ByCodeOrFullCode_ReturnsExpectedCode(string input, string expectedCode) {
        var result = LanguageCode.GetLanguageCode(input);

        await Assert.That(result.Code).IsEqualTo(expectedCode);
    }

    [Test]
    [Arguments("RU")]
    [Arguments("ru")]
    public async Task GetLanguageCode_RuAlias_ReturnsRussian(string input) {
        var result = LanguageCode.GetLanguageCode(input);

        await Assert.That(result.Code).IsEqualTo("RUS");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task GetLanguageCode_NullOrEmpty_ReturnsUnknown(string? input) {
        var result = LanguageCode.GetLanguageCode(input!);

        await Assert.That(result).IsEqualTo(LanguageCode.Unknown);
    }

    [Test]
    public async Task GetLanguageCode_UnknownValue_ReturnsUnknown() {
        var result = LanguageCode.GetLanguageCode("not-a-real-code");

        await Assert.That(result).IsEqualTo(LanguageCode.Unknown);
    }

    [Test]
    public async Task GetLanguageCode_ByCultureInfo_ReturnsMatchingCode() {
        var result = LanguageCode.GetLanguageCode(CultureInfo.GetCultureInfo("ru-RU"));

        await Assert.That(result.Code).IsEqualTo("RUS");
    }

    [Test]
    public async Task GetLanguageCode_ByCultureInfo_NullCulture_Throws() {
        await Assert.That(() => LanguageCode.GetLanguageCode((CultureInfo)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetLanguageCode_ByCultureInfo_UnsupportedCulture_Throws() {
        var unsupported = CultureInfo.GetCultureInfo("sv-SE");

        await Assert.That(() => LanguageCode.GetLanguageCode(unsupported))
            .Throws<NotSupportedException>();
    }

    [Test]
    public async Task GetLanguageCodes_ReturnsFifteenSupportedCodesPlusUnknown() {
        var codes = LanguageCode.GetLanguageCodes().ToList();

        // 15 supported Revit language codes + the Unknown sentinel value,
        // since GetLanguageCodes() reflects over all public static fields.
        await Assert.That(codes.Count).IsEqualTo(16);
        await Assert.That(codes.Select(item => item.Code).Distinct().Count()).IsEqualTo(16);
    }

    [Test]
    public async Task GetLanguageCodes_ContainsUnknown() {
        var codes = LanguageCode.GetLanguageCodes();

        await Assert.That(codes).Contains(LanguageCode.Unknown);
    }

    [Test]
    public async Task Equals_SameCodeAndFullCode_AreEqual() {
        var first = LanguageCode.GetLanguageCode("RUS");
        var second = LanguageCode.GetLanguageCode("Russian");

        await Assert.That(first.Equals(second)).IsTrue();
        await Assert.That(first == second).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task Equals_DifferentCodes_AreNotEqual() {
        var first = LanguageCode.GetLanguageCode("RUS");
        var second = LanguageCode.GetLanguageCode("ENU");

        await Assert.That(first.Equals(second)).IsFalse();
        await Assert.That(first != second).IsTrue();
    }

    [Test]
    public async Task Equals_Null_ReturnsFalse() {
        var code = LanguageCode.GetLanguageCode("RUS");

        await Assert.That(code.Equals(null)).IsFalse();
        await Assert.That(code == null).IsFalse();
        await Assert.That(code != null).IsTrue();
    }

    [Test]
    public async Task ToString_ReturnsDisplayName() {
        var code = LanguageCode.GetLanguageCode("RUS");

        await Assert.That(code.ToString()).IsEqualTo(code.DisplayName);
    }
}
