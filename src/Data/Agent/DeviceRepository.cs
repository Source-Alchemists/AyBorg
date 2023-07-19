namespace AyBorg.Data.Agent;

public sealed class DeviceRepository : IDeviceRepository
{
    public ValueTask<IEnumerable<DeviceRecord>> GetAllAsync() => throw new NotImplementedException();
    public ValueTask<DeviceRecord> AddAsync(DeviceRecord device) => throw new NotImplementedException();
    public ValueTask<DeviceRecord> RemoveAsync(DeviceRecord device) => throw new NotImplementedException();
    public ValueTask<DeviceRecord> UpdateAsync(DeviceRecord device) => throw new NotImplementedException();
}
