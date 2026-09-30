using Microsoft.Win32;
using System.ServiceProcess;

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

    public string Repair()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Windows Search onarımı yalnızca Windows'ta kullanılabilir.");
        using var service = new ServiceController("WSearch");
        if (service.Status != ServiceControllerStatus.Stopped &&
            service.Status != ServiceControllerStatus.StopPending)
            service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
        return "Windows Search servisi yeniden başlatıldı ve doğrulandı.";
    }
}
