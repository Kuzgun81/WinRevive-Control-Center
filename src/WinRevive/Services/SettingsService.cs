using System.Text.Json;

namespace WinRevive.Services;

public sealed class SettingsService
{
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "settings.json");

    public string Theme
    {
        get => Read().Theme;
        set => Write(Read() with { Theme = value });
    }

    public bool FirstRunCompleted
    {
        get => Read().FirstRunCompleted;
        set => Write(Read() with { FirstRunCompleted = value });
    }

    public string PreferredProfile
    {
        get => Read().PreferredProfile;
        set => Write(Read() with { PreferredProfile = value });
    }

    private Settings Read()
    {
        try
        {
            return File.Exists(_path)
                ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(_path)) ?? new Settings("dark", false, "general")
                : new Settings("dark", false, "general");
        }
        catch (JsonException)
        {
            return new Settings("dark", false, "general");
        }
    }

    private void Write(Settings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed record Settings(string Theme, bool FirstRunCompleted, string PreferredProfile);
}
