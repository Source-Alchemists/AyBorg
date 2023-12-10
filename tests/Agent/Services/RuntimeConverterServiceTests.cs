using System.Collections.Immutable;
using System.Globalization;
using AyBorg.Agent.Services;
using AyBorg.Agent.Tests.Dummies;
using AyBorg.Data.Agent;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AyBorg.Agent.Tests.Services;

public class RuntimeConverterServiceTests
{
    private static readonly NullLogger<RuntimeConverterService> s_logger = new();
    private readonly Mock<IServiceProvider> _mockServiceProvider = new();
    private readonly Mock<IPluginsService> _mockPluginService = new();
    private readonly RuntimeConverterService _service;

    public RuntimeConverterServiceTests()
    {
        _service = new RuntimeConverterService(new NullLoggerFactory(), s_logger, _mockServiceProvider.Object, _mockPluginService.Object);
    }

    [Theory]
    [InlineData(true, PortBrand.Boolean, true, true)]
    [InlineData(true, PortBrand.String, "abc", "abc")]
    [InlineData(true, PortBrand.Folder, "/abc", "/abc")]
    [InlineData(true, PortBrand.Numeric, 456, 456)]
    [InlineData(true, PortBrand.Enum, PortBrand.Enum, PortBrand.Enum)]
    [InlineData(true, PortBrand.Rectangle, null, null)]
    [InlineData(true, PortBrand.Rectangle, "{\"X\":5,\"Y\":6,\"Width\":7,\"Height\":8}", null)]
    [InlineData(true, PortBrand.Image, null, null)]
    [InlineData(true, PortBrand.StringCollection, null, null)]
    [InlineData(true, PortBrand.StringCollection, "[\"Test1\",\"Test2\"]", null)]
    public async Task Test_TryUpdatePortValueAsync(bool expectedSuccess, PortBrand portBrand, object value, object expectedValue)
    {
        // Arrange
        if (portBrand == PortBrand.Rectangle)
        {
            expectedValue = new ImageTorque.Rectangle { X = 5, Y = 6, Width = 7, Height = 8 };
            value ??= expectedValue;
        }

        if(portBrand == PortBrand.StringCollection)
        {
            expectedValue = new List<string> { "Test1", "Test2" }.ToImmutableList();
            value ??= expectedValue;
        }

        IPort port = null!;
        switch (portBrand)
        {
            case PortBrand.String:
                port = new StringPort("Port", PortDirection.Input, "123");
                break;
            case PortBrand.Folder:
                port = new FolderPort("Port", PortDirection.Input, "/123");
                break;
            case PortBrand.Numeric:
                port = new NumericPort("Port", PortDirection.Input, 123);
                break;
            case PortBrand.Boolean:
                port = new BooleanPort("Port", PortDirection.Input, false);
                break;
            case PortBrand.Enum:
                port = new EnumPort("Port", PortDirection.Input, PortBrand.Enum);
                break;
            case PortBrand.Rectangle:
                port = new RectanglePort("Port", PortDirection.Input, new ImageTorque.Rectangle { X = 1, Y = 2, Width = 3, Height = 4 });
                break;
            case PortBrand.Image:
                port = new ImagePort("Port", PortDirection.Input, null!);
                break;
            case PortBrand.StringCollection:
                port = new StringCollectionPort("Port", PortDirection.Input, ImmutableList<string>.Empty);
                break;
        }

        // Act
        bool result = await _service.TryUpdatePortValueAsync(port, value);

        // Assert
        Assert.Equal(expectedSuccess, result);
        switch (portBrand)
        {
            case PortBrand.String:
                Assert.Equal(expectedValue, ((StringPort)port).Value);
                break;
            case PortBrand.Folder:
                Assert.Equal(expectedValue, ((FolderPort)port).Value);
                break;
            case PortBrand.Numeric:
                Assert.Equal(Convert.ToDouble(expectedValue), ((NumericPort)port).Value);
                break;
            case PortBrand.Boolean:
                Assert.Equal(expectedValue, ((BooleanPort)port).Value);
                break;
            case PortBrand.Enum:
                Assert.Equal(expectedValue, ((EnumPort)port).Value);
                break;
            case PortBrand.Rectangle:
                Assert.Equal(expectedValue, ((RectanglePort)port).Value);
                break;
            case PortBrand.Image:
                Assert.Null(((ImagePort)port).Value);
                break;
            case PortBrand.StringCollection:
                Assert.Equal(expectedValue, ((StringCollectionPort)port).Value);
                break;
        }
    }

    [Fact]
    public async Task Test_UpdateStringCollection_WidthNullValue()
    {
        // Arrange
        var expectedValue = new List<string>{ string.Empty, string.Empty }.ToImmutableList();
        var value = new List<string> { null!, null! }.ToImmutableList();
        var port = new StringCollectionPort("Port", PortDirection.Input, ImmutableList<string>.Empty);

        // Act
        bool result = await _service.TryUpdatePortValueAsync(port, value);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, port.Value);
    }

    [Fact]
    public async Task Test_ConvertAsync()
    {
        // Arrange
        ProjectRecord projectRecord = CreateEmptyProjectRecord();
        var inputPort1 = new StepPortRecord
        {
            Id = Guid.NewGuid(),
            Name = "String input",
            Value = "Test string",
            Brand = PortBrand.String,
            Direction = PortDirection.Input
        };
        var inputPort2 = new StepPortRecord
        {
            Id = Guid.NewGuid(),
            Name = "Numeric input",
            Value = "12346789.123",
            Brand = PortBrand.Numeric,
            Direction = PortDirection.Input
        };
        var outputPort = new StepPortRecord
        {
            Id = Guid.NewGuid(),
            Name = "Output",
            Value = "000.001",
            Brand = PortBrand.Numeric,
            Direction = PortDirection.Output
        };
        var stepRecord = new StepRecord
        {
            Id = Guid.NewGuid(),
            Name = "Test step",
            MetaInfo = new PluginMetaInfoRecord
            {
                TypeName = nameof(DummyStep)
            },
            X = 123,
            Y = 456,
            Ports = new List<StepPortRecord> { inputPort1, inputPort2, outputPort }
        };
        projectRecord.Steps.Add(stepRecord);
        projectRecord.Links.Add(new LinkRecord());

        _mockPluginService.Setup(x => x.Find(stepRecord)).Returns(new StepProxy(new NullLogger<StepProxy>(), new DummyStep()));

        // Act
        Project project = await _service.ConvertAsync(projectRecord);

        // Assert
        Assert.NotNull(project);
        Assert.Equal(projectRecord.Meta.Name, project.Meta.Name);

        Assert.NotEmpty(project.Steps);
        Assert.Single(project.Steps);
        IStepProxy step = project.Steps.First();
        Assert.Equal(stepRecord.Name, step.Name);
        Assert.Equal(stepRecord.X, step.X);
        Assert.Equal(stepRecord.Y, step.Y);

        var input1 = step.Ports.First(x => x.Name == inputPort1.Name) as StringPort;
        var input2 = step.Ports.First(x => x.Name == inputPort2.Name) as NumericPort;
        var output = step.Ports.First(x => x.Name == outputPort.Name) as NumericPort;
        Assert.NotNull(input1);
        Assert.NotNull(input2);
        Assert.NotNull(output);
        Assert.Equal(inputPort1.Value, input1.Value);
        Assert.Equal(inputPort1.Id, input1.Id);
        Assert.Equal(Convert.ToDouble(inputPort2.Value, CultureInfo.InvariantCulture), input2.Value);
        Assert.Equal(inputPort2.Id, input2.Id);
        Assert.Equal(Convert.ToDouble(outputPort.Value, CultureInfo.InvariantCulture), output.Value);
        Assert.Equal(outputPort.Id, output.Id);
    }

    private static ProjectRecord CreateEmptyProjectRecord()
    {
        return new ProjectRecord
        {
            DbId = Guid.NewGuid(),
            Meta = new ProjectMetaRecord
            {
                DbId = Guid.NewGuid(),
                Name = "Test",
                CreatedDate = DateTime.Now, // Just to have two different dates
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                State = ProjectState.Draft
            }
        };
    }
}
