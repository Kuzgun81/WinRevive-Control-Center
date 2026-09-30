using System.Management;

namespace WinRevive.Services;

public sealed class WindowsDiagnosticsService
{
    public string ReadGraphics()
    {
        var names = Query("Win32_VideoController", "Name")
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return names.Length == 0 ? "GPU: Bilinmiyor" : $"GPU: {string.Join(", ", names)}";
    }

    public string ReadBattery()
    {
        var batteries = Query("Win32_Battery", "EstimatedChargeRemaining").ToArray();
        if (batteries.Length == 0) return "Pil: Masaüstü veya bilgi yok";
        return $"Pil: %{batteries.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "Bilinmiyor"}";
    }

    public string ReadPagefile()
    {
        var files = Query("Win32_PageFileUsage", "Name", "AllocatedBaseSize", "CurrentUsage")
            .ToArray();
        return files.Length == 0 ? "Pagefile: Bilgi yok" : $"Pagefile: {files.Length} kayıt";
    }

    private static IEnumerable<string> Query(string className, params string[] properties)
    {
        if (!OperatingSystem.IsWindows()) return [];
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT {string.Join(", ", properties)} FROM {className}");
            return searcher.Get()
                .Cast<ManagementObject>()
                .Select(item => string.Join(" ", properties.Select(property => item[property]?.ToString())))
                .ToArray();
        }
        catch (ManagementException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }
}
