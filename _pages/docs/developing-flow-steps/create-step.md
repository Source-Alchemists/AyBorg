---
title: Create a step
description: How develop a workflow step plugin.
sidebar: developing-flow-steps
permalink: "/docs/developing-flow-steps/create-step.html"
---

## Creating a new step plugin for AyBorg: `IStepBody` interface

AyBorg's `IStepBody` interface is the primary means of extending AyBorg's functionality. In this guide, we'll walk through how to create a new plugin (step) using this interface.

### Prerequisites

- Familiarity with C# programming language
- .NET 7+ installed on your machine
- An IDE (e.g., Visual Studio, JetBrains Rider, or Visual Studio Code)

### Using the AyBorg.SDK.Common SDK

In order to develop plugins for AyBorg, you will need to use the `AyBorg.SDK.Common` SDK. This SDK provides the necessary interfaces and classes to create plugins.

### Understanding the IStepBody interface

The `IStepBody` interface is central to creating a new plugin in AyBorg. The interface ensures that your plugin has a `TryRunAsync` method that the AyBorg system can call when it's time to execute your step. It also requires that your plugin defines its `Name` and `Ports`. Here's the interface structure:

```csharp
public interface IStepBody
{
    string Name { get; }
    IEnumerable<IPort> Ports { get; }
    ValueTask<bool> TryRunAsync(CancellationToken cancellationToken);
}
```

### Creating a new step

To create a new step, you will need to create a new class that implements the `IStepBody` interface. We'll take the `ImageScale` class as a case study.

#### Step 1: Define your class

Your class should implement the `IStepBody` interface and optionally `IDisposable` if it requires cleanup after use. It should also include several `IPort` fields, representing inputs and outputs of your step. Here's the class definition:

```csharp
public sealed class ImageScale : IStepBody, IDisposable
{
    private readonly ImagePort _imagePort = new("Image", PortDirection.Input, null!);
    private readonly ImagePort _scaledImagePort = new("Scaled image", PortDirection.Output, null!);
    private readonly NumericPort _widthPort = new("Width", PortDirection.Output, 0);
    private readonly NumericPort _heightPort = new("Height", PortDirection.Output, 0);
    private readonly NumericPort _scalePort = new("Scale factor", PortDirection.Input, 0.5d, 0.01d, 2d);
    private bool _disposedValue;

    public string Name => "Image.Scale";

    public IReadOnlyCollection<string> Categories { get; } = new List<string> { DefaultStepCategories.ImageProcessing };

    public IEnumerable<IPort> Ports { get; }

    public ImageScale()
    {
        Ports = new List<IPort>
        {
            _imagePort,
            _scaledImagePort,
            _widthPort,
            _heightPort,
            _scalePort
        };
    }
    // ...
}
```

#### Step 2: Implement the TryRunAsync method

The `TryRunAsync` method is where the main logic of your plugin will reside. For our `ImageScale` example, this method scales an image to a certain size:

```csharp
public ValueTask<bool> TryRunAsync(CancellationToken cancellationToken)
{
    _scaledImagePort.Value?.Dispose();
    Image sourceImage = _imagePort.Value;
    if (_scalePort.Value.Equals(1d))
    {
        _scaledImagePort.Value = sourceImage;
        return ValueTask.FromResult(true);
    }

    int w = (int)(sourceImage.Width * _scalePort.Value);
    int h = (int)(sourceImage.Height * _scalePort.Value);
    _scaledImagePort.Value = sourceImage.Resize(w, h);
    _widthPort.Value = w;
    _heightPort.Value = h;
    return ValueTask.FromResult(true);
}
```

#### Step 3: Implement the Dispose method (Optional)

If your plugin acquires resources that need to be released, it should implement the `IDisposable` interface and its `Dispose` method:

```csharp
public void Dispose()
{
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
}

private void Dispose(bool disposing)
{
    if (!_disposedValue && disposing)
    {
        _scaledImagePort?.Dispose();
        _disposedValue = true;
    }
}
```

#### Step 4: Build and deploy your step

After you've implemented your plugin, you can build and deploy the resulting DLL to the AyBorg's plugins directory.

That's it! You've just created a new plugin for AyBorg using the `IStepBody` interface. Remember, the logic inside the `TryRunAsync` method can be whatever you need for your specific use case.

### Conclusion

Creating plugins for AyBorg using the `IStepBody` interface allows developers to extend the functionality of AyBorg in powerful and flexible ways. With the ability to add new steps to workflows, the possibilities are endless. Happy coding!

Please note: This guide assumes that you have a working knowledge of C# and .NET. If you're new to these technologies, you may need to familiarize yourself with them before you can create a plugin for AyBorg.
