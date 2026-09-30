using System.Runtime.InteropServices;
using WinRevive.Models;

namespace WinRevive.Services;

public sealed class MemoryDiagnosticsService
{
    public MemoryDiagnostics Read()
    {
        if (!OperatingSystem.IsWindows())
            return new(0, 0, 0, 0, 0, 0, "Desteklenmiyor", "Bellek tanılaması yalnızca Windows'ta kullanılabilir.");

        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status))
            throw new InvalidOperationException($"Windows bellek bilgisi okunamadı: {Marshal.GetLastWin32Error()}.");

        var used = status.TotalPhysical - status.AvailablePhysical;
        var ratio = status.TotalPhysical == 0 ? 0 : (double)used / status.TotalPhysical;
        var pressure = ratio >= 0.9 ? "Yüksek" : ratio >= 0.75 ? "Orta" : "Düşük";
        var processCount = Process.GetProcesses().Length;
        var performance = new PerformanceInformation { Size = (uint)Marshal.SizeOf<PerformanceInformation>() };
        var hasCommit = GetPerformanceInfo(ref performance, performance.Size);
        var committed = hasCommit ? performance.CommitTotal * performance.PageSize : 0;
        var commitLimit = hasCommit ? performance.CommitLimit * performance.PageSize : 0;
        return new(
            (long)status.TotalPhysical,
            (long)status.AvailablePhysical,
            (long)used,
            (long)commitLimit,
            (long)committed,
            processCount,
            pressure,
            $"RAM kullanımı %{ratio * 100:0}; kullanılabilir {Format(status.AvailablePhysical)}; commit {(hasCommit ? $"{Format(committed)} / {Format(commitLimit)}" : "bilinmiyor")}.");
    }

    private static string Format(ulong bytes) => $"{bytes / 1024d / 1024d / 1024d:0.0} GB";

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool GetPerformanceInfo(ref PerformanceInformation performanceInformation, uint size);

    [StructLayout(LayoutKind.Sequential)]
    private struct PerformanceInformation
    {
        public uint Size;
        public nuint CommitTotal;
        public nuint CommitLimit;
        public nuint CommitPeak;
        public nuint PhysicalTotal;
        public nuint PhysicalAvailable;
        public nuint SystemCache;
        public nuint KernelTotal;
        public nuint KernelPaged;
        public nuint KernelNonPaged;
        public nuint PageSize;
        public uint HandlesCount;
        public uint ProcessCount;
        public uint ThreadCount;
    }
}
