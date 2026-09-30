using Microsoft.Win32;
using WinRevive.Models;

namespace WinRevive.Services;

public sealed class StartupService
{
    private static readonly string[] RunKeyPaths =
    [
        @"Software\Microsoft\Windows\CurrentVersion\Run",
        @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
    ];

    public IReadOnlyList<StartupEntry> ReadEntries()
    {
        if (!OperatingSystem.IsWindows())
            return [];

        var entries = new List<StartupEntry>();
        ReadScope(Registry.CurrentUser, "Kullanıcı", entries);
        ReadScope(Registry.LocalMachine, "Bilgisayar", entries);
        return entries;
    }

    private static void ReadScope(RegistryKey root, string scope, ICollection<StartupEntry> entries)
    {
        foreach (var path in RunKeyPaths)
        {
            using var key = root.OpenSubKey(path, false);
            if (key is null) continue;
            foreach (var name in key.GetValueNames())
            {
                var command = key.GetValue(name)?.ToString();
                if (!string.IsNullOrWhiteSpace(command))
                    entries.Add(new StartupEntry(name, command, scope, path));
            }
        }
    }

    public void DisableCurrentUserEntry(StartupEntry entry)
    {
        EnsureUserEntry(entry);
        using var key = Registry.CurrentUser.OpenSubKey(entry.RegistryPath, true)
            ?? throw new InvalidOperationException("Başlangıç kayıt anahtarı bulunamadı.");
        key.DeleteValue(entry.Name, false);
    }

    public void EnableCurrentUserEntry(StartupEntry entry)
    {
        EnsureUserEntry(entry);
        using var key = Registry.CurrentUser.CreateSubKey(entry.RegistryPath);
        key.SetValue(entry.Name, entry.Command, RegistryValueKind.String);
    }

    private static void EnsureUserEntry(StartupEntry entry)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Başlangıç yönetimi yalnızca Windows'ta kullanılabilir.");
        if (!string.Equals(entry.Scope, "Kullanıcı", StringComparison.Ordinal))
            throw new InvalidOperationException("Bilgisayar kapsamındaki başlangıç girdileri bu sürümde değiştirilemez.");
        if (!RunKeyPaths.Contains(entry.RegistryPath, StringComparer.Ordinal))
            throw new InvalidOperationException("Desteklenmeyen başlangıç kayıt yolu.");
    }
}
