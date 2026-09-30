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
}
