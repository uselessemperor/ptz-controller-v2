using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services.CameraControl.Visca;

public sealed class ViscaClient : IAsyncDisposable
{
    private readonly string _cameraIp;
    private readonly int _port;

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;

    public bool IsConnected => _tcpClient?.Connected == true && _stream != null;

    public ViscaClient(string cameraIp, int port = 5678)
    {
        _cameraIp = cameraIp;
        _port = port;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        _tcpClient = new TcpClient();

        await _tcpClient.ConnectAsync(_cameraIp, _port, cancellationToken);

        _stream = _tcpClient.GetStream();
    }

    public async Task SendCommandAsync(
        byte[] command,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            await ConnectAsync(cancellationToken);

        if (_stream == null)
            throw new InvalidOperationException("VISCA stream is not available.");

        await _stream.WriteAsync(command, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    public Task DisconnectAsync()
    {
        _stream?.Dispose();
        _stream = null;

        _tcpClient?.Dispose();
        _tcpClient = null;

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}