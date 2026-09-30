namespace WinRevive.Services;

public sealed class PowerProfileService
{
    private static readonly IReadOnlyDictionary<string, string> Schemes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["balanced"] = "SCHEME_BALANCED",
            ["performance"] = "SCHEME_MIN",
            ["battery"] = "SCHEME_MAX"
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
        var result = Run($"/setactive {scheme}");
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Güç planı uygulanamadı: {result.StandardError.Trim()}");
        var active = ReadActive();
        if (active.Contains("Could not", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Güç planı doğrulanamadı.");
        return $"Güç profili uygulandı: {profile}.";
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
