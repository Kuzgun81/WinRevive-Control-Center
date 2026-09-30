using System.Text.Json;

namespace WinRevive.Services;

public sealed class SettingsService
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "settings.json");

    public string Theme
    {
        get
        {
            try
            {
                if (File.Exists(_path))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(_path))?.Theme ?? "dark";
            }
            catch { }
            return "dark";
        }
        set
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(new Settings(value), new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    private sealed record Settings(string Theme);
}
