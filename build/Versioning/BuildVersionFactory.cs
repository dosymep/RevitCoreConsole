using System.Xml.Linq;

using ModularPipelines.Git;

namespace Build.Versioning;

public sealed class BuildVersionFactory {
    private readonly string _prefix;

    public BuildVersionFactory(string solutionRoot) {
        _prefix = ReadPrefix(solutionRoot);
    }

    public string ForRevit(IGitInformation git, string revitVersion) {
        string hash = FirstNonEmpty(git.LastCommitShortSha, git.LastCommitSha)
            ?? throw new InvalidOperationException("Git commit hash is unavailable.");

        return $"{_prefix}-{ToCommitToken(hash)}.{revitVersion}";
    }

    public (string Name, string Value)[] PropertiesFor(IGitInformation git, string revitVersion) {
        string version = ForRevit(git, revitVersion);
        return [
            ("Version", version),
            ("InformationalVersion", version),
            ("IncludeSourceRevisionInInformationalVersion", "false"),
        ];
    }

    private static string? FirstNonEmpty(params string?[] values) {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string ReadPrefix(string solutionRoot) {
        string propsPath = Path.Combine(solutionRoot, "Directory.Build.props");
        string? version = XDocument.Load(propsPath)
            .Descendants("Version")
            .FirstOrDefault()
            ?.Value
            .Trim();

        if(string.IsNullOrWhiteSpace(version)) {
            throw new InvalidOperationException($"Version element was not found in {propsPath}.");
        }

        int suffixIndex = version.IndexOf('-');
        return suffixIndex >= 0 ? version[..suffixIndex] : version;
    }

    private static string ToCommitToken(string commitHash) {
        string hash = commitHash.Trim().ToLowerInvariant();
        if(hash.Length > 7) {
            hash = hash[..7];
        }

        if(hash.Length == 0) {
            throw new InvalidOperationException("Commit hash is empty.");
        }

        foreach(char character in hash) {
            if(character is < '0' or > '9') {
                return hash;
            }
        }

        return "g" + hash;
    }
}
