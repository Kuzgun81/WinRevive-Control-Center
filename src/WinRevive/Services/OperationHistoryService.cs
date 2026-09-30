using System.Text.Json;

namespace WinRevive.Services;

public sealed class OperationHistoryService
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "history.jsonl");

    public void Record(string operation, string status, string detail)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var item = JsonSerializer.Serialize(new HistoryItem(DateTimeOffset.UtcNow, operation, status, detail));
        File.AppendAllText(_path, item + Environment.NewLine);
    }

    public HistoryItem? ReadLatest()
    {
        if (!File.Exists(_path)) return null;
        return ReadRecent(1).FirstOrDefault();
    }

    public IReadOnlyList<HistoryItem> ReadRecent(int limit = 10)
    {
        if (!File.Exists(_path)) return [];
        return File.ReadLines(_path)
            .Reverse()
            .Take(limit)
            .Select(TryDeserialize)
            .Where(item => item is not null)
            .Cast<HistoryItem>()
            .ToArray();
    }

    private static HistoryItem? TryDeserialize(string line)
    {
        try
        {
            return string.IsNullOrWhiteSpace(line)
                ? null
                : JsonSerializer.Deserialize<HistoryItem>(line);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public sealed record HistoryItem(DateTimeOffset Timestamp, string Operation, string Status, string Detail);
}
