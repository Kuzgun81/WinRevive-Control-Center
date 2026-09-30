using Microsoft.Win32;

namespace WinRevive.Services;

public sealed class CapabilityService
{
    public string ReadWindowsVersion()
    {
        if (!OperatingSystem.IsWindows())
            return "Windows dışı ortam";

        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);
        var product = key?.GetValue("ProductName")?.ToString() ?? "Windows";
        var displayVersion = key?.GetValue("DisplayVersion")?.ToString();
        var build = key?.GetValue("CurrentBuild")?.ToString();
        var suffix = string.Join(" ", new[] { displayVersion, build }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(suffix) ? product : $"{product} ({suffix})";
    }

    public string ReadSummary()
    {
        if (!OperatingSystem.IsWindows())
            return "Windows entegrasyonları kullanılamıyor";

        var registry = CanReadCurrentUserRegistry() ? "Registry ✓" : "Registry ✕";
        return $"{registry} • HKCU değişiklikleri destekleniyor • Yönetici gerekmez";
    }

    private static bool CanReadCurrentUserRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser;
            return key is not null;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
