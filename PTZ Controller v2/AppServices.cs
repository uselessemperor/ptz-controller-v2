using PTZ_Controller_v2.Services;
using PTZ_Controller_v2.Services.CameraControl;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PTZ_Controller_v2;

public sealed class AppServices : IAsyncDisposable
{
    public ICameraRepository CameraRepository { get; }

    public ICameraControlProtocolFactory CameraControlProtocolFactory { get; }

    public AppServices()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cameras.db3");

        CameraRepository = new CameraRepository(dbPath);
        CameraControlProtocolFactory = new CameraControlProtocolFactory();
    }

    public async Task InitializeAsync()
    {
        await CameraRepository.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await CameraRepository.DisposeAsync();
    }
}