using Microsoft.Win32;
using WinRevive.Models;

namespace WinRevive.Services;

public sealed class ApplicationInventoryService
{
    private static readonly string[] UninstallPaths =
    [
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
        @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];

    public IReadOnlyList<InstalledApplication> Read()
    {
        if (!OperatingSystem.IsWindows()) return [];
        var result = new List<InstalledApplication>();
        ReadScope(Registry.CurrentUser, "Kullanıcı", result);
        ReadScope(Registry.LocalMachine, "Bilgisayar", result);
        return result
            .GroupBy(item => $"{item.Name}|{item.Version}|{item.Scope}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.Name)
            .ToArray();
    }

    private static void ReadScope(RegistryKey root, string scope, ICollection<InstalledApplication> result)
    {
        foreach (var path in UninstallPaths)
        {
            using var key = root.OpenSubKey(path, false);
            if (key is null) continue;
            foreach (var name in key.GetSubKeyNames())
            {
                using var app = key.OpenSubKey(name, false);
                var displayName = app?.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrWhiteSpace(displayName)) continue;
                result.Add(new InstalledApplication(
                    displayName,
                    app?.GetValue("DisplayVersion")?.ToString() ?? "Bilinmiyor",
                    app?.GetValue("Publisher")?.ToString() ?? "Bilinmiyor",
                    scope));
            }
        }
    }
}
