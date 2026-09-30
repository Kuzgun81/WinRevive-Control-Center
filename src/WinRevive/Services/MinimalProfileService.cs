using System.Text.Json;

namespace WinRevive.Services;

public sealed class MinimalProfileService
{
    private readonly RegistryService _registry;
    private readonly FileLogger _logger;
    private readonly OperationHistoryService _history;
    private readonly string _backupPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "backups", "minimal-profile.json");

    public MinimalProfileService(RegistryService registry, FileLogger logger, OperationHistoryService history)
    {
        _registry = registry;
        _logger = logger;
        _history = history;
    }

    public string Apply()
    {
        EnsureWindows();
        var backup = new Backup(
            _registry.ReadPersonalizeValue("EnableTransparency"),
            _registry.ReadPersonalizeValue("AppsUseLightTheme"),
            _registry.ReadPersonalizeValue("SystemUsesLightTheme"),
            _registry.ReadExplorerValue("TaskbarAl"),
            DateTimeOffset.UtcNow);
        Directory.CreateDirectory(Path.GetDirectoryName(_backupPath)!);
        File.WriteAllText(_backupPath, JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true }));

        try
        {
            _registry.WritePersonalizeValue("EnableTransparency", 0);
            _registry.WritePersonalizeValue("AppsUseLightTheme", 0);
            _registry.WritePersonalizeValue("SystemUsesLightTheme", 0);
            _registry.WriteExplorerValue("TaskbarAl", 0);
            Verify();
        }
        catch
        {
            Restore(backup);
            throw;
        }
        _logger.Info("Minimal profile applied and verified.");
        _history.Record("Minimal profile", "Applied", "Dark mode, reduced transparency, left taskbar alignment.");
        return "Minimal profil uygulandı ve doğrulandı. Explorer değişiklikleri yeniden oturum açınca görünür olabilir.";
    }

    public string Revert()
    {
        EnsureWindows();
        if (!File.Exists(_backupPath)) return "Minimal profil yedeği bulunamadı.";
        var backup = JsonSerializer.Deserialize<Backup>(File.ReadAllText(_backupPath))
            ?? throw new InvalidOperationException("Minimal profil yedeği okunamadı.");
        RestorePersonalize("EnableTransparency", backup.EnableTransparency);
        RestorePersonalize("AppsUseLightTheme", backup.AppsUseLightTheme);
        RestorePersonalize("SystemUsesLightTheme", backup.SystemUsesLightTheme);
        if (backup.TaskbarAlignment is int alignment) _registry.WriteExplorerValue("TaskbarAl", alignment);
        else _registry.DeleteExplorerValue("TaskbarAl");
        VerifyRestored(backup);
        _history.Record("Minimal profile", "Reverted", "Previous appearance values restored.");
        return "Minimal profil geri alındı.";
    }

    public string Detect()
    {
        if (!OperatingSystem.IsWindows()) return "Windows dışında kullanılamaz.";
        var dark = _registry.ReadPersonalizeValue("AppsUseLightTheme") == 0;
        var reducedTransparency = _registry.ReadPersonalizeValue("EnableTransparency") == 0;
        return dark && reducedTransparency ? "Minimal görünüm etkin olabilir." : "Varsayılan/karma görünüm";
    }

    private void Verify()
    {
        if (_registry.ReadPersonalizeValue("EnableTransparency") != 0 ||
            _registry.ReadPersonalizeValue("AppsUseLightTheme") != 0 ||
            _registry.ReadPersonalizeValue("SystemUsesLightTheme") != 0 ||
            _registry.ReadExplorerValue("TaskbarAl") != 0)
            throw new InvalidOperationException("Minimal profil doğrulanamadı; değişiklikler güvenilir kabul edilmedi.");
    }

    private void RestorePersonalize(string name, int? value)
    {
        if (value is int previous) _registry.WritePersonalizeValue(name, previous);
        else _registry.DeletePersonalizeValue(name);
    }

    private void Restore(Backup backup)
    {
        RestorePersonalize("EnableTransparency", backup.EnableTransparency);
        RestorePersonalize("AppsUseLightTheme", backup.AppsUseLightTheme);
        RestorePersonalize("SystemUsesLightTheme", backup.SystemUsesLightTheme);
        if (backup.TaskbarAlignment is int alignment) _registry.WriteExplorerValue("TaskbarAl", alignment);
        else _registry.DeleteExplorerValue("TaskbarAl");
    }

    private void VerifyRestored(Backup backup)
    {
        if (_registry.ReadPersonalizeValue("EnableTransparency") != backup.EnableTransparency ||
            _registry.ReadPersonalizeValue("AppsUseLightTheme") != backup.AppsUseLightTheme ||
            _registry.ReadPersonalizeValue("SystemUsesLightTheme") != backup.SystemUsesLightTheme ||
            _registry.ReadExplorerValue("TaskbarAl") != backup.TaskbarAlignment)
            throw new InvalidOperationException("Minimal profil geri alma doğrulanamadı.");
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Minimal profil yalnızca Windows'ta kullanılabilir.");
    }

    private sealed record Backup(
        int? EnableTransparency,
        int? AppsUseLightTheme,
        int? SystemUsesLightTheme,
        int? TaskbarAlignment,
        DateTimeOffset CreatedAt);
}
