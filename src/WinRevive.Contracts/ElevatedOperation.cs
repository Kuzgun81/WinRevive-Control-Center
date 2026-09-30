namespace WinRevive.Contracts;

public enum ElevatedOperation
{
    RepairWindowsSearch,
    RebuildWindowsSearch
}

public sealed record ElevatedRequest(
    int ProtocolVersion,
    string Nonce,
    ElevatedOperation Operation);

public sealed record ElevatedResponse(
    bool Success,
    string Message);
