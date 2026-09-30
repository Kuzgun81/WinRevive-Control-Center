namespace WinRevive.Models;

public sealed record MemoryDiagnostics(
    long TotalBytes,
    long AvailableBytes,
    long UsedBytes,
    long CommitLimitBytes,
    long CommittedBytes,
    int ProcessCount,
    string Pressure,
    string Summary);
