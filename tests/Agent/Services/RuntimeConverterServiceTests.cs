using System.Globalization;
using Atomy.Agent.Services;
using Atomy.Agent.Tests.Dummies;
using Atomy.SDK;
using Atomy.SDK.DAL;
using Atomy.SDK.Ports;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

#nullable disable

namespace Atomy.Agent.Tests.Services;

public class RuntimeConverterServiceTests
{
    private static NullLogger<RuntimeConverterService> _logger = new NullLogger<RuntimeConverterService>();

    [Fact]
    public async Task TestConvertProjectRecordToProject()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var projectRecord = CreateEmptyProjectRecord();
        var inputPort1 = new PortRecord
        {
            Id = Guid.NewGuid(),
            Name = "String input",
            Value = "Test string",
            Brand = PortBrand.String,
            Direction = PortDirection.Input
        };
        var inputPort2 = new PortRecord
        {
            Id = Guid.NewGuid(),
            Name = "Numeric input",
            Value = "12346789.123",
            Brand = PortBrand.Numeric,
            Direction = PortDirection.Input
        };
        var outputPort = new PortRecord
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
            MetaInfo = new PluginMetaInfo
            {
                TypeName = nameof(DummyStep)
            },
            X = 123,
            Y = 456,
            Ports = new List<PortRecord> { inputPort1, inputPort2, outputPort }
        };
        projectRecord.Steps.Add(stepRecord);

        var pluginsServiceMock = new Mock<IPluginsService>();
        pluginsServiceMock.Setup(x => x.Find(stepRecord)).Returns(new StepProxy(new DummyStep()));

        var service = new RuntimeConverterService(_logger, serviceProviderMock.Object, pluginsServiceMock.Object);

        // Act
        var project = await service.ConvertAsync(projectRecord);

        // Assert
        Assert.NotNull(project);
        Assert.Equal(projectRecord.Meta.Name, project.Meta.Name);

        Assert.NotEmpty(project.Steps);
        Assert.Single(project.Steps);
        var step = project.Steps.First();
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