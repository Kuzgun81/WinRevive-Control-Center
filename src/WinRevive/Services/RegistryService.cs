using Microsoft.Win32;

namespace WinRevive.Services;

public sealed class RegistryService
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public int? ReadTransparency()
    {
        if (!OperatingSystem.IsWindows()) return null;
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, false);
        return key?.GetValue("EnableTransparency") is int value ? value : null;
    }

    public void WriteTransparency(int value)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Bu ayar yalnızca Windows'ta kullanılabilir.");
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
        key.SetValue("EnableTransparency", value, RegistryValueKind.DWord);
    }
}
