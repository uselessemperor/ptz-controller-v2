using SQLite;

namespace PTZ_Controller_v2.Models;

public class CameraSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CameraId { get; set; }

    public ExposureMode ExposureMode { get; set; } = ExposureMode.FullAuto;

    public WhiteBalanceMode WhiteBalanceMode { get; set; } = WhiteBalanceMode.Auto;

    public bool AutoWhiteBalanceEnabled { get; set; } = true;

    public int Gain { get; set; } = 0;

    public int CurrentZoom { get; set; } = 0;

    public int PanSpeed { get; set; } = 10;

    public int TiltSpeed { get; set; } = 10;
}