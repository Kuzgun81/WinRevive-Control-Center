namespace WinRevive.Services;

public sealed class OptimizationProfileService
{
    private readonly MinimalProfileService _minimal;
    private readonly PowerProfileService _power;
    private readonly OperationHistoryService _history;

    public OptimizationProfileService(MinimalProfileService minimal, PowerProfileService power, OperationHistoryService history)
    {
        _minimal = minimal;
        _power = power;
        _history = history;
    }

    public string Apply(string profile) => profile switch
    {
        "low-hardware" => _minimal.Apply(),
        "gaming" => ApplyPower("performance"),
        "battery" => ApplyPower("battery"),
        "general" => ApplyPower("balanced"),
        _ => throw new ArgumentException("Desteklenmeyen optimizasyon profili.", nameof(profile))
    };

    private string ApplyPower(string powerProfile)
    {
        var result = _power.Apply(powerProfile);
        _history.Record("Optimization profile", "Applied", $"{powerProfile}: {result}");
        return result;
    }
}
