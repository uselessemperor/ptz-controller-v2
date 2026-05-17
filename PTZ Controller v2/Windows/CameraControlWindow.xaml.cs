using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PTZ_Controller_v2.Helpers;
using PTZ_Controller_v2.Models;
using PTZ_Controller_v2.Services.CameraControl;
using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace PTZ_Controller_v2;

public sealed partial class CameraControlWindow : Window
{
    private readonly Camera _camera;
    private readonly AppServices _services;
    private readonly ICameraControlProtocol _cameraControl;


    private CameraSettings _settings = default!;
    private MediaPlayer? _mediaPlayer;

    private byte PanSpeed => (byte)Math.Round(PanSpeedSlider.Value);
    private byte TiltSpeed => (byte)Math.Round(TiltSpeedSlider.Value);

    public CameraControlWindow(Camera camera, AppServices services)
    {
        this.InitializeComponent();
        WindowTitleBarHelper.ApplyThemeTitleBar(this);

        _camera = camera;
        _services = services;
        _cameraControl = _services.CameraControlProtocolFactory.Create(camera);

        CameraNameTextBlock.Text = camera.CameraName;
        CameraIpTextBlock.Text = $"{camera.IP} • {camera.Protocol}";

        VideoStreamTextBlock.Text = string.IsNullOrWhiteSpace(camera.PreviewStreamUrl)
            ? "No preview stream URL configured."
            : camera.PreviewStreamUrl;

        _ = InitializeAsync();
    }
    private async Task InitializeAsync()
    {
        await LoadCameraSettingsAsync();
        await InitializePreviewAsync();
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

            await Task.Run(() => { });

            _mediaPlayer?.Dispose();
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

    private async void ReloadPreview_Click(object sender, RoutedEventArgs e)
    {
        await InitializePreviewAsync();
    }

    private async Task LoadCameraSettingsAsync()
    {
        try
        {
            _settings = await _services.CameraRepository.GetSettingsForCameraAsync(_camera.Id);

            PanSpeedSlider.Value = _settings.PanSpeed;
            TiltSpeedSlider.Value = _settings.TiltSpeed;
            GainSlider.Value = _settings.Gain;

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

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _cameraControl.ConnectAsync();

            SetConnectedState(true);
            SetStatus("Connected to camera.");
        }
        catch (Exception ex)
        {
            SetConnectedState(false);
            SetStatus($"Connection failed: {ex.Message}");
        }
    }

    private async void Disconnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _cameraControl.DisconnectAsync();

            SetConnectedState(false);
            SetStatus("Disconnected from camera.");
        }
        catch (Exception ex)
        {
            SetStatus($"Disconnect failed: {ex.Message}");
        }
    }

    private async void PanLeft_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(() =>
            _cameraControl.PanTiltAsync(
                panSpeed: PanSpeed,
                tiltSpeed: TiltSpeed,
                panDirection: 0x01,
                tiltDirection: 0x03),
            "Panning left.");
    }

    private async void PanRight_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(() =>
            _cameraControl.PanTiltAsync(
                panSpeed: PanSpeed,
                tiltSpeed: TiltSpeed,
                panDirection: 0x02,
                tiltDirection: 0x03),
            "Panning right.");
    }

    private async void TiltUp_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(() =>
            _cameraControl.PanTiltAsync(
                panSpeed: PanSpeed,
                tiltSpeed: TiltSpeed,
                panDirection: 0x03,
                tiltDirection: 0x01),
            "Tilting up.");
    }

    private async void TiltDown_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(() =>
            _cameraControl.PanTiltAsync(
                panSpeed: PanSpeed,
                tiltSpeed: TiltSpeed,
                panDirection: 0x03,
                tiltDirection: 0x02),
            "Tilting down.");
    }

    private async void Stop_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.StopPanTiltAsync(),
            "Pan/tilt stopped.");
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

        SetStatus("Auto white balance disabled. Select a manual white balance mode.");
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

            SetConnectedState(_cameraControl.IsConnected);
            SetStatus(successMessage);
        }
        catch (Exception ex)
        {
            SetConnectedState(_cameraControl.IsConnected);
            SetStatus($"Command failed: {ex.Message}");
        }
    }

    private void SetConnectedState(bool connected)
    {
        ConnectionStatusTextBlock.Text = connected ? "Connected" : "Disconnected";

        if (connected)
        {
            ConnectionStatusBadge.Style = (Style)Application.Current.Resources["OnlineStatusBadgeStyle"];
            ConnectionStatusTextBlock.Style = (Style)Application.Current.Resources["OnlineStatusTextStyle"];
        }
        else
        {
            ConnectionStatusBadge.Style = (Style)Application.Current.Resources["OfflineStatusBadgeStyle"];
            ConnectionStatusTextBlock.Style = (Style)Application.Current.Resources["OfflineStatusTextStyle"];
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

    private async void Window_Closed(object sender, WindowEventArgs args)
    {
        await _cameraControl.DisposeAsync();
    }
    private async void UpLeft_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x01, 0x01),
            "Moving up-left.");
    }

    private async void UpRight_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x02, 0x01),
            "Moving up-right.");
    }

    private async void DownLeft_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x01, 0x02),
            "Moving down-left.");
    }

    private async void DownRight_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteCameraCommandAsync(
            () => _cameraControl.PanTiltAsync(PanSpeed, TiltSpeed, 0x02, 0x02),
            "Moving down-right.");
    }

   
}