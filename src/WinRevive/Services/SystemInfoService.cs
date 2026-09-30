using System.Runtime.InteropServices;
using WinRevive.Models;

namespace WinRevive.Services;

public sealed class SystemInfoService
{
    private readonly CapabilityService _capabilities;
    private readonly StartupService _startup;
    private readonly SearchService _search;

    public SystemInfoService(CapabilityService capabilities, StartupService startup, SearchService search)
    {
        _capabilities = capabilities;
        _startup = startup;
        _search = search;
    }

    public SystemInfo Read()
    {
        var memoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);
        var drive = systemRoot is null ? null : new DriveInfo(systemRoot);
        var memory = memoryBytes > 0 ? $"{memoryBytes / 1024d / 1024d / 1024d:0.0} GB kullanılabilir" : "Bilinmiyor";
        var disk = drive is null
            ? "Bilinmiyor"
            : $"{drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0.0} GB boş / {drive.TotalSize / 1024d / 1024d / 1024d:0.0} GB";
        return new(
            _capabilities.ReadWindowsVersion(),
            Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? Environment.ProcessorCount + " mantıksal işlemci",
            memory,
            disk,
            RuntimeInformation.OSArchitecture.ToString(),
            Environment.ProcessorCount.ToString(),
            _capabilities.ReadSummary(),
            $"{_startup.ReadEntries().Count} kayıt bulundu",
            _search.ReadStatus());
    }
}
