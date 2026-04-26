using PTZ_Controller_v2.Models;

namespace PTZ_Controller_v2;

public sealed class CameraCardViewModel
{
    public Camera Camera { get; }

    public int Id => Camera.Id;

    public string CameraName => Camera.CameraName;

    public string IP => Camera.IP;

    public string Protocol => Camera.Protocol;

    public string UserId => Camera.UserId;

  
    public string PreviewStreamUrl => Camera.PreviewStreamUrl;

    public string TrackingStreamUrl => Camera.TrackingStreamUrl;
    public string PreviewStreamStatus =>
    string.IsNullOrWhiteSpace(Camera.PreviewStreamUrl) ? "Not set" : "Configured";

    public string TrackingStreamStatus =>
        string.IsNullOrWhiteSpace(Camera.TrackingStreamUrl) ? "Not set" : "Configured";

    public CameraCardViewModel(Camera camera)
    {
        Camera = camera;
    }
}