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
    private readonly MinimalProfileService _minimalProfile;
    private readonly StartupService _startup;
    private readonly StartupManagementService _startupManagement;
    private readonly SearchService _search;
    private readonly OptimizationProfileService _profiles;
    private readonly PersonalizationService _personalization;
    private readonly ApplicationInventoryService _applications;
    private readonly OperationHistoryService _history;
    private readonly SnapshotService _snapshots;
    private string _theme;
    private string _transparencyState = "Durum okunuyor...";
    private string _lastOperation = "Henüz işlem yapılmadı.";
    private string _wallpaperPath = string.Empty;

    public MainViewModel(SystemInfoService systemInfoService, SettingsService settings, FileLogger logger, TransparencyOptimizationRule rule, MinimalProfileService minimalProfile, StartupService startup, StartupManagementService startupManagement, SearchService search, OptimizationProfileService profiles, PersonalizationService personalization, ApplicationInventoryService applications, SnapshotService snapshots)
    {
        _systemInfoService = systemInfoService;
        _settings = settings;
        _logger = logger;
        _rule = rule;
        _minimalProfile = minimalProfile;
        _startup = startup;
        _startupManagement = startupManagement;
        _search = search;
        _profiles = profiles;
        _personalization = personalization;
        _applications = applications;
        _snapshots = snapshots;
        _history = new OperationHistoryService();
        _theme = settings.Theme;
        Themes = new(new[] { new ThemeOption("Koyu", "dark"), new ThemeOption("Açık", "light") });
        RefreshCommand = new ActionCommand(Refresh);
        ApplyTransparencyCommand = new ActionCommand(() => Run(() => _rule.Apply()));
        RevertTransparencyCommand = new ActionCommand(() => Run(() => _rule.Revert()));
        ApplyMinimalProfileCommand = new ActionCommand(() => Run(() => _minimalProfile.Apply()));
        RevertMinimalProfileCommand = new ActionCommand(() => Run(() => _minimalProfile.Revert()));
        RefreshStartupCommand = new ActionCommand(RefreshStartup);
        DisableSelectedStartupCommand = new ActionCommand(DisableSelectedStartup);
        RevertStartupCommand = new ActionCommand(() => RunStartup(_startupManagement.RevertLatest));
        RepairSearchCommand = new ActionCommand(() => Run(() => _search.Repair()));
        ApplyGeneralProfileCommand = new ActionCommand(() => Run(() => _profiles.Apply("general")));
        ApplyGamingProfileCommand = new ActionCommand(() => Run(() => _profiles.Apply("gaming")));
        ApplyBatteryProfileCommand = new ActionCommand(() => Run(() => _profiles.Apply("battery")));
        ApplyLowHardwareProfileCommand = new ActionCommand(() => Run(() => _profiles.Apply("low-hardware")));
        ApplyBlueAccentCommand = new ActionCommand(() => Run(() => _personalization.ApplyAccent("00A4EF")));
        ApplyPurpleAccentCommand = new ActionCommand(() => Run(() => _personalization.ApplyAccent("8764B8")));
        RevertAccentCommand = new ActionCommand(() => Run(() => _personalization.RevertAccent()));
        ApplyWallpaperCommand = new ActionCommand(() => Run(() => _personalization.ApplyWallpaper(WallpaperPath)));
        RevertWallpaperCommand = new ActionCommand(() => Run(() => _personalization.RevertWallpaper()));
        RefreshApplicationsCommand = new ActionCommand(RefreshApplications);
        RefreshSnapshotsCommand = new ActionCommand(RefreshSnapshots);
        DeleteSelectedSnapshotCommand = new ActionCommand(DeleteSelectedSnapshot);
        Refresh();
        RefreshStartup();
        RefreshApplications();
        RefreshSnapshots();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ThemeOption> Themes { get; }
    public ObservableCollection<string> RecentOperations { get; } = [];
    public ObservableCollection<StartupEntry> StartupEntries { get; } = [];
    public ObservableCollection<InstalledApplication> InstalledApplications { get; } = [];
    public ObservableCollection<SnapshotService.SnapshotEntry> Snapshots { get; } = [];
    public SnapshotService.SnapshotEntry? SelectedSnapshot { get; set; }
    public StartupEntry? SelectedStartup { get; set; }
    public SystemInfo SystemInfo { get; private set; } = new(
        "Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...",
        "Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...", "Okunuyor...",
        "Okunuyor...", "Okunuyor...");
    public string StatusMessage => "Hazır";
    public string TransparencyState { get => _transparencyState; private set => Set(ref _transparencyState, value); }
    public string LastOperation { get => _lastOperation; private set => Set(ref _lastOperation, value); }
    public string WallpaperPath { get => _wallpaperPath; set => Set(ref _wallpaperPath, value); }
    public string Theme { get => _theme; set { if (Set(ref _theme, value)) _settings.Theme = value; } }
    public string MinimalProfileState => _minimalProfile.Detect();
    public ICommand RefreshCommand { get; }
    public ICommand ApplyTransparencyCommand { get; }
    public ICommand RevertTransparencyCommand { get; }
    public ICommand ApplyMinimalProfileCommand { get; }
    public ICommand RevertMinimalProfileCommand { get; }
    public ICommand RefreshStartupCommand { get; }
    public ICommand DisableSelectedStartupCommand { get; }
    public ICommand RevertStartupCommand { get; }
    public ICommand RepairSearchCommand { get; }
    public ICommand ApplyGeneralProfileCommand { get; }
    public ICommand ApplyGamingProfileCommand { get; }
    public ICommand ApplyBatteryProfileCommand { get; }
    public ICommand ApplyLowHardwareProfileCommand { get; }
    public ICommand ApplyBlueAccentCommand { get; }
    public ICommand ApplyPurpleAccentCommand { get; }
    public ICommand RevertAccentCommand { get; }
    public ICommand ApplyWallpaperCommand { get; }
    public ICommand RevertWallpaperCommand { get; }
    public ICommand RefreshApplicationsCommand { get; }
    public ICommand RefreshSnapshotsCommand { get; }
    public ICommand DeleteSelectedSnapshotCommand { get; }

    private void Refresh()
    {
        try
        {
            SystemInfo = _systemInfoService.Read();
            TransparencyState = _rule.Detect();
            OnPropertyChanged(nameof(MinimalProfileState));
            LastOperation = _history.ReadLatest() is { } latest
                ? $"{latest.Operation}: {latest.Status} ({latest.Detail})"
                : "Sistem bilgileri yenilendi.";
            LoadRecentOperations();
            RefreshSnapshots();
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
        try
        {
            LastOperation = operation();
            TransparencyState = _rule.Detect();
            OnPropertyChanged(nameof(MinimalProfileState));
            LoadRecentOperations();
            RefreshSnapshots();
        }
        catch (Exception ex) { _logger.Error("Optimization operation failed.", ex); LastOperation = "İşlem başarısız: " + ex.Message; }
    }

    private void RefreshStartup()
    {
        StartupEntries.Clear();
        foreach (var entry in _startup.ReadEntries()) StartupEntries.Add(entry);
    }

    private void RefreshApplications()
    {
        InstalledApplications.Clear();
        foreach (var application in _applications.Read())
            InstalledApplications.Add(application);
    }

    private void RefreshSnapshots()
    {
        Snapshots.Clear();
        foreach (var snapshot in _snapshots.List())
            Snapshots.Add(snapshot);
    }

    private void DeleteSelectedSnapshot()
    {
        if (SelectedSnapshot is null)
        {
            LastOperation = "Önce silinecek bir snapshot seçin.";
            return;
        }

        try
        {
            if (!_snapshots.Delete(SelectedSnapshot.Id))
                throw new InvalidOperationException("Snapshot bulunamadı veya silinemedi.");
            _history.Record("Snapshots", "Deleted", SelectedSnapshot.Id);
            LastOperation = $"Snapshot silindi: {SelectedSnapshot.Id}";
            RefreshSnapshots();
        }
        catch (Exception ex)
        {
            _logger.Error("Snapshot deletion failed.", ex);
            LastOperation = "Snapshot silinemedi: " + ex.Message;
        }
    }

    private void DisableSelectedStartup()
    {
        if (SelectedStartup is null)
        {
            LastOperation = "Önce kullanıcı kapsamındaki bir başlangıç girdisi seçin.";
            return;
        }
        Run(() => _startupManagement.Disable(SelectedStartup));
        RefreshStartup();
    }

    private void RunStartup(Func<string> operation)
    {
        Run(operation);
        RefreshStartup();
    }

    private void LoadRecentOperations()
    {
        RecentOperations.Clear();
        foreach (var item in _history.ReadRecent())
            RecentOperations.Add($"{item.Timestamp.LocalDateTime:g} • {item.Operation} • {item.Status} • {item.Detail}");
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
