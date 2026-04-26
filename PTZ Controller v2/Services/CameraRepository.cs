using PTZ_Controller_v2.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PTZ_Controller_v2.Services;

public sealed class CameraRepository : ICameraRepository
{
    private readonly SQLiteAsyncConnection _database;
    private bool _initialized;

    public CameraRepository(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await _database.CreateTableAsync<Camera>();
        await _database.CreateTableAsync<CameraSettings>();

        _initialized = true;
    }

    public async Task<List<Camera>> GetCamerasAsync()
    {
        await InitializeAsync();
        return await _database.Table<Camera>().ToListAsync();
    }

    public async Task<Camera?> GetCameraAsync(int cameraId)
    {
        await InitializeAsync();

        return await _database
            .Table<Camera>()
            .Where(c => c.Id == cameraId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveCameraAsync(Camera camera)
    {
        await InitializeAsync();

        if (camera.Id == 0)
        {
            await _database.InsertAsync(camera);

            var defaultSettings = new CameraSettings
            {
                CameraId = camera.Id,
                ExposureMode = ExposureMode.FullAuto,
                WhiteBalanceMode = WhiteBalanceMode.Auto,
                AutoWhiteBalanceEnabled = true,
                Gain = 0,
                CurrentZoom = 0,
                PanSpeed = 10,
                TiltSpeed = 10
            };

            await _database.InsertAsync(defaultSettings);

            return camera.Id;
        }

        return await _database.UpdateAsync(camera);
    }

    public async Task<int> DeleteCameraAsync(Camera camera)
    {
        await InitializeAsync();

        await DeleteCameraSettingsAsync(camera.Id);

        return await _database.DeleteAsync(camera);
    }

    public async Task<CameraSettings> GetSettingsForCameraAsync(int cameraId)
    {
        await InitializeAsync();

        var settings = await _database
            .Table<CameraSettings>()
            .Where(s => s.CameraId == cameraId)
            .FirstOrDefaultAsync();

        if (settings != null)
            return settings;

        settings = new CameraSettings
        {
            CameraId = cameraId,
            ExposureMode = ExposureMode.FullAuto,
            WhiteBalanceMode = WhiteBalanceMode.Auto,
            AutoWhiteBalanceEnabled = true,
            Gain = 0,
            CurrentZoom = 0,
            PanSpeed = 10,
            TiltSpeed = 10
        };

        await _database.InsertAsync(settings);

        return settings;
    }

    public async Task<int> SaveCameraSettingsAsync(CameraSettings settings)
    {
        await InitializeAsync();

        if (settings.Id == 0)
            return await _database.InsertAsync(settings);

        return await _database.UpdateAsync(settings);
    }

    public async Task<int> DeleteCameraSettingsAsync(int cameraId)
    {
        await InitializeAsync();

        var settings = await _database
            .Table<CameraSettings>()
            .Where(s => s.CameraId == cameraId)
            .ToListAsync();

        var deleteCount = 0;

        foreach (var item in settings)
        {
            deleteCount += await _database.DeleteAsync(item);
        }

        return deleteCount;
    }

    public async ValueTask DisposeAsync()
    {
        await _database.CloseAsync();
    }
}