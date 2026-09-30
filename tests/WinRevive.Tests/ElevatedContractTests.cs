using WinRevive.Contracts;
using Xunit;

namespace WinRevive.Tests;

public sealed class ElevatedContractTests
{
    [Fact]
    public void ProtocolUsesAllowlistedOperation()
    {
        var request = new ElevatedRequest(1, "nonce", ElevatedOperation.RepairWindowsSearch);

        Assert.Equal(1, request.ProtocolVersion);
        Assert.Equal(ElevatedOperation.RepairWindowsSearch, request.Operation);
    }
}
