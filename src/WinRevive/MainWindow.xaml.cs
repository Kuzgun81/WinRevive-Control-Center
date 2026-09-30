using System.Windows;
using System.ComponentModel;
using WinRevive.Services;
using WinRevive.ViewModels;

namespace WinRevive;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var settings = new SettingsService();
        if (!settings.FirstRunCompleted)
            new FirstRunWindow(settings).ShowDialog();
        InitializeComponent();
        var capabilities = new CapabilityService();
        var snapshots = new SnapshotService();
        var startup = new StartupService();
        var search = new SearchService();
        var minimal = new MinimalProfileService(
            new RegistryService(),
            new FileLogger(),
            new OperationHistoryService());
        var viewModel = new MainViewModel(
            new SystemInfoService(
                capabilities,
                startup,
                search,
                new WindowsDiagnosticsService(),
                new SecurityDiagnosticsService(),
                new ApplicationInventoryService()),
            settings,
            new FileLogger(),
            new TransparencyOptimizationRule(
                new RegistryService(),
                new FileLogger(),
                new OperationHistoryService(),
                snapshots),
            minimal,
            startup,
            new StartupManagementService(startup, snapshots, new OperationHistoryService()),
            search,
            new OptimizationProfileService(minimal, new PowerProfileService(), new OperationHistoryService()));
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
