using WinRevive.Models;

namespace WinRevive.Services;

public sealed class StartupManagementService
{
    private readonly StartupService _startup;
    private readonly SnapshotService _snapshots;
    private readonly OperationHistoryService _history;

    public StartupManagementService(StartupService startup, SnapshotService snapshots, OperationHistoryService history)
    {
        _startup = startup;
        _snapshots = snapshots;
        _history = history;
    }

    public string Disable(StartupEntry entry)
    {
        var snapshotId = _snapshots.Create("startup", entry);
        _startup.DisableCurrentUserEntry(entry);
        var present = _startup.ReadEntries().Any(item =>
            item.Scope == entry.Scope &&
            item.RegistryPath == entry.RegistryPath &&
            item.Name == entry.Name);
        if (present) throw new InvalidOperationException("Başlangıç girdisi devre dışı bırakılamadı.");
        _history.Record("Startup", "Disabled", $"{entry.Name}; snapshot {snapshotId}.");
        return $"{entry.Name} devre dışı bırakıldı.";
    }

    public string RevertLatest()
    {
        var snapshot = _snapshots.ReadLatest<StartupEntry>("startup")
            ?? throw new InvalidOperationException("Geri alınacak başlangıç snapshot'ı bulunamadı.");
        _startup.EnableCurrentUserEntry(snapshot.State);
        var restored = _startup.ReadEntries().Any(item =>
            item.Scope == snapshot.State.Scope &&
            item.RegistryPath == snapshot.State.RegistryPath &&
            item.Name == snapshot.State.Name &&
            item.Command == snapshot.State.Command);
        if (!restored) throw new InvalidOperationException("Başlangıç girdisi geri alınamadı.");
        _history.Record("Startup", "Reverted", $"{snapshot.State.Name}; snapshot {snapshot.Id}.");
        return $"{snapshot.State.Name} geri alındı.";
    }
}
