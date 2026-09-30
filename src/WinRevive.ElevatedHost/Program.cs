using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text.Json;
using WinRevive.Contracts;

if (args.Length != 3 || !int.TryParse(args[0], out var protocolVersion))
    return 2;

var pipeName = args[1];
var expectedNonce = args[2];
using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
try
{
    await pipe.ConnectAsync(20_000);
    using var reader = new StreamReader(pipe);
    using var writer = new StreamWriter(pipe) { AutoFlush = true };
    var request = JsonSerializer.Deserialize<ElevatedRequest>(await reader.ReadLineAsync() ?? string.Empty);
    if (request is null ||
        request.ProtocolVersion != protocolVersion ||
        request.ProtocolVersion != 1 ||
        !string.Equals(request.Nonce, expectedNonce, StringComparison.Ordinal))
        return await Respond(writer, false, "Geçersiz elevated host protokolü.");

    var response = request.Operation switch
    {
        ElevatedOperation.RepairWindowsSearch => RepairSearch(),
        ElevatedOperation.RebuildWindowsSearch => RebuildSearch(),
        _ => new ElevatedResponse(false, "İzin verilmeyen elevated işlem.")
    };
    return await Respond(writer, response.Success, response.Message);
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.TimeoutException)
{
    return 3;
}

static ElevatedResponse RepairSearch()
{
    if (!OperatingSystem.IsWindows())
        return new(false, "Windows Search yalnızca Windows'ta onarılabilir.");

    try
    {
        using var service = new ServiceController("WSearch");
        if (service.Status != ServiceControllerStatus.Stopped &&
            service.Status != ServiceControllerStatus.StopPending)
            service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
        return new(true, "Windows Search servisi elevated host üzerinden yeniden başlatıldı ve doğrulandı.");
    }
    catch (Exception ex) when (ex is InvalidOperationException or System.TimeoutException)
    {
        return new(false, $"Windows Search onarılamadı: {ex.Message}");
    }
}

static ElevatedResponse RebuildSearch()
    {
        if (!OperatingSystem.IsWindows())
            return new(false, "Windows Search yalnızca Windows'ta yeniden oluşturulabilir.");

        try
        {
            var managerType = Type.GetTypeFromProgID("Search.Manager");
            if (managerType is null)
                return new(false, "Windows Search yönetim bileşeni bulunamadı.");

            dynamic manager = Activator.CreateInstance(managerType)
                ?? throw new InvalidOperationException("Windows Search yönetim bileşeni başlatılamadı.");
            dynamic catalog = manager.GetCatalog("SystemIndex");
            catalog.Reindex();
            return new(true, "Windows Search dizini yeniden oluşturma isteği başlatıldı.");
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            return new(false, $"Windows Search dizini yeniden oluşturulamadı: {ex.Message}");
        }
}

static async Task<int> Respond(StreamWriter writer, bool success, string message)
{
    await writer.WriteLineAsync(JsonSerializer.Serialize(new ElevatedResponse(success, message)));
    return success ? 0 : 1;
}
