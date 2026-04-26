namespace PTZ_Controller_v2.Models;

public enum ExposureMode : byte
{
    FullAuto = 0x00,
    Manual = 0x03,
    ShutterPriority = 0x0A,
    IrisPriority = 0x0B
}

public enum WhiteBalanceMode : byte
{
    Auto = 0x00,
    Indoor = 0x01,
    Outdoor = 0x02,
    OnePush = 0x03,
    Manual = 0x04
}