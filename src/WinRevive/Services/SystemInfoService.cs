using System.Runtime.InteropServices;
using WinRevive.Models;

namespace WinRevive.Services;

public sealed class SystemInfoService
{
    public SystemInfo Read()
    {
        var memoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);
        var memory = memoryBytes > 0 ? $"{memoryBytes / 1024d / 1024d / 1024d:0.0} GB kullanılabilir" : "Bilinmiyor";
        var disk = $"{drive.AvailableFreeSpace / 1024d / 1024d / 1024d:0.0} GB boş / {drive.TotalSize / 1024d / 1024d / 1024d:0.0} GB";
        return new(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{Environment.OSVersion.VersionString} ({RuntimeInformation.OSArchitecture})" : "Windows dışı ortam",
            Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? Environment.ProcessorCount + " mantıksal işlemci",
            memory,
            disk);
    }
}
