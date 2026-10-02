using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WinRevive.Services;

public sealed class PersonalizationService
{
    private const string AccentPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent";
    private const string DesktopPath = @"Control Panel\Desktop";
    private readonly SnapshotService _snapshots;
    private readonly OperationHistoryService _history;

    public PersonalizationService(SnapshotService snapshots, OperationHistoryService history)
    {
        _snapshots = snapshots;
        _history = history;
    }

    public string ApplyAccent(string hexColor)
    {
        EnsureWindows();
        if (!uint.TryParse(hexColor, System.Globalization.NumberStyles.HexNumber, null, out var color))
            throw new ArgumentException("Geçersiz accent rengi.", nameof(hexColor));
        var previous = ReadDword(AccentPath, "AccentColorMenu");
        var snapshotId = _snapshots.Create("personalization-accent", new AccentState(previous));
        WriteDword(AccentPath, "AccentColorMenu", unchecked((int)color));
        if (ReadDword(AccentPath, "AccentColorMenu") != unchecked((int)color))
            throw new InvalidOperationException("Accent rengi doğrulanamadı.");
        BroadcastThemeChange();
        _history.Record("Personalization", "Applied", $"Accent {hexColor}; snapshot {snapshotId}.");
        return $"Accent rengi uygulandı ve Windows'a bildirildi: #{hexColor}.";
    }

    public string ApplyWallpaper(string path)
    {
        EnsureWindows();
        if (!File.Exists(path)) throw new FileNotFoundException("Duvar kâğıdı dosyası bulunamadı.", path);
        var extension = Path.GetExtension(path);
        if (!new[] { ".jpg", ".jpeg", ".png", ".bmp" }.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Yalnızca JPG, PNG ve BMP duvar kâğıtları desteklenir.");
        var previous = ReadString(DesktopPath, "Wallpaper");
        var snapshotId = _snapshots.Create("personalization-wallpaper", new WallpaperState(previous));
        WriteString(DesktopPath, "Wallpaper", Path.GetFullPath(path));
        ApplyWallpaperNative(Path.GetFullPath(path));
        if (!string.Equals(ReadString(DesktopPath, "Wallpaper"), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Duvar kâğıdı yolu doğrulanamadı.");
        _history.Record("Personalization", "Applied", $"Wallpaper; snapshot {snapshotId}.");
        return "Duvar kâğıdı uygulandı ve doğrulandı.";
    }

    public string RevertWallpaper()
    {
        EnsureWindows();
        var snapshot = _snapshots.ReadLatest<WallpaperState>("personalization-wallpaper")
            ?? throw new InvalidOperationException("Duvar kâğıdı snapshot'ı bulunamadı.");
        if (string.IsNullOrWhiteSpace(snapshot.State.Path))
        {
            DeleteValue(DesktopPath, "Wallpaper");
        }
        else
        {
            WriteString(DesktopPath, "Wallpaper", snapshot.State.Path);
            ApplyWallpaperNative(snapshot.State.Path);
        }

        _history.Record("Personalization", "Reverted", $"Wallpaper snapshot {snapshot.Id}.");
        return "Duvar kâğıdı geri alındı.";
    }

    public string RevertAccent()
    {
        EnsureWindows();
        var snapshot = _snapshots.ReadLatest<AccentState>("personalization-accent")
            ?? throw new InvalidOperationException("Accent snapshot bulunamadı.");
        if (snapshot.State.Value is int value) WriteDword(AccentPath, "AccentColorMenu", value);
        else DeleteValue(AccentPath, "AccentColorMenu");
        BroadcastThemeChange();
        _history.Record("Personalization", "Reverted", $"Accent snapshot {snapshot.Id}.");
        return "Accent rengi geri alındı.";
    }

    private static int? ReadDword(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path, false);
        return key?.GetValue(name) is int value ? value : null;
    }

    private static string? ReadString(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path, false);
        return key?.GetValue(name)?.ToString();
    }

    private static void WriteDword(string path, string name, int value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(path);
        key.SetValue(name, value, RegistryValueKind.DWord);
    }

    private static void WriteString(string path, string name, string value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(path);
        key.SetValue(name, value, RegistryValueKind.String);
    }

    private static void DeleteValue(string path, string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(path, true);
        key?.DeleteValue(name, false);
    }

    private static void ApplyWallpaperNative(string path)
    {
        if (!SystemParametersInfo(20, 0, path, 0x01 | 0x02))
            throw new InvalidOperationException("Windows duvar kâğıdını uygulayamadı.");
    }

    private static void BroadcastThemeChange()
    {
        SendMessageTimeout(new IntPtr(0xFFFF), 0x001A, IntPtr.Zero, IntPtr.Zero, 0x0002, 1000, out _);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, string value, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr window,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        uint flags,
        uint timeout,
        out IntPtr result);

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Kişiselleştirme yalnızca Windows'ta kullanılabilir.");
    }

    private sealed record AccentState(int? Value);
    private sealed record WallpaperState(string? Path);
}
