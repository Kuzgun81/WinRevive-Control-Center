using System.Windows;
using System.Collections.ObjectModel;
using WinRevive.Models;
using WinRevive.Services;

namespace WinRevive;

public partial class FirstRunWindow : Window
{
    private readonly SettingsService _settings;

    public FirstRunWindow(SettingsService settings)
    {
        _settings = settings;
        InitializeComponent();
        Themes = new(new[] { new ThemeOption("Koyu", "dark"), new ThemeOption("Açık", "light") });
        Profiles = new(new[]
        {
            new ThemeOption("Genel / dengeli", "general"),
            new ThemeOption("Düşük donanım", "low-hardware"),
            new ThemeOption("Oyun", "gaming"),
            new ThemeOption("Pil", "battery")
        });
        Theme = settings.Theme;
        PreferredProfile = string.IsNullOrWhiteSpace(settings.PreferredProfile) ? "general" : settings.PreferredProfile;
        DataContext = this;
        ThemeService.Apply(Theme);
    }

    public ObservableCollection<ThemeOption> Themes { get; }
    public ObservableCollection<ThemeOption> Profiles { get; }
    public string Theme { get; set; }
    public string PreferredProfile { get; set; }

    private void ContinueClick(object sender, RoutedEventArgs e)
    {
        _settings.Theme = Theme;
        _settings.PreferredProfile = PreferredProfile;
        _settings.FirstRunCompleted = true;
        DialogResult = true;
        Close();
    }
}
