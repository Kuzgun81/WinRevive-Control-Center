using System.Runtime.InteropServices;
using WinRevive.Models;

namespace WinRevive.Services;

public sealed class SystemInfoService
{
    private readonly CapabilityService _capabilities;
    private readonly StartupService _startup;
    private readonly SearchService _search;
    private readonly WindowsDiagnosticsService _windowsDiagnostics;
    private readonly SecurityDiagnosticsService _security;
    private readonly ApplicationInventoryService _applications;

    public SystemInfoService(CapabilityService capabilities, StartupService startup, SearchService search, WindowsDiagnosticsService windowsDiagnostics, SecurityDiagnosticsService security, ApplicationInventoryService applications)
    {
        _capabilities = capabilities;
        _startup = startup;
        _search = search;
        _windowsDiagnostics = windowsDiagnostics;
        _security = security;
        _applications = applications;
    }

    public SystemInfo Read()
    {
        var memory = new MemoryDiagnosticsService().Read();
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);
        var drive = systemRoot is null ? null : new DriveInfo(systemRoot);
        var memoryUsage = memory.TotalBytes == 0 ? 0 : memory.UsedBytes * 100d / memory.TotalBytes;
        var memorySummary = memory.TotalBytes > 0
            ? $"{FormatBytes(memory.TotalBytes)} toplam • {FormatBytes(memory.AvailableBytes)} kullanılabilir • %{memoryUsage:0} kullanım"
            : memory.Summary;
        var disk = drive is null
            ? "Bilinmiyor"
            : $"{drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0.0} GB boş / {drive.TotalSize / 1024d / 1024d / 1024d:0.0} GB";
        return new(
            _capabilities.ReadWindowsVersion(),
            Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? Environment.ProcessorCount + " mantıksal işlemci",
            memorySummary,
            disk,
            RuntimeInformation.OSArchitecture.ToString(),
            Environment.ProcessorCount.ToString(),
            _capabilities.ReadSummary(),
            $"{_startup.ReadEntries().Count} kayıt bulundu",
            _search.ReadStatus(),
            _windowsDiagnostics.ReadGraphics(),
            _windowsDiagnostics.ReadBattery(),
            _windowsDiagnostics.ReadPagefile(),
            _security.ReadSummary(),
            $"{_applications.Read().Count} kurulu uygulama bulundu");
    }

    private static string FormatBytes(long bytes) =>
        $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
}
