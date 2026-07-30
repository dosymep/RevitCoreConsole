namespace Build.Helpers;

public sealed class SolutionRootLocator {
    public string Find(string solutionFileName, string startDirectory) {
        for(DirectoryInfo? directory = new(startDirectory);
            directory is not null;
            directory = directory.Parent) {
            string solutionPath = Path.Combine(directory.FullName, solutionFileName);
            if(File.Exists(solutionPath)) {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate {solutionFileName} starting from {startDirectory}.");
    }
}
