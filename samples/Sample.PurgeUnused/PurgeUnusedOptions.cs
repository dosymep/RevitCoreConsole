namespace Sample.PurgeUnused;

public sealed class PurgeUnusedOptions {
    public int TryCount { get; init; } = 5;
    public bool WithThermals { get; init; } = true;
    public bool WithMaterials { get; init; } = true;
    public bool WithStructures { get; init; } = true;
    public bool WithAppearances { get; init; } = true;
    public bool WithSymbols { get; init; } = true;
    public bool WithLinkSymbols { get; init; } = true;
    public bool WithFamilies { get; init; } = true;
    public bool WithNonDeletable { get; init; } = true;
    public bool WithImportCategories { get; init; } = true;
}
