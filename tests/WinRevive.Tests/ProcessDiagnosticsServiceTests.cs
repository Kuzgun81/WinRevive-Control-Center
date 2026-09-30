using WinRevive.Models;
using WinRevive.Services;
using Xunit;

namespace WinRevive.Tests;

public sealed class ProcessDiagnosticsServiceTests
{
    [Fact]
    public void ProtectedProcessCannotBeSafelyStopped()
    {
        var service = new ProcessDiagnosticsService();
        var process = new ProcessInfo(1, "lsass", 0, 0, true, true, "Authentication and security");

        var exception = Assert.Throws<InvalidOperationException>(() => service.RequestSafeStop(process));

        Assert.Contains("Korunan süreç", exception.Message);
    }
}
