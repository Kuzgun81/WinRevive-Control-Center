using Microsoft.Win32;
using WinRevive.Contracts;

namespace WinRevive.Services;

public sealed class SearchService
{
    private readonly ElevatedHostClient _elevatedHost;

    public SearchService(ElevatedHostClient? elevatedHost = null)
    {
        _elevatedHost = elevatedHost ?? new ElevatedHostClient();
    }

    public string ReadStatus()
    {
        if (!OperatingSystem.IsWindows())
            return "Windows Search: Windows dışı ortam";

        using var key = Registry.LocalMachine.OpenSubKey(
            @"SYSTEM\CurrentControlSet\Services\WSearch", false);
        if (key is null)
            return "Windows Search: bulunamadı";

        var start = Convert.ToInt32(key.GetValue("Start", -1));
        return start switch
        {
            4 => "Windows Search: devre dışı",
            3 => "Windows Search: elle başlatma",
            _ => "Windows Search: yapılandırılmış"
        };
    }

    public string Repair()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows Search onarımı yalnızca Windows'ta kullanılabilir.");
        return _elevatedHost.Execute(ElevatedOperation.RepairWindowsSearch);
    }

    public string Rebuild()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows Search yeniden oluşturma yalnızca Windows'ta kullanılabilir.");
        return _elevatedHost.Execute(ElevatedOperation.RebuildWindowsSearch);
    }

    public string ReadIndexStatus()
    {
        if (!OperatingSystem.IsWindows())
            return "Dizin durumu: Windows dışı ortam";

        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows Search\Gather\Windows\SystemIndex", false);
        if (key is null)
            return "Dizin durumu: yapılandırma bulunamadı";

        var completed = key.GetValue("SetupCompletedSuccessfully");
        return completed is 1 or true
            ? "Dizin durumu: hazır"
            : "Dizin durumu: hazırlanıyor veya yeniden oluşturuluyor";
    }
}
