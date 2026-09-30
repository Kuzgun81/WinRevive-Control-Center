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

    [Fact]
    public void ListAndDelete_ManagesSnapshotFiles()
        {
            var root = Path.Combine(Path.GetTempPath(), "WinReviveTests", Guid.NewGuid().ToString("N"));
            try
            {
                var service = new SnapshotService(root);
                var id = service.Create("test", new TestState("before", 7));

                var listed = service.List();

                Assert.Contains(listed, item => item.Id == id && item.IsValid);
                Assert.True(service.Delete(id));
                Assert.Empty(service.List());
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

    [Fact]
    public void List_MarksCorruptSnapshotWithoutHidingIt()
        {
            var root = Path.Combine(Path.GetTempPath(), "WinReviveTests", Guid.NewGuid().ToString("N"));
            try
            {
                var directory = Path.Combine(root, "test");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "corrupt.json"), "{");

                var listed = new SnapshotService(root).List();

                var item = Assert.Single(listed);
                Assert.False(item.IsValid);
                Assert.NotNull(item.Error);
            }
            finally
            {
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

    private sealed record TestState(string Value, int Number);
}
