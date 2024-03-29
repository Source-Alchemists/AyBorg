using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AyBorg.Data.Agent;

public sealed class DeviceRepository : IDeviceRepository
{
    private readonly IDbContextFactory<DeviceContext> _contextFactory;

    public DeviceRepository(IDbContextFactory<DeviceContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<IEnumerable<DeviceRecord>> GetAllAsync()
    {
        using DeviceContext context = await _contextFactory.CreateDbContextAsync();
        return await context.AyBorgDevices!.Include(d => d.MetaInfo)
                                            .Include(d => d.ProviderMetaInfo)
                                            .Include(d => d.Ports)
                                            .ToListAsync();
    }

    public async ValueTask<DeviceRecord> AddAsync(DeviceRecord device)
    {
        using DeviceContext context = await _contextFactory.CreateDbContextAsync();
        EntityEntry<DeviceRecord> result = await context.AyBorgDevices!.AddAsync(device);
        await context.SaveChangesAsync();
        return result.Entity;
    }
    public async ValueTask<DeviceRecord> RemoveAsync(DeviceRecord device)
    {
        using DeviceContext context = await _contextFactory.CreateDbContextAsync();
        List<DeviceRecord> targetDevices = await context.AyBorgDevices!.Include(d => d.MetaInfo).Include(d => d.Ports).Where(d => d.Id.Equals(device.Id)).ToListAsync();
        DeviceRecord targetDevice = targetDevices.Single();
        IEnumerable<DevicePortRecord> targetPorts = targetDevice.Ports;

        context.Remove(targetDevice);
        if (targetPorts.Any())
        {
            context.RemoveRange(targetPorts);
        }
        context.Remove(targetDevice.MetaInfo);
        await context.SaveChangesAsync();
        return targetDevice;
    }
    public async ValueTask<DeviceRecord> UpdateAsync(DeviceRecord device)
    {
        using DeviceContext context = await _contextFactory.CreateDbContextAsync();
        DeviceRecord? targetDevice = await context.AyBorgDevices!.Include(d => d.Ports)
                                                                .FirstOrDefaultAsync(d => d.Id.Equals(device.Id))
                                                                ?? throw new KeyNotFoundException($"Device '{device.Id}' not found");

        targetDevice.IsActive = device.IsActive;

        foreach(DevicePortRecord? sp in device.Ports)
        {
            DevicePortRecord tp = targetDevice.Ports.Single(d => d.Id.Equals(sp.Id));
            tp.Value = sp.Value;
        }

        await context.SaveChangesAsync();
        return targetDevice;
    }
}
