namespace WinRevive.Models;

public sealed record SystemInfo(
    string WindowsVersion,
    string Processor,
    string Memory,
    string Disk,
    string Architecture,
    string LogicalProcessors,
    string Capabilities,
    string StartupSummary,
    string SearchSummary);
