using System.Text.Json;

namespace WinRevive.Services;

public sealed class TransparencyOptimizationRule
{
    private readonly RegistryService _registry;
    private readonly FileLogger _logger;
    private readonly string _backupPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinRevive", "backups", "transparency.json");

    public TransparencyOptimizationRule(RegistryService registry, FileLogger logger)
    {
        _registry = registry;
        _logger = logger;
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
        Directory.CreateDirectory(Path.GetDirectoryName(_backupPath)!);
        File.WriteAllText(_backupPath, JsonSerializer.Serialize(new Backup(current, DateTimeOffset.UtcNow)));
        _registry.WriteTransparency(0);
        if (_registry.ReadTransparency() != 0) throw new InvalidOperationException("Değişiklik doğrulanamadı.");
        _logger.Info($"Transparency applied; previous value: {current}.");
        return "Uygulandı ve doğrulandı.";
    }

    public string Revert()
    {
        if (!File.Exists(_backupPath)) return "Geri alınacak yedek bulunamadı.";
        var backup = JsonSerializer.Deserialize<Backup>(File.ReadAllText(_backupPath))
            ?? throw new InvalidOperationException("Yedek okunamadı.");
        _registry.WriteTransparency(backup.Value);
        if (_registry.ReadTransparency() != backup.Value) throw new InvalidOperationException("Geri alma doğrulanamadı.");
        _logger.Info($"Transparency reverted to {backup.Value}.");
        return "Geri alındı ve doğrulandı.";
    }

    private sealed record Backup(int Value, DateTimeOffset CreatedAt);
}
