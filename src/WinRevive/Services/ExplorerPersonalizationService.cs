using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WinRevive.Services;

public sealed class ExplorerPersonalizationService
{
    private const string ExplorerAdvancedPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private readonly SnapshotService _snapshots;
    private readonly OperationHistoryService _history;

    public ExplorerPersonalizationService(SnapshotService snapshots, OperationHistoryService history)
    {
        _snapshots = snapshots;
        _history = history;
    }

    public string ApplyFileExtensions(bool show)
    {
        return ApplyDword("file-extensions", "HideFileExt", show ? 0 : 1,
            show ? "Dosya uzantıları gösteriliyor." : "Dosya uzantıları gizleniyor.");
    }

    public string ApplyHiddenFiles(bool show)
    {
        return ApplyDword("hidden-files", "Hidden", show ? 1 : 2,
            show ? "Gizli dosyalar gösteriliyor." : "Gizli dosyalar gizleniyor.");
    }

    public string ApplyCompactView(bool enabled)
    {
        return ApplyDword("compact-view", "UseCompactMode", enabled ? 1 : 0,
            enabled ? "Explorer kompakt görünüm kullanıyor." : "Explorer normal görünüm kullanıyor.");
    }

    public string ApplyLaunchLocation(bool thisPc)
    {
        return ApplyDword("launch-location", "LaunchTo", thisPc ? 1 : 2,
            thisPc ? "Explorer Bu Bilgisayar ile açılacak." : "Explorer Ana Sayfa ile açılacak.");
    }

    public string Revert(string scope, string valueName)
    {
        EnsureWindows();
        var snapshot = _snapshots.ReadLatest<ExplorerValue>(scope)
            ?? throw new InvalidOperationException($"{scope} snapshot'ı bulunamadı.");
        WriteValue(valueName, snapshot.State.Value);
        if (ReadValue(valueName) != snapshot.State.Value)
            throw new InvalidOperationException("Explorer ayarı geri alınamadı.");
        RefreshExplorer();
        _history.Record("Explorer", "Reverted", $"{scope}; snapshot {snapshot.Id}.");
        return $"Explorer ayarı geri alındı: {scope}.";
    }

    private string ApplyDword(string scope, string valueName, int value, string message)
    {
        EnsureWindows();
        var previous = ReadValue(valueName);
        var snapshotId = _snapshots.Create(scope, new ExplorerValue(valueName, previous));
        WriteValue(valueName, value);
        if (ReadValue(valueName) != value)
            throw new InvalidOperationException($"Explorer ayarı doğrulanamadı: {valueName}.");
        RefreshExplorer();
        _history.Record("Explorer", "Applied", $"{scope}; snapshot {snapshotId}; value {value}.");
        return $"{message} Değer: {value}; Explorer yenilendi.";
    }

    private static int? ReadValue(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedPath, false);
        return key?.GetValue(name) is int value ? value : null;
    }

    private static void WriteValue(string name, int? value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(ExplorerAdvancedPath);
        if (value.HasValue) key.SetValue(name, value.Value, RegistryValueKind.DWord);
        else key.DeleteValue(name, false);
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Explorer ayarları yalnızca Windows'ta kullanılabilir.");
    }

    private static void RefreshExplorer()
    {
        SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        SendMessageTimeout(new IntPtr(0xFFFF), 0x001A, IntPtr.Zero, IntPtr.Zero, 0x0002, 1000, out _);
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr window,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        uint flags,
        uint timeout,
        out IntPtr result);

    public sealed record ExplorerValue(string Name, int? Value);
}
