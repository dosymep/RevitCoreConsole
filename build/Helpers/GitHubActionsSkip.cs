using ModularPipelines.Context;
using ModularPipelines.Enums;
using ModularPipelines.Models;

namespace Build.Helpers;

internal static class GitHubActionsSkip {
    public static SkipDecision WhenRunningOnGitHubActions(IModuleContext context) {
        return context.IsRunningIn(BuildSystem.GitHubActions)
            ? SkipDecision.Skip("Requires installed Autodesk Revit; skipped on GitHub Actions.")
            : SkipDecision.DoNotSkip;
    }
}
