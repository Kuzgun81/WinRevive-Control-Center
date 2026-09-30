using System.Windows;
using System.Windows.Media;

namespace WinRevive.Services;

public static class ThemeService
{
    public static void Apply(string theme)
    {
        var resources = Application.Current.Resources;
        var dark = !string.Equals(theme, "light", StringComparison.OrdinalIgnoreCase);
        resources["WindowBackground"] = Brush(dark ? "#0F172A" : "#F8FAFC");
        resources["CardBackground"] = Brush(dark ? "#1E293B" : "#E2E8F0");
        resources["SurfaceBackground"] = Brush(dark ? "#162238" : "#FFFFFF");
        resources["BorderBrush"] = Brush(dark ? "#2B3B55" : "#CBD5E1");
        resources["PrimaryText"] = Brush(dark ? "#F8FAFC" : "#0F172A");
        resources["MutedText"] = Brush(dark ? "#94A3B8" : "#475569");
        resources["AccentBrush"] = Brush(dark ? "#38BDF8" : "#0284C7");
        resources["SecondaryButtonBackground"] = Brush(dark ? "#26364F" : "#E2E8F0");
        resources["SecondaryButtonForeground"] = Brush(dark ? "#E2E8F0" : "#1E293B");
    }

    private static SolidColorBrush Brush(string color) => new((Color)ColorConverter.ConvertFromString(color));
}
