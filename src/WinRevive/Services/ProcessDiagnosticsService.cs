using WinRevive.Models;

namespace WinRevive.Services;

public sealed class ProcessDiagnosticsService
{
    private static readonly IReadOnlyDictionary<string, string> ProtectedProcesses =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["csrss"] = "Windows critical process",
            ["dwm"] = "Desktop Window Manager",
            ["explorer"] = "Windows shell",
            ["lsass"] = "Authentication and security",
            ["services"] = "Windows service manager",
            ["smss"] = "Windows session manager",
            ["svchost"] = "Windows service host",
            ["system"] = "Windows kernel",
            ["wininit"] = "Windows initialization",
            ["winlogon"] = "Windows logon"
        };

    public IReadOnlyList<ProcessInfo> Read()
    {
        if (!OperatingSystem.IsWindows()) return [];
        var currentId = Environment.ProcessId;
        return Process.GetProcesses()
            .Select(process => ReadProcess(process, currentId))
            .Where(item => item is not null)
            .Cast<ProcessInfo>()
            .OrderByDescending(item => item.WorkingSetBytes)
            .ToArray();
    }

    public string RequestSafeStop(ProcessInfo processInfo)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Süreç yönetimi yalnızca Windows'ta kullanılabilir.");
        if (processInfo.IsProtected)
            throw new InvalidOperationException($"Korunan süreç durdurulamaz: {processInfo.ProtectionReason}.");
        if (processInfo.Id == Environment.ProcessId)
            throw new InvalidOperationException("WinRevive kendi sürecini durduramaz.");

        using var process = Process.GetProcessById(processInfo.Id);
        if (process.HasExited) return "Süreç zaten kapalı.";
        if (!process.CloseMainWindow())
            throw new InvalidOperationException("Süreç güvenli kapanış isteğini kabul etmedi.");
        return process.WaitForExit(5000)
            ? $"{processInfo.Name} güvenli kapanışla durduruldu."
            : $"{processInfo.Name} kapanış isteği aldı; zorla sonlandırılmadı.";
    }

    private static ProcessInfo? ReadProcess(Process process, int currentId)
    {
        try
        {
            var name = process.ProcessName;
            var protectedReason = ProtectedProcesses.TryGetValue(name, out var reason)
                ? reason
                : process.Id == currentId ? "WinRevive işlemi" : string.Empty;
            return new(
                process.Id,
                name,
                process.WorkingSet64,
                process.PrivateMemorySize64,
                process.Responding,
                !string.IsNullOrWhiteSpace(protectedReason),
                protectedReason);
        }
        catch (System.ComponentModel.Win32Exception) { return null; }
        catch (InvalidOperationException) { return null; }
    }
}
