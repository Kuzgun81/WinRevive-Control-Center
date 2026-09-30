namespace WinRevive.Services;

public sealed class TransparencyOptimizationRule
{
    private readonly RegistryService _registry;
    private readonly FileLogger _logger;
    private readonly OperationHistoryService _history;
    private readonly SnapshotService _snapshots;

    public TransparencyOptimizationRule(RegistryService registry, FileLogger logger, OperationHistoryService history, SnapshotService snapshots)
    {
        _registry = registry;
        _logger = logger;
        _history = history;
        _snapshots = snapshots;
    }

    public string Detect()
    {
        var value = _registry.ReadTransparency();
        return value is null ? "Windows'ta okunamadı veya değer tanımlı değil." : value == 0 ? "Zaten azaltılmış" : "Etkin";
    }

    public string Apply()
    {
        var current = _registry.ReadTransparency();
        if (current is null) throw new InvalidOperationException("Mevcut şeffaflık değeri okunamadı; değişiklik uygulanmadı.");
        var snapshotId = _snapshots.Create("transparency", new Backup(current.Value));
        _registry.WriteTransparency(0);
        if (_registry.ReadTransparency() != 0) throw new InvalidOperationException("Değişiklik doğrulanamadı.");
        _logger.Info($"Transparency applied; previous value: {current}.");
        _history.Record("Transparency", "Applied", $"Snapshot {snapshotId}; previous value: {current}; new value: 0.");
        return "Uygulandı ve doğrulandı.";
    }

    public string Revert()
    {
        var snapshot = _snapshots.ReadLatest<Backup>("transparency")
            ?? throw new InvalidOperationException("Geri alınacak snapshot bulunamadı.");
        _registry.WriteTransparency(snapshot.State.Value);
        if (_registry.ReadTransparency() != snapshot.State.Value) throw new InvalidOperationException("Geri alma doğrulanamadı.");
        _logger.Info($"Transparency reverted to {snapshot.State.Value}.");
        _history.Record("Transparency", "Reverted", $"Snapshot {snapshot.Id}; restored value: {snapshot.State.Value}.");
        return "Geri alındı ve doğrulandı.";
    }

    private sealed record Backup(int Value);
}
