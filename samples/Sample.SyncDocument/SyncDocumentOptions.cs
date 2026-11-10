namespace Sample.SyncDocument;

public sealed class SyncDocumentOptions {
    public bool Compact { get; init; } = true;
    public string Comment { get; init; } = "Sync central model.";

    public bool UserWorksets { get; init; }
    public bool ViewWorksets { get; init; }
    public bool FamilyWorksets { get; init; }
    public bool StandardWorksets { get; init; }
    public bool CheckedOutElements { get; init; }

    public bool SaveLocalAfter { get; init; }
    public bool SaveLocalBefore { get; init; }
}
