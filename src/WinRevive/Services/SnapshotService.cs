using System.Text.Json;

namespace WinRevive.Services;

public sealed class SnapshotService
{
    private readonly string _root;

    public SnapshotService(string? root = null)
    {
        _root = root ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinRevive", "snapshots");
    }

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

    public IReadOnlyList<SnapshotEntry> List()
    {
        if (!Directory.Exists(_root)) return [];
        return Directory.EnumerateFiles(_root, "*.json", SearchOption.AllDirectories)
            .Select(ReadEntry)
            .OrderByDescending(item => item.CreatedAt)
            .ToArray();
    }

    public SnapshotComparison Compare(string firstId, string secondId)
    {
        var first = FindPath(firstId) ?? throw new FileNotFoundException("İlk snapshot bulunamadı.", firstId);
        var second = FindPath(secondId) ?? throw new FileNotFoundException("İkinci snapshot bulunamadı.", secondId);
        var firstJson = JsonDocument.Parse(File.ReadAllText(first)).RootElement.GetRawText();
        var secondJson = JsonDocument.Parse(File.ReadAllText(second)).RootElement.GetRawText();
        return new SnapshotComparison(firstId, secondId, firstJson == secondJson, firstJson, secondJson);
    }

    public bool Delete(string id)
    {
        var path = FindPath(id);
        if (path is null) return false;
        File.Delete(path);
        return true;
    }

    public int DeleteInvalid()
    {
        return List()
            .Where(item => !item.IsValid)
            .Count(item => Delete(item.Id));
    }

    private SnapshotEntry ReadEntry(string path)
    {
        var id = Path.GetFileNameWithoutExtension(path);
        var scope = Directory.GetParent(path)?.Name ?? "unknown";
        var createdAt = File.GetLastWriteTimeUtc(path);
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            if (root.TryGetProperty("Scope", out var scopeValue))
                scope = scopeValue.GetString() ?? scope;
            if (root.TryGetProperty("CreatedAt", out var createdValue) &&
                createdValue.TryGetDateTimeOffset(out var parsedCreatedAt))
                createdAt = parsedCreatedAt.UtcDateTime;
            return new SnapshotEntry(id, scope, new DateTimeOffset(createdAt, TimeSpan.Zero),
                new FileInfo(path).Length, true, null);
        }
        catch (JsonException ex)
        {
            return new SnapshotEntry(id, scope, new DateTimeOffset(createdAt, TimeSpan.Zero),
                new FileInfo(path).Length, false, ex.Message);
        }
    }

    private string? FindPath(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !Directory.Exists(_root)) return null;
        return Directory.EnumerateFiles(_root, id + ".json", SearchOption.AllDirectories).SingleOrDefault();
    }

    public sealed record SnapshotEntry(
        string Id,
        string Scope,
        DateTimeOffset CreatedAt,
        long SizeBytes,
        bool IsValid,
        string? Error);

    public sealed record SnapshotComparison(
        string FirstId,
        string SecondId,
        bool AreEqual,
        string FirstJson,
        string SecondJson);

    public sealed record Snapshot<T>(string Id, string Scope, DateTimeOffset CreatedAt, T State);
}
