using PTZ_Controller_v2.Models;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services;

public interface ICameraControlService
{
    Task ConnectAsync(Camera camera);

    Task PanLeftAsync(Camera camera);

    Task PanRightAsync(Camera camera);

    Task TiltUpAsync(Camera camera);

    Task TiltDownAsync(Camera camera);

    Task StopAsync(Camera camera);
}