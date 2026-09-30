using System.Windows;
using WinRevive.Services;
using WinRevive.ViewModels;

namespace WinRevive;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(
            new SystemInfoService(),
            new SettingsService(),
            new FileLogger(),
            new TransparencyOptimizationRule(
                new RegistryService(),
                new FileLogger(),
                new OperationHistoryService()));
    }
}
