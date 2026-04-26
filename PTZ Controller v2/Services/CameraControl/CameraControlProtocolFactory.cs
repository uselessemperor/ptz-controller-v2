using PTZ_Controller_v2.Models;
using PTZ_Controller_v2.Services.CameraControl.Visca;
using System;

namespace PTZ_Controller_v2.Services.CameraControl;

public interface ICameraControlProtocolFactory
{
    ICameraControlProtocol Create(Camera camera);
}

public sealed class CameraControlProtocolFactory : ICameraControlProtocolFactory
{
    public ICameraControlProtocol Create(Camera camera)
    {
        return camera.Protocol?.Trim().ToUpperInvariant() switch
        {
            "VISCA" => new ViscaCameraControlProtocol(camera),
            "VISCA-IP" => new ViscaCameraControlProtocol(camera),

            "ONVIF" => throw new NotSupportedException("ONVIF control is not implemented yet."),
            "PELCO-D" => throw new NotSupportedException("PELCO-D control is not implemented yet."),
            "PELCO-P" => throw new NotSupportedException("PELCO-P control is not implemented yet."),

            _ => throw new NotSupportedException(
                $"Camera protocol '{camera.Protocol}' is not supported.")
        };
    }
}