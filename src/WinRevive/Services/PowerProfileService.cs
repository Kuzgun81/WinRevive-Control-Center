namespace WinRevive.Services;

public sealed class PowerProfileService
{
    private static readonly IReadOnlyDictionary<string, (string Alias, string Guid)> Schemes =
        new Dictionary<string, (string Alias, string Guid)>(StringComparer.OrdinalIgnoreCase)
        {
            ["balanced"] = ("SCHEME_BALANCED", "381b4222-f694-41f0-9685-ff5bb260df2e"),
            ["performance"] = ("SCHEME_MIN", "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c"),
            ["battery"] = ("SCHEME_MAX", "a1841308-3541-4fab-bc81-f71556f20b4a")
        };

    public string ReadActive()
    {
        if (!OperatingSystem.IsWindows()) return "Windows dışında kullanılamaz.";
        var result = Run("/getactivescheme");
        return result.ExitCode == 0 ? result.StandardOutput.Trim() : "Güç planı okunamadı.";
    }

    public string Apply(string profile)
    {
        if (!Schemes.TryGetValue(profile, out var scheme))
            throw new ArgumentException("Desteklenmeyen güç profili.", nameof(profile));
        var result = Run($"/setactive {scheme.Alias}");
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Güç planı uygulanamadı: {result.StandardError.Trim()}");
        var active = ReadActive();
        if (!active.Contains(scheme.Guid, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Güç planı doğrulanamadı. Aktif plan: {active}");
        return $"Güç profili uygulandı ve doğrulandı: {profile}.";
    }

    private static ProcessResult Run(string arguments)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Güç profilleri yalnızca Windows'ta kullanılabilir.");
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException("powercfg.exe başlatılamadı.");
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
