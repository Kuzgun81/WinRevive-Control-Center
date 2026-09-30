using System.Windows;
using System.ComponentModel;
using WinRevive.Services;
using WinRevive.ViewModels;

namespace WinRevive;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var capabilities = new CapabilityService();
        var snapshots = new SnapshotService();
        var viewModel = new MainViewModel(
            new SystemInfoService(capabilities, new StartupService(), new SearchService(), new WindowsDiagnosticsService()),
            new SettingsService(),
            new FileLogger(),
            new TransparencyOptimizationRule(
                new RegistryService(),
                new FileLogger(),
                new OperationHistoryService(),
                snapshots),
            new MinimalProfileService(
                new RegistryService(),
                new FileLogger(),
                new OperationHistoryService()));
        DataContext = viewModel;
        ThemeService.Apply(viewModel.Theme);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private static void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is MainViewModel viewModel && args.PropertyName == nameof(MainViewModel.Theme))
            ThemeService.Apply(viewModel.Theme);
    }
}
