using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace ScreenControls.Services;

internal enum AppTheme
{
    Dark,
    Light
}

internal sealed class ThemePreferenceStore
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScreenControls",
        "settings.json");

    public AppTheme Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return AppTheme.Dark;
            var settings = JsonSerializer.Deserialize<ThemeSettings>(File.ReadAllText(SettingsPath));
            return Enum.TryParse<AppTheme>(settings?.Theme, ignoreCase: true, out var theme)
                ? theme
                : AppTheme.Dark;
        }
        catch
        {
            return AppTheme.Dark;
        }
    }

    public void Save(AppTheme theme)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(new ThemeSettings { Theme = theme.ToString() });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Keep the selected theme for the current session if the preference cannot be persisted.
        }
    }

    private sealed class ThemeSettings
    {
        public string? Theme { get; init; }
    }
}

internal sealed record ThemePalette(IReadOnlyDictionary<string, Color> Colors)
{
    public static ThemePalette Dark { get; } = new(new Dictionary<string, Color>
    {
        ["WindowBackground"] = Color.FromRgb(16, 21, 31),
        ["Surface"] = Color.FromRgb(27, 35, 48),
        ["PrimaryText"] = Color.FromRgb(243, 246, 252),
        ["SecondaryText"] = Color.FromRgb(162, 174, 191),
        ["Accent"] = Color.FromRgb(138, 164, 255),
        ["TrackRemainder"] = Color.FromRgb(57, 68, 86),
        ["TrackFill"] = Color.FromRgb(138, 164, 255),
        ["ThumbFill"] = Color.FromRgb(211, 220, 255),
        ["ThemeControlBackground"] = Color.FromRgb(35, 45, 59),
        ["ThemeSelectedBackground"] = Color.FromRgb(51, 70, 117),
        ["VolumeIconBackground"] = Color.FromRgb(38, 50, 81),
        ["VolumeIconForeground"] = Color.FromRgb(175, 192, 255),
        ["BrightnessIconBackground"] = Color.FromRgb(58, 48, 33),
        ["BrightnessIconForeground"] = Color.FromRgb(245, 184, 94)
    });

    public static ThemePalette Light { get; } = new(new Dictionary<string, Color>
    {
        ["WindowBackground"] = Color.FromRgb(243, 245, 249),
        ["Surface"] = Color.FromRgb(255, 255, 255),
        ["PrimaryText"] = Color.FromRgb(24, 34, 48),
        ["SecondaryText"] = Color.FromRgb(102, 117, 139),
        ["Accent"] = Color.FromRgb(72, 104, 232),
        ["TrackRemainder"] = Color.FromRgb(213, 220, 231),
        ["TrackFill"] = Color.FromRgb(72, 104, 232),
        ["ThumbFill"] = Color.FromRgb(255, 255, 255),
        ["ThemeControlBackground"] = Color.FromRgb(229, 233, 241),
        ["ThemeSelectedBackground"] = Color.FromRgb(255, 255, 255),
        ["VolumeIconBackground"] = Color.FromRgb(238, 241, 255),
        ["VolumeIconForeground"] = Color.FromRgb(72, 104, 232),
        ["BrightnessIconBackground"] = Color.FromRgb(255, 244, 232),
        ["BrightnessIconForeground"] = Color.FromRgb(216, 131, 34)
    });
}
