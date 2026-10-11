using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.CoreAudioApi;
using ScreenControls.Services;

namespace ScreenControls;

public partial class MainWindow : Window
{
    private readonly BrightnessController _brightness = new();
    private readonly DispatcherTimer _brightnessTimer;
    private bool _isInitializing = true;
    private bool _suppressThemeEvents;
    private readonly ThemePreferenceStore _themePreferences = new();

    public MainWindow()
    {
        InitializeComponent();
        var theme = _themePreferences.Load();
        _suppressThemeEvents = true;
        DarkThemeOption.IsChecked = theme == AppTheme.Dark;
        LightThemeOption.IsChecked = theme == AppTheme.Light;
        ApplyTheme(theme);
        _suppressThemeEvents = false;

        _brightnessTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        _brightnessTimer.Tick += async (_, _) =>
        {
            _brightnessTimer.Stop();
            await SetBrightnessAsync((byte)Math.Round(BrightnessSlider.Value));
        };
        Loaded += MainWindow_OnLoaded;
    }

    private void ThemeOption_OnChecked(object sender, RoutedEventArgs e)
    {
        if (_suppressThemeEvents) return;
        var theme = LightThemeOption.IsChecked == true ? AppTheme.Light : AppTheme.Dark;
        ApplyTheme(theme);
        _themePreferences.Save(theme);
    }

    private void ApplyTheme(AppTheme theme)
    {
        var palette = theme == AppTheme.Dark ? ThemePalette.Dark : ThemePalette.Light;
        foreach (var (key, color) in palette.Colors)
        {
            if (Resources[key] is System.Windows.Media.SolidColorBrush brush)
                brush.Color = color;
            else
                Resources[key] = new System.Windows.Media.SolidColorBrush(color);
        }
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            VolumeSlider.Value = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
            VolumeValue.Text = $"{Math.Round(VolumeSlider.Value)}%";
            VolumeStatus.Text = device.FriendlyName;
        }
        catch (Exception ex)
        {
            VolumeSlider.IsEnabled = false;
            VolumeStatus.Text = "Não foi possível acessar a saída de áudio.";
            VolumeStatus.ToolTip = ex.Message;
        }

        try
        {
            var level = await Task.Run(_brightness.GetBrightness);
            BrightnessSlider.Value = level;
            BrightnessValue.Text = $"{level}%";
        }
        catch (Exception ex)
        {
            BrightnessSlider.IsEnabled = false;
            BrightnessStatus.Text = "Brilho não disponível neste dispositivo.";
            BrightnessStatus.ToolTip = ex.Message;
        }

        _isInitializing = false;
    }

    private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (VolumeValue is null) return;
        var value = (float)(Math.Round(e.NewValue) / 100d);
        VolumeValue.Text = $"{Math.Round(e.NewValue)}%";
        if (_isInitializing) return;

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = value;
            VolumeStatus.Text = device.FriendlyName;
        }
        catch (Exception ex)
        {
            VolumeStatus.Text = "Não foi possível alterar o volume.";
            VolumeStatus.ToolTip = ex.Message;
        }
    }

    private void BrightnessSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (BrightnessValue is null) return;
        BrightnessValue.Text = $"{Math.Round(e.NewValue)}%";
        if (_isInitializing) return;
        _brightnessTimer.Stop();
        _brightnessTimer.Start();
    }

    private async Task SetBrightnessAsync(byte level)
    {
        try
        {
            await Task.Run(() => _brightness.SetBrightness(level));
            BrightnessStatus.Text = "Tela integrada";
            BrightnessStatus.ToolTip = null;
        }
        catch (Exception ex)
        {
            BrightnessStatus.Text = "Não foi possível alterar o brilho.";
            BrightnessStatus.ToolTip = ex.Message;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _brightnessTimer.Stop();
        base.OnClosing(e);
    }
}
