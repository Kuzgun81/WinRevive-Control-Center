using System.Text.Json;

namespace WinRevive.Services;

public sealed class SnapshotService
{
    private readonly string _root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "snapshots");

    public string Create<T>(string scope, T state)
    {
        var id = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
        var directory = Path.Combine(_root, scope);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, id + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(
            new Snapshot<T>(id, scope, DateTimeOffset.UtcNow, state),
            new JsonSerializerOptions { WriteIndented = true }));
        return id;
    }

    public Snapshot<T>? ReadLatest<T>(string scope)
    {
        var directory = Path.Combine(_root, scope);
        if (!Directory.Exists(directory)) return null;
        var path = Directory.EnumerateFiles(directory, "*.json")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        return path is null ? null : JsonSerializer.Deserialize<Snapshot<T>>(File.ReadAllText(path));
    }

    public sealed record Snapshot<T>(string Id, string Scope, DateTimeOffset CreatedAt, T State);
}
