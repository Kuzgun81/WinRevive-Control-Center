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
        var line = File.ReadLines(_path).LastOrDefault();
        return string.IsNullOrWhiteSpace(line) ? null : JsonSerializer.Deserialize<HistoryItem>(line);
    }

    public sealed record HistoryItem(DateTimeOffset Timestamp, string Operation, string Status, string Detail);
}
