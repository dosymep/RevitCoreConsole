using System.CommandLine;
using System.CommandLine.Parsing;

using ResourceStrings = RevitCoreConsole.Resources.Resources;

namespace RevitCoreConsole.CommandLine;

/// <summary>
/// Provides parsers for strongly typed command-line options.
/// </summary>
internal static class CommandLineOptionParsers {
    /// <summary>
    /// Parses and validates a local or remote Revit model path.
    /// </summary>
    /// <param name="argumentResult">The command-line argument being parsed.</param>
    /// <returns>The parsed model path.</returns>
    public static string? ParseModelPath(ArgumentResult argumentResult) {
        string? value = argumentResult.Tokens.SingleOrDefault()?.Value;
        if(string.IsNullOrWhiteSpace(value)) {
            argumentResult.AddError(ResourceStrings.ModelPathRequiredError);
            return value;
        }

        if(IsRemoteModelPath(value!)) {
            return value;
        }

        if(!value!.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase)
           && !value.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase)) {
            argumentResult.AddError(ResourceStrings.ModelPathExtensionError);
            return value;
        }

        if(!File.Exists(value)) {
            argumentResult.AddError(ResourceStrings.ModelPathNotFoundError);
        }

        return value;
    }

    /// <summary>
    /// Parses and validates an AppBundle file or directory path.
    /// </summary>
    /// <param name="argumentResult">The command-line argument being parsed.</param>
    /// <returns>The parsed AppBundle path.</returns>
    public static FileInfo? ParseBundlePath(ArgumentResult argumentResult) {
        string? value = argumentResult.Tokens.SingleOrDefault()?.Value;
        if(string.IsNullOrWhiteSpace(value)) {
            argumentResult.AddError(ResourceStrings.BundlePathRequiredError);
            return null;
        }

        var bundlePath = new FileInfo(value);
        if(Directory.Exists(bundlePath.FullName)) {
            return bundlePath;
        }

        if(!bundlePath.Exists) {
            argumentResult.AddError(ResourceStrings.BundlePathNotFoundError);
            return bundlePath;
        }

        if(!bundlePath.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)) {
            argumentResult.AddError(ResourceStrings.BundlePathExtensionError);
        }

        return bundlePath;
    }

    /// <summary>
    /// Parses a supported Revit language code.
    /// </summary>
    /// <param name="argumentResult">The command-line argument being parsed.</param>
    /// <returns>The parsed language code, or <see cref="LanguageCode.Unknown"/>.</returns>
    public static LanguageCode ParseLanguageCode(ArgumentResult argumentResult) {
        string? value = argumentResult.Tokens.SingleOrDefault()?.Value;
        var languageCode = LanguageCode.GetLanguageCode(value ?? string.Empty);
        if(languageCode == LanguageCode.Unknown) {
            argumentResult.AddError(ResourceStrings.LanguageCodeError);
        }

        return languageCode;
    }

    /// <summary>
    /// Parses a supported workset configuration.
    /// </summary>
    /// <param name="argumentResult">The command-line argument being parsed.</param>
    /// <returns>The parsed workset configuration, or <see cref="WorksetConfiguration.Unknown"/>.</returns>
    public static WorksetConfiguration ParseWorksetConfiguration(ArgumentResult argumentResult) {
        string? value = argumentResult.Tokens.SingleOrDefault()?.Value;
        var configuration = WorksetConfiguration.Parse(value);
        if(configuration.Equals(WorksetConfiguration.Unknown)) {
            argumentResult.AddError(ResourceStrings.WorksetsError);
        }

        return configuration;
    }

    /// <summary>
    /// Determines whether a model path refers to a remote Revit model.
    /// </summary>
    /// <param name="modelPath">The model path.</param>
    /// <returns><see langword="true"/> for RSN or BIM 360 paths.</returns>
    private static bool IsRemoteModelPath(string modelPath) {
        return modelPath.StartsWith("RSN://", StringComparison.OrdinalIgnoreCase)
               || modelPath.StartsWith("BIM 360://", StringComparison.OrdinalIgnoreCase);
    }
}
