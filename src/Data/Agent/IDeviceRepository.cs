namespace AyBorg.Data.Agent;

public interface IDeviceRepository
{
    ValueTask<IEnumerable<DeviceRecord>> GetAllAsync();
    ValueTask<DeviceRecord> AddAsync(DeviceRecord device);
    ValueTask<DeviceRecord> RemoveAsync(DeviceRecord device);
    ValueTask<DeviceRecord> UpdateAsync(DeviceRecord device);
}
