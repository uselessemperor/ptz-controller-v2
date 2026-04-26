using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PTZ_Controller_v2.Models;
using PTZ_Controller_v2.Services.CameraControl;
using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace PTZ_Controller_v2.Controls;

public sealed partial class CameraControlView : UserControl
{
    private readonly Camera _camera;
    private readonly AppServices _services;
    private readonly ICameraControlProtocol _cameraControl;

    private CameraSettings _settings = default!;
    private MediaPlayer? _mediaPlayer;
    private bool _isLoaded;

    private byte PanSpeed => (byte)Math.Round(PanSpeedSlider.Value);
    private byte TiltSpeed => (byte)Math.Round(TiltSpeedSlider.Value);

    public CameraControlView(Camera camera, AppServices services)
    {
        InitializeComponent();

        _camera = camera;
        _services = services;
        _cameraControl = _services.CameraControlProtocolFactory.Create(camera);

        CameraTitleTextBlock.Text = $"{camera.CameraName} • {camera.IP} • {camera.Protocol}";

        VideoStreamTextBlock.Text = string.IsNullOrWhiteSpace(camera.PreviewStreamUrl)
            ? "No preview stream URL configured."
            : camera.PreviewStreamUrl;

        Loaded += CameraControlView_Loaded;
        Unloaded += CameraControlView_Unloaded;
    }

    private async void CameraControlView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
            return;

        _isLoaded = true;

        await LoadCameraSettingsAsync();
        await InitializePreviewAsync();
    }

    private async void CameraControlView_Unloaded(object sender, RoutedEventArgs e)
    {
        await DisposeAsync();
    }

    public async Task ActivateAsync()
    {
        await InitializePreviewAsync();
    }

    public async Task DeactivateAsync()
    {
        await StopPreviewAsync();
        await _cameraControl.StopPanTiltAsync();
    }

    public async Task DisposeAsync()
    {
        await StopPreviewAsync();
        await _cameraControl.DisposeAsync();
    }

    private async Task LoadCameraSettingsAsync()
    {
        try
        {
            _settings = await _services.CameraRepository.GetSettingsForCameraAsync(_camera.Id);

            PanSpeedSlider.Value = _settings.PanSpeed;
            TiltSpeedSlider.Value = _settings.TiltSpeed;
            GainSlider.Value = _settings.Gain;
            GainValueTextBlock.Text = $"Gain: {_settings.Gain}";

            SetExposureComboBox(_settings.ExposureMode);
            SetWhiteBalanceComboBox(_settings.WhiteBalanceMode);

            AutoWhiteBalanceCheckBox.IsChecked = _settings.AutoWhiteBalanceEnabled;

            SetStatus("Camera settings loaded.");
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load settings: {ex.Message}");
        }
    }

    private async Task InitializePreviewAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_camera.PreviewStreamUrl))
            {
                SetStatus("No preview stream URL configured for this camera.");
                return;
            }

            await StopPreviewAsync();

            _mediaPlayer = new MediaPlayer();
            PreviewPlayer.SetMediaPlayer(_mediaPlayer);

            _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(_camera.PreviewStreamUrl));
            _mediaPlayer.Play();

            SetStatus("Preview started.");
        }
        catch (Exception ex)
        {
            SetStatus($"Preview failed: {ex.Message}");
        }
    }

    private Task StopPreviewAsync()
    {
        _mediaPlayer?.Pause();
        _mediaPlayer?.Dispose();
        _mediaPlayer = null;
        PreviewPlayer.SetMediaPlayer(null);

        return Task.CompletedTask;
    }

    private async void ReloadPreview_Click(object sender, RoutedEventArgs e)
    {
        await InitializePreviewAsync();
    }

    private async void UpLeft_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x01, 0x01),
            "Moving up-left.");
    }

    private async void TiltUp_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x03, 0x01),
            "Tilting up.");
    }

    private async void UpRight_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x02, 0x01),
            "Moving up-right.");
    }

    private async void PanLeft_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x01, 0x03),
            "Panning left.");
    }

    private async void Stop_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.StopPanTiltAsync(),
            "Movement stopped.");
    }

    private async void PanRight_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x02, 0x03),
            "Panning right.");
    }

    private async void DownLeft_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x01, 0x02),
            "Moving down-left.");
    }

    private async void TiltDown_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x03, 0x02),
            "Tilting down.");
    }

    private async void DownRight_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x02, 0x02),
            "Moving down-right.");
    }

    private async void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.ZoomInAsync(),
            "Zooming in.");
    }

    private async void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.ZoomOutAsync(),
            "Zooming out.");
    }

    private async void ZoomStop_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.StopZoomAsync(),
            "Zoom stopped.");
    }

    private async void ExposureModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_settings == null)
            return;

        var modeText = GetSelectedComboBoxText(ExposureModeComboBox);

        if (!Enum.TryParse<ExposureMode>(modeText, out var exposureMode))
            return;

        _settings.ExposureMode = exposureMode;
        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);

        await ExecuteCameraCommandAsync(
            () => _cameraControl.SetExposureModeAsync(exposureMode),
            $"Exposure mode set to {exposureMode}.");
    }

    private async void WhiteBalanceModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_settings == null)
            return;

        var modeText = GetSelectedComboBoxText(WhiteBalanceModeComboBox);

        if (!Enum.TryParse<WhiteBalanceMode>(modeText, out var whiteBalanceMode))
            return;

        _settings.WhiteBalanceMode = whiteBalanceMode;
        _settings.AutoWhiteBalanceEnabled = whiteBalanceMode == WhiteBalanceMode.Auto;
        AutoWhiteBalanceCheckBox.IsChecked = _settings.AutoWhiteBalanceEnabled;

        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);

        await ExecuteCameraCommandAsync(
            () => _cameraControl.SetWhiteBalanceModeAsync(whiteBalanceMode),
            $"White balance mode set to {whiteBalanceMode}.");
    }

    private async void AutoWhiteBalanceCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_settings == null)
            return;

        _settings.AutoWhiteBalanceEnabled = true;
        _settings.WhiteBalanceMode = WhiteBalanceMode.Auto;

        SetWhiteBalanceComboBox(WhiteBalanceMode.Auto);

        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);

        await ExecuteCameraCommandAsync(
            () => _cameraControl.SetWhiteBalanceModeAsync(WhiteBalanceMode.Auto),
            "Auto white balance enabled.");
    }

    private async void AutoWhiteBalanceCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_settings == null)
            return;

        _settings.AutoWhiteBalanceEnabled = false;
        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);

        SetStatus("Auto white balance disabled.");
    }

    private async void GainSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_settings == null || GainValueTextBlock == null)
            return;

        var gain = (int)Math.Round(e.NewValue);

        GainValueTextBlock.Text = $"Gain: {gain}";

        _settings.Gain = gain;
        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);

        await ExecuteCameraCommandAsync(
            () => _cameraControl.SetGainAsync((byte)gain),
            $"Gain set to {gain}.");
    }

    private async void PanSpeedSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_settings == null)
            return;

        _settings.PanSpeed = (int)Math.Round(e.NewValue);
        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);
    }

    private async void TiltSpeedSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_settings == null)
            return;

        _settings.TiltSpeed = (int)Math.Round(e.NewValue);
        await _services.CameraRepository.SaveCameraSettingsAsync(_settings);
    }

    private async Task ExecuteCameraCommandAsync(Func<Task> command, string successMessage)
    {
        try
        {
            await command();
            SetStatus(successMessage);
        }
        catch (Exception ex)
        {
            SetStatus($"Command failed: {ex.Message}");
        }
    }

    private void SetExposureComboBox(ExposureMode mode)
    {
        SetComboBoxByText(ExposureModeComboBox, mode.ToString());
    }

    private void SetWhiteBalanceComboBox(WhiteBalanceMode mode)
    {
        SetComboBoxByText(WhiteBalanceModeComboBox, mode.ToString());
    }

    private static void SetComboBoxByText(ComboBox comboBox, string value)
    {
        foreach (var item in comboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem &&
                comboBoxItem.Content?.ToString() == value)
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }
    }

    private static string GetSelectedComboBoxText(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            return selectedItem.Content?.ToString() ?? string.Empty;

        return string.Empty;
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }
}