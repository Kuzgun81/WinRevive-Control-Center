using System.Windows;
using WinRevive.Services;

namespace WinRevive;

public partial class App : Application
{
    private readonly FileLogger _logger = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            _logger.Error("Unhandled UI exception.", args.Exception);
            MessageBox.Show("Beklenmeyen bir hata oluştu. Ayrıntılar günlük dosyasına yazıldı.",
                "WinRevive", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            _logger.Error("Unhandled application exception.", args.ExceptionObject as Exception);

        _logger.Info("Application started.");
        base.OnStartup(e);

        try
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            _logger.Error("Application startup failed.", exception);
            MessageBox.Show(
                $"WinRevive açılamadı.\n\n{exception.Message}\n\nAyrıntılar şu dosyaya yazıldı:\n%LocalAppData%\\WinRevive\\logs\\app.log",
                "WinRevive",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
