using PTZ_Controller_v2.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services.CameraControl;

public interface ICameraControlProtocol : IAsyncDisposable
{
    Camera Camera { get; }

    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync();

    Task PanTiltAsync(
        byte panSpeed,
        byte tiltSpeed,
        byte panDirection,
        byte tiltDirection,
        CancellationToken cancellationToken = default);

    Task StopPanTiltAsync(CancellationToken cancellationToken = default);

    Task ZoomInAsync(CancellationToken cancellationToken = default);

    Task ZoomOutAsync(CancellationToken cancellationToken = default);

    Task StopZoomAsync(CancellationToken cancellationToken = default);

    Task SetExposureModeAsync(
        ExposureMode exposureMode,
        CancellationToken cancellationToken = default);

    Task SetGainAsync(
        byte level,
        CancellationToken cancellationToken = default);

    Task SetWhiteBalanceModeAsync(
        WhiteBalanceMode whiteBalanceMode,
        CancellationToken cancellationToken = default);
}