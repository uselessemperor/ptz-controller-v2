using PTZ_Controller_v2.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services;

public interface ICameraRepository : IAsyncDisposable
{
    Task InitializeAsync();

    Task<List<Camera>> GetCamerasAsync();

    Task<Camera?> GetCameraAsync(int cameraId);

    Task<int> SaveCameraAsync(Camera camera);

    Task<int> DeleteCameraAsync(Camera camera);

    Task<CameraSettings> GetSettingsForCameraAsync(int cameraId);

    Task<int> SaveCameraSettingsAsync(CameraSettings settings);

    Task<int> DeleteCameraSettingsAsync(int cameraId);
}