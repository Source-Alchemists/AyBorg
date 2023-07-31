---
title: Create a device provider
description: Developing a device provider for AyBorg allows the creation and management of devices, such as cameras or other hardware peripherals. This guide will walk you through the general process of implementing a device provider, using the interfaces provided by AyBorg SDK.
sidebar: developing-plugins
permalink: "/docs/developing-plugins/create-device-provider.html"
---

Developing a device provider for AyBorg allows the creation and management of devices, such as cameras or other hardware peripherals. This guide will walk you through the general process of implementing a device provider, using the interfaces provided by AyBorg SDK.

## Prerequisites

- Familiarity with C# programming language
- .NET 7+ installed on your machine
- An IDE (e.g., Visual Studio, JetBrains Rider, or Visual Studio Code)

## Implementing device provider class

Create a class implementing `IDeviceProvider`, encapsulating the functionalities of the device provider.

## Provider properties

Define the properties for the provider:

- `Prefix`: A unique prefix for identifying the provider.
- `CanCreate`: A flag to indicate whether the provider can create devices.
- `Name`: The provider's name.
- `Categories`: The collection of categories associated with the provider.

## Device creation method

- `CreateAsync`: Implement this method to create a device instance based on a given ID, returning it asynchronously.

## (Optional) Initialization using `IAfterInitialized` interface

If you need to perform any initialization, such as discovering available camera devices on the network, you can implement the `IAfterInitialized` interface:

```csharp
public interface IAfterInitialized
{
    ValueTask AfterInitializedAsync();
}
```

This interface provides the `AfterInitializedAsync` method, which is called once during initialization, allowing you to perform necessary preparations before the provider starts running.

## Example implementation

```csharp
using AyBorg.SDK.Common;
using Microsoft.Extensions.Logging;

namespace AyBorg.Plugins.ImageTorque;

public sealed class VirtualDeviceProvider : IDeviceProvider
{
    private readonly ILogger<VirtualDeviceProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEnvironment _environment;

    public string Prefix => "AyBV";

    public bool CanCreate => true;

    public string Name => "Virtual Devices";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultDeviceCategories.Camera, "Virtual Device" };

    public VirtualDeviceProvider(ILogger<VirtualDeviceProvider> logger, ILoggerFactory loggerFactory, IEnvironment environment)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _environment = environment;
    }

    public async ValueTask<IDevice> CreateAsync(string id)
    {
        var device = new VirtualDevice(_loggerFactory.CreateLogger<VirtualDevice>(), _environment, id);
        _logger.LogTrace((int)EventLogType.Plugin, "Added virtual device '{id}'", id);
        return await ValueTask.FromResult(device);
    }
}
```

## Conclusion

This guide outlines the general approach to developing a device provider for AyBorg. By following these guidelines and adapting them to your specific device type, you can create a custom device provider that integrates seamlessly with AyBorg.

## Next step

- [Create a camera device]({{site.baseurl}}/docs/developing-plugins/create-camera-device)