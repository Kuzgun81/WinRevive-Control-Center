using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using WinRevive.Contracts;

namespace WinRevive.Services;

public sealed class ElevatedHostClient
{
    private const int ProtocolVersion = 1;

    public string Execute(ElevatedOperation operation)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Yetkili Windows işlemleri yalnızca Windows'ta kullanılabilir.");

        var pipeName = $"WinRevive-Elevated-{Guid.NewGuid():N}";
        var nonce = Guid.NewGuid().ToString("N");
        using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        var helperPath = Path.Combine(AppContext.BaseDirectory, "WinRevive.ElevatedHost.exe");
        if (!File.Exists(helperPath))
            throw new FileNotFoundException("Elevated host bulunamadı.", helperPath);

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = helperPath,
            Arguments = $"{ProtocolVersion} {pipeName} {nonce}",
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = AppContext.BaseDirectory
        }) ?? throw new InvalidOperationException("Elevated host başlatılamadı.");

        server.WaitForConnection();
        using var reader = new StreamReader(server);
        using var writer = new StreamWriter(server) { AutoFlush = true };
        writer.WriteLine(JsonSerializer.Serialize(new ElevatedRequest(ProtocolVersion, nonce, operation)));
        var response = JsonSerializer.Deserialize<ElevatedResponse>(reader.ReadLine() ?? string.Empty)
            ?? throw new InvalidOperationException("Elevated host geçersiz yanıt verdi.");
        process.WaitForExit(30_000);
        if (!response.Success) throw new InvalidOperationException(response.Message);
        return response.Message;
    }
}
