using Microsoft.Win32;
using System.Management;

namespace WinRevive.Services;

public sealed class SecurityDiagnosticsService
{
    public string ReadSummary()
    {
        if (!OperatingSystem.IsWindows()) return "Güvenlik tanılaması Windows dışında kullanılamaz.";
        var defender = ReadDefender();
        var secureBoot = ReadSecureBoot();
        var tpm = ReadTpm();
        var bitLocker = ReadBitLocker();
        return $"Defender: {defender} • Secure Boot: {secureBoot} • TPM: {tpm} • BitLocker: {bitLocker}";
    }

    private static string ReadDefender()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\SecurityCenter2",
                "SELECT displayName, productState FROM AntiVirusProduct");
            var names = searcher.Get().Cast<ManagementObject>()
                .Select(item => item["displayName"]?.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray();
            return names.Length == 0 ? "Bilinmiyor" : string.Join(", ", names);
        }
        catch (ManagementException) { return "Bilinmiyor"; }
        catch (UnauthorizedAccessException) { return "Yetki yok"; }
    }

    private static string ReadSecureBoot()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\SecureBoot\State", false);
            return key?.GetValue("UEFISecureBootEnabled") is int value
                ? value == 1 ? "Açık" : "Kapalı"
                : "Bilinmiyor";
        }
        catch (UnauthorizedAccessException) { return "Yetki yok"; }
    }

    private static string ReadTpm()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\CIMV2\Security\MicrosoftTpm",
                "SELECT IsEnabled_InitialValue FROM Win32_Tpm");
            var item = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            return item?["IsEnabled_InitialValue"] is bool enabled
                ? enabled ? "Etkin" : "Kapalı"
                : "Bilinmiyor";
        }
        catch (ManagementException) { return "Bilinmiyor"; }
        catch (UnauthorizedAccessException) { return "Yetki yok"; }
    }

    private static string ReadBitLocker()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\CIMV2\Security\MicrosoftVolumeEncryption",
                "SELECT ProtectionStatus FROM Win32_EncryptableVolume");
            var count = searcher.Get().Cast<ManagementObject>().Count();
            return count == 0 ? "Bilinmiyor" : $"{count} birim bulundu";
        }
        catch (ManagementException) { return "Bilinmiyor"; }
        catch (UnauthorizedAccessException) { return "Yetki yok"; }
    }
}
