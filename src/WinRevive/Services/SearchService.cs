using Microsoft.Win32;

namespace WinRevive.Services;

public sealed class SearchService
{
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
}
