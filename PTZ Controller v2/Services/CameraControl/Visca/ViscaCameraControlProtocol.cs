using PTZ_Controller_v2.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services.CameraControl.Visca;

public sealed class ViscaCameraControlProtocol : ICameraControlProtocol
{
    private readonly ViscaClient _client;

    public Camera Camera { get; }

    public bool IsConnected => _client.IsConnected;

    public ViscaCameraControlProtocol(Camera camera)
    {
        Camera = camera;

        var port = GetPortFromCamera(camera);

        _client = new ViscaClient(camera.IP, port);
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return _client.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync()
    {
        return _client.DisconnectAsync();
    }

    public Task PanTiltAsync(
        byte panSpeed,
        byte tiltSpeed,
        byte panDirection,
        byte tiltDirection,
        CancellationToken cancellationToken = default)
    {
        return _client.SendCommandAsync(
            new byte[]
            {
                0x81, 0x01, 0x06, 0x01,
                panSpeed, tiltSpeed,
                panDirection, tiltDirection,
                0xFF
            },
            cancellationToken);
    }

    public Task StopPanTiltAsync(CancellationToken cancellationToken = default)
    {
        return PanTiltAsync(
            panSpeed: 0x00,
            tiltSpeed: 0x00,
            panDirection: 0x03,
            tiltDirection: 0x03,
            cancellationToken);
    }

    public Task ZoomInAsync(CancellationToken cancellationToken = default)
    {
        return _client.SendCommandAsync(
            new byte[] { 0x81, 0x01, 0x04, 0x07, 0x20, 0xFF },
            cancellationToken);
    }

    public Task ZoomOutAsync(CancellationToken cancellationToken = default)
    {
        return _client.SendCommandAsync(
            new byte[] { 0x81, 0x01, 0x04, 0x07, 0x30, 0xFF },
            cancellationToken);
    }

    public Task StopZoomAsync(CancellationToken cancellationToken = default)
    {
        return _client.SendCommandAsync(
            new byte[] { 0x81, 0x01, 0x04, 0x07, 0x00, 0xFF },
            cancellationToken);
    }

    public Task SetExposureModeAsync(
        ExposureMode exposureMode,
        CancellationToken cancellationToken = default)
    {
        return _client.SendCommandAsync(
            new byte[] { 0x81, 0x01, 0x04, 0x39, (byte)exposureMode, 0xFF },
            cancellationToken);
    }

    public Task SetGainAsync(
        byte level,
        CancellationToken cancellationToken = default)
    {
        if (level > 0x0F)
            level = 0x0F;

        return _client.SendCommandAsync(
            new byte[]
            {
                0x81, 0x01, 0x04, 0x4C,
                0x00, 0x00, 0x00, level,
                0xFF
            },
            cancellationToken);
    }

    public Task SetWhiteBalanceModeAsync(
        WhiteBalanceMode whiteBalanceMode,
        CancellationToken cancellationToken = default)
    {
        return _client.SendCommandAsync(
            new byte[] { 0x81, 0x01, 0x04, 0x35, (byte)whiteBalanceMode, 0xFF },
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    private static int GetPortFromCamera(Camera camera)
    {
        // For now, default VISCA-over-IP port.
        // Later, add Port to the Camera table.
        return 5678;
    }
}