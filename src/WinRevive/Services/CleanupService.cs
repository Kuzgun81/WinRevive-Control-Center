namespace WinRevive.Services;

public sealed class CleanupService
{
    private readonly IReadOnlyList<CleanupLocation> _locations;

    public CleanupService(IReadOnlyList<CleanupLocation>? locations = null)
    {
        _locations = locations ?? CreateDefaultLocations();
    }

    public IReadOnlyList<CleanupItem> Scan()
    {
        if (!OperatingSystem.IsWindows() && _locations.Count == 0)
            throw new PlatformNotSupportedException("Temizlik taraması yalnızca Windows'ta kullanılabilir.");

        return _locations
            .SelectMany(ScanLocation)
            .OrderByDescending(item => item.SizeBytes)
            .ToArray();
    }

    public CleanupResult Delete(IEnumerable<CleanupItem> items)
    {
        var deletedFiles = 0;
        long freedBytes = 0;
        var failures = new List<string>();

        foreach (var item in items)
        {
            if (!IsAllowed(item.Path))
            {
                failures.Add($"{item.Path}: izin verilen temizlik alanı dışında.");
                continue;
            }

            try
            {
                if (!File.Exists(item.Path)) continue;
                var size = new FileInfo(item.Path).Length;
                File.Delete(item.Path);
                deletedFiles++;
                freedBytes += size;
            }
            catch (IOException ex) { failures.Add($"{item.Path}: {ex.Message}"); }
            catch (UnauthorizedAccessException ex) { failures.Add($"{item.Path}: {ex.Message}"); }
        }

        return new CleanupResult(deletedFiles, freedBytes, failures);
    }

    private IEnumerable<CleanupItem> ScanLocation(CleanupLocation location)
    {
        if (!Directory.Exists(location.Path)) yield break;

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(location.Path, "*", SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException) { yield break; }
        catch (IOException) { yield break; }

        foreach (var path in files)
        {
            if (!IsAllowed(path)) continue;
            FileInfo info;
            try { info = new FileInfo(path); }
            catch (IOException) { continue; }
            if ((info.Attributes & FileAttributes.ReparsePoint) != 0) continue;
            yield return new CleanupItem(location.Id, location.DisplayName, path, info.Length);
        }
    }

    private bool IsAllowed(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return _locations.Any(location =>
            fullPath.StartsWith(Path.GetFullPath(location.Path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<CleanupLocation> CreateDefaultLocations()
    {
        var locations = new List<CleanupLocation>();
        var userTemp = Path.GetTempPath();
        if (!string.IsNullOrWhiteSpace(userTemp))
            locations.Add(new("user-temp", "Kullanıcı geçici dosyaları", userTemp));

        if (OperatingSystem.IsWindows())
        {
            var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (!string.IsNullOrWhiteSpace(windows))
                locations.Add(new("windows-temp", "Windows geçici dosyaları", Path.Combine(windows, "Temp")));
        }

        return locations;
    }

    public sealed record CleanupLocation(string Id, string DisplayName, string Path);
    public sealed record CleanupItem(string LocationId, string LocationName, string Path, long SizeBytes);
    public sealed record CleanupResult(int DeletedFiles, long FreedBytes, IReadOnlyList<string> Failures);
}
