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
                    entries.Add(new StartupEntry(name, command, scope));
            }
        }
    }
}
