namespace WinRevive.Models;

public sealed record ProcessInfo(
    int Id,
    string Name,
    long WorkingSetBytes,
    long PrivateMemoryBytes,
    bool Responding,
    bool IsProtected,
    string ProtectionReason);
