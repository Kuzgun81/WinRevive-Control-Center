using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WinRevive.Models;
using WinRevive.Services;

namespace WinRevive.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SystemInfoService _systemInfoService;
    private readonly SettingsService _settings;
    private readonly FileLogger _logger;
    private readonly TransparencyOptimizationRule _rule;
    private readonly OperationHistoryService _history;
    private string _theme;
    private string _transparencyState = "Durum okunuyor...";
    private string _lastOperation = "Henüz işlem yapılmadı.";

    public MainViewModel(SystemInfoService systemInfoService, SettingsService settings, FileLogger logger, TransparencyOptimizationRule rule)
    {
        _systemInfoService = systemInfoService;
        _settings = settings;
        _logger = logger;
        _rule = rule;
        _history = new OperationHistoryService();
        _theme = settings.Theme;
        Themes = new(new[] { new ThemeOption("Koyu", "dark"), new ThemeOption("Açık (yakında)", "light"), new ThemeOption("Sistem (yakında)", "system") });
        RefreshCommand = new ActionCommand(Refresh);
        ApplyTransparencyCommand = new ActionCommand(() => Run(() => _rule.Apply()));
        RevertTransparencyCommand = new ActionCommand(() => Run(() => _rule.Revert()));
        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ThemeOption> Themes { get; }
    public SystemInfo SystemInfo { get; private set; } = new("Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...");
    public string StatusMessage => "Hazır";
    public string TransparencyState { get => _transparencyState; private set => Set(ref _transparencyState, value); }
    public string LastOperation { get => _lastOperation; private set => Set(ref _lastOperation, value); }
    public string Theme { get => _theme; set { if (Set(ref _theme, value)) _settings.Theme = value; } }
    public ICommand RefreshCommand { get; }
    public ICommand ApplyTransparencyCommand { get; }
    public ICommand RevertTransparencyCommand { get; }

    private void Refresh()
    {
        try
        {
            SystemInfo = _systemInfoService.Read();
            TransparencyState = _rule.Detect();
            LastOperation = _history.ReadLatest() is { } latest
                ? $"{latest.Operation}: {latest.Status} ({latest.Detail})"
                : "Sistem bilgileri yenilendi.";
            OnPropertyChanged(nameof(SystemInfo));
        }
        catch (Exception ex)
        {
            _logger.Error("Refresh failed.", ex);
            LastOperation = "Tarama başarısız: " + ex.Message;
        }
    }

    private void Run(Func<string> operation)
    {
        try { LastOperation = operation(); TransparencyState = _rule.Detect(); }
        catch (Exception ex) { _logger.Error("Optimization operation failed.", ex); LastOperation = "İşlem başarısız: " + ex.Message; }
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new(name));
}

public sealed class ActionCommand(Action action) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => action();
}
