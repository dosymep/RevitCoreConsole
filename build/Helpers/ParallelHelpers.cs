namespace Build.Helpers;

internal static class ParallelHelpers {
    public static Task ForEachAsync<T>(
        IEnumerable<T> source,
        CancellationToken cancellationToken,
        Func<T, CancellationToken, ValueTask> body,
        int maxDegreeOfParallelism) {
        return Parallel.ForEachAsync(
            source,
            new ParallelOptions {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism),
            },
            body);
    }
}
