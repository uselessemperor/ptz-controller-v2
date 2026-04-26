using SQLite;

namespace PTZ_Controller_v2.Models;

public class Camera
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string CameraName { get; set; } = string.Empty;

    public string IP { get; set; } = string.Empty;

    public string Protocol { get; set; } = "VISCA";

    public string UserId { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PreviewStreamUrl { get; set; } = string.Empty;

    public string TrackingStreamUrl { get; set; } = string.Empty;
}