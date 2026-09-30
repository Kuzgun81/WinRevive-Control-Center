using Microsoft.Win32;

namespace WinRevive.Services;

public sealed class RegistryService
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string ExplorerKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

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

    public int? ReadPersonalizeValue(string name)
    {
        if (!OperatingSystem.IsWindows()) return null;
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath, false);
        return key?.GetValue(name) is int value ? value : null;
    }

    public void WritePersonalizeValue(string name, int value) => WriteDword(KeyPath, name, value);

    public int? ReadExplorerValue(string name)
    {
        if (!OperatingSystem.IsWindows()) return null;
        using var key = Registry.CurrentUser.OpenSubKey(ExplorerKeyPath, false);
        return key?.GetValue(name) is int value ? value : null;
    }

    public void WriteExplorerValue(string name, int value) => WriteDword(ExplorerKeyPath, name, value);

    public void DeletePersonalizeValue(string name) => DeleteValue(KeyPath, name);
    public void DeleteExplorerValue(string name) => DeleteValue(ExplorerKeyPath, name);

    private static void WriteDword(string path, string name, int value)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Bu ayar yalnızca Windows'ta kullanılabilir.");
        using var key = Registry.CurrentUser.CreateSubKey(path);
        key.SetValue(name, value, RegistryValueKind.DWord);
    }

    private static void DeleteValue(string path, string name)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Bu ayar yalnızca Windows'ta kullanılabilir.");
        using var key = Registry.CurrentUser.OpenSubKey(path, true);
        key?.DeleteValue(name, false);
    }
}
