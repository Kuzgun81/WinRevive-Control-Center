using WinRevive.Services;
using Xunit;

namespace WinRevive.Tests;

public sealed class CleanupServiceTests
{
    [Fact]
    public void ScanAndDelete_OnlyTouchesConfiguredCleanupRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "WinReviveTests", Guid.NewGuid().ToString("N"));
        var outside = Path.Combine(Path.GetTempPath(), "WinReviveTestsOutside", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(outside);
        var insideFile = Path.Combine(root, "cache.tmp");
        var outsideFile = Path.Combine(outside, "keep.txt");
        File.WriteAllText(insideFile, "temporary");
        File.WriteAllText(outsideFile, "keep");

        try
        {
            var service = new CleanupService([new("test", "Test cleanup", root)]);
            var items = service.Scan();
            var result = service.Delete(items);

            Assert.Single(items);
            Assert.Equal(1, result.DeletedFiles);
            Assert.False(File.Exists(insideFile));
            Assert.True(File.Exists(outsideFile));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
            if (Directory.Exists(outside)) Directory.Delete(outside, true);
        }
    }
}
