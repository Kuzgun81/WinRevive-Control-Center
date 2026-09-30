using System.Text.Json;

namespace WinRevive.Services;

public sealed class FileLogger
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "logs", "app.log");
    private readonly object _gate = new();

    public void Info(string message) => Write("Information", message, null);
    public void Error(string message, Exception? exception) => Write("Error", message, exception?.ToString());

    private void Write(string level, string message, string? exception)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var entry = JsonSerializer.Serialize(new { timestamp = DateTimeOffset.UtcNow, level, message, exception });
            lock (_gate) File.AppendAllText(_path, entry + Environment.NewLine);
        }
        catch (IOException)
        {
            // Logging must not take down the application when the log path is unavailable.
        }
    }
}
