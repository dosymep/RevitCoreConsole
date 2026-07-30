using System.Xml.Linq;

namespace Build.Helpers;

internal static class SolutionFileHelpers {
    /// <summary>
    /// Returns absolute paths of projects marked with <c>&lt;Build Project="false" /&gt;</c> in an .slnx file.
    /// </summary>
    public static IReadOnlySet<string> GetExcludedProjectPaths(string solutionPath) {
        if(!File.Exists(solutionPath)
           || !string.Equals(Path.GetExtension(solutionPath), ".slnx", StringComparison.OrdinalIgnoreCase)) {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        string solutionDirectory = Path.GetDirectoryName(solutionPath)!;
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var document = XDocument.Load(solutionPath);

        foreach(var projectElement in document.Descendants("Project")) {
            string? relativePath = projectElement.Attribute("Path")?.Value;
            if(string.IsNullOrWhiteSpace(relativePath)) {
                continue;
            }

            bool isExcluded = projectElement
                .Elements("Build")
                .Any(static build => IsFalse(build.Attribute("Project")?.Value));

            if(!isExcluded) {
                continue;
            }

            excluded.Add(Path.GetFullPath(Path.Combine(solutionDirectory, relativePath)));
        }

        return excluded;
    }

    private static bool IsFalse(string? value) {
        return bool.TryParse(value, out bool parsed) && !parsed;
    }
}
