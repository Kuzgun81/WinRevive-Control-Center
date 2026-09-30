using WinRevive.Services;
using Xunit;

namespace WinRevive.Tests;

public sealed class SnapshotServiceTests
{
    [Fact]
    public void CreateAndReadLatest_RoundTripsState()
    {
        var root = Path.Combine(Path.GetTempPath(), "WinReviveTests", Guid.NewGuid().ToString("N"));
        try
        {
            var service = new SnapshotService(root);
            var id = service.Create("test", new TestState("before", 7));

            var snapshot = service.ReadLatest<TestState>("test");

            Assert.NotNull(snapshot);
            Assert.Equal(id, snapshot!.Id);
            Assert.Equal("before", snapshot.State.Value);
            Assert.Equal(7, snapshot.State.Number);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private sealed record TestState(string Value, int Number);
}
