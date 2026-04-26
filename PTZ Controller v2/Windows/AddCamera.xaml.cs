using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PTZ_Controller_v2.Models;
using PTZ_Controller_v2.Services;

namespace PTZ_Controller_v2;

public sealed partial class AddCameraWindow : Window
{
    private readonly ICameraRepository _cameraRepository;

    public AddCameraWindow(ICameraRepository cameraRepository)
    {
        this.InitializeComponent();
        _cameraRepository = cameraRepository;
    }

    private async void SaveCamera_Click(object sender, RoutedEventArgs e)
    {
        var cameraName = CameraNameTextBox.Text?.Trim();
        var ipAddress = IpAddressTextBox.Text?.Trim();
        var userId = UserIdTextBox.Text?.Trim();
        var password = PasswordBox.Password?.Trim();
        var previewStreamUrl = PreviewStreamUrlTextBox.Text?.Trim();
        var trackingStreamUrl = TrackingStreamUrlTextBox.Text?.Trim();

        var protocol = GetSelectedComboBoxValue(ProtocolComboBox);
        var videoFeedbackProtocol = GetSelectedComboBoxValue(VideoFeedbackProtocolComboBox);

        if (string.IsNullOrWhiteSpace(cameraName))
        {
            ShowError("Camera name is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ShowError("IP address is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(protocol))
        {
            ShowError("Control protocol is required.");
            return;
        }

        var camera = new Camera
        {
            CameraName = cameraName,
            IP = ipAddress,
            Protocol = protocol,
            UserId = userId ?? string.Empty,
            Password = password ?? string.Empty,
            PreviewStreamUrl = previewStreamUrl ?? string.Empty,
            TrackingStreamUrl = trackingStreamUrl?? string.Empty
        };

        await _cameraRepository.SaveCameraAsync(camera);

        this.Close();
    }

    private static string GetSelectedComboBoxValue(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            return selectedItem.Content?.ToString() ?? string.Empty;

        return string.Empty;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.Visibility = Visibility.Visible;
    }
}