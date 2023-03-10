using AyBorg.Data.Agent;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Tests;

public class AuditMapperTests{

    [Fact]
    public void Test_MapProjectRecordToProjectAuditEntry()
    {
        // Arrange
        var project = new ProjectRecord {
            Meta = new ProjectMetaRecord {
                Id = Guid.NewGuid(),
                Name = "TestProject",
                State = ProjectState.Draft
            },
            Settings = new ProjectSettingsRecord {
                IsForceResultCommunicationEnabled = true
            },
            Steps = new List<StepRecord> {
                new StepRecord {
                    Id = Guid.NewGuid(),
                    Name = "TestStep",
                    MetaInfo = new PluginMetaInfoRecord(),
                    X = 0,
                    Y = 0,
                    Ports = new List<PortRecord> {
                        new PortRecord {
                            Id = Guid.NewGuid(),
                            Direction = SDK.Common.Ports.PortDirection.Input,
                            Name = "TestPort",
                            Value = "TestValue",
                            Brand = SDK.Common.Ports.PortBrand.String
                        }
                    }
                }
            }
        };

        // Act
        Ayborg.Gateway.Audit.V1.AgentProjectAuditEntry result = AuditMapper.Map(project);

        // Assert
        Assert.Equal(project.Meta.Id.ToString(), result.Id);
        Assert.Equal(project.Meta.Name, result.Name);
        Assert.Equal((int)project.Meta.State, result.State);
        Assert.True(result.Settings.IsForceResultCommunicationEnabled);
        Assert.Equal(project.Steps.Count, result.Steps.Count);
        Assert.Equal(project.Steps[0].Id.ToString(), result.Steps[0].Id);
        Assert.Equal(project.Steps[0].Name, result.Steps[0].Name);
        Assert.Equal(project.Steps[0].X, result.Steps[0].X);
        Assert.Equal(project.Steps[0].Y, result.Steps[0].Y);
        Assert.Equal(project.Steps[0].MetaInfo.AssemblyName, result.Steps[0].AssemblyName);
        Assert.Equal(project.Steps[0].MetaInfo.AssemblyVersion, result.Steps[0].AssemblyVersion);
        Assert.Equal(project.Steps[0].MetaInfo.TypeName, result.Steps[0].TypeName);
        Assert.Equal(project.Steps[0].Ports.Count, result.Steps[0].Ports.Count);
        Assert.Equal(project.Steps[0].Ports[0].Id.ToString(), result.Steps[0].Ports[0].Id);
        Assert.Equal(project.Steps[0].Ports[0].Name, result.Steps[0].Ports[0].Name);
        Assert.Equal(project.Steps[0].Ports[0].Value, result.Steps[0].Ports[0].Value);
        Assert.Equal((int)project.Steps[0].Ports[0].Brand, result.Steps[0].Ports[0].Brand);
        Assert.Equal((int)project.Steps[0].Ports[0].Direction, result.Steps[0].Ports[0].Direction);
    }
}
