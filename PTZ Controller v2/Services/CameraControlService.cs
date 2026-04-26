using PTZ_Controller_v2.Models;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services;

public sealed class CameraControlService : ICameraControlService
{
    public Task ConnectAsync(Camera camera)
    {
        // TODO: Add ONVIF/VISCA connection logic later.
        return Task.CompletedTask;
    }

    public Task PanLeftAsync(Camera camera)
    {
        // TODO: Send pan-left command.
        return Task.CompletedTask;
    }

    public Task PanRightAsync(Camera camera)
    {
        // TODO: Send pan-right command.
        return Task.CompletedTask;
    }

    public Task TiltUpAsync(Camera camera)
    {
        // TODO: Send tilt-up command.
        return Task.CompletedTask;
    }

    public Task TiltDownAsync(Camera camera)
    {
        // TODO: Send tilt-down command.
        return Task.CompletedTask;
    }

    public Task StopAsync(Camera camera)
    {
        // TODO: Send stop command.
        return Task.CompletedTask;
    }
}