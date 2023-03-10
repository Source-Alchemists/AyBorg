using AyBorg.Data.Audit.Models.Agent;
using AyBorg.SDK.Common.Ports;

namespace AyBorg.Audit.Services.Tests;

public class AgentCompareServiceTest
{
    private static StepAuditRecord CreateStep(Guid id, string name, string assemblyName, string assemblyVersion, string typeName, List<PortAuditRecord> ports)
    {
        return new StepAuditRecord
        {
            Id = id,
            Name = name,
            X = 1,
            Y = 2,
            AssemblyName = assemblyName,
            AssemblyVersion = assemblyVersion,
            TypeName = typeName,
            Ports = ports
        };
    }

    private static PortAuditRecord CreatePort(Guid id, string name, string value, PortBrand brand = PortBrand.String, PortDirection direction = PortDirection.Input)
    {
        return new PortAuditRecord {
            Id = id,
            Name = name,
            Value = value,
            Brand = brand,
            Direction = direction
        };
    }

    [Fact]
    public void Test_CompareTwoChangesetsFromSameProject()
    {
        // Arrange
        var projectAuditRecords = new List<ProjectAuditRecord>();

        var projectId = Guid.NewGuid();

        var knownStepId = Guid.NewGuid();
        var knownStep2Id = Guid.NewGuid();
        var knownStep3Id = Guid.NewGuid();
        var knownStep4Id = Guid.NewGuid();
        var knownStep5Id = Guid.NewGuid();
        string knownStepName = "Known.Step";
        string knownStepAssemblyName = "Test.Assembly";
        string knownStepAssemblyVersion = "1.0.0";
        string knownStepTypeName = "Known.Step.Type";

        var knownPortId = Guid.NewGuid();
        var knownPort2Id = Guid.NewGuid();
        var knownInputPortId = Guid.NewGuid();
        var knownOutputPortId = Guid.NewGuid();
        string knownPortName = "Test.Port";
        string knownPortValue = "Test value";

        var orgChangeset = new ProjectAuditRecord
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes(10),
            ProjectId = projectId,
            Settings = new ProjectSettingsAuditRecord { IsForceResultCommunicationEnabled = true },
            Steps = new List<StepAuditRecord> {
                CreateStep(knownStepId, knownStepName, knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(knownPortId, knownPortName, knownPortValue)
                }),
                new StepAuditRecord {
                    Id = knownStep2Id,
                    Name = "Test2",
                    X = 3,
                    Y = 4,
                    AssemblyName = "Test.Assembly",
                    AssemblyVersion = "0.0.1",
                    TypeName = "Test.Type"
                },
                CreateStep(knownStep4Id, "PortTestStep1", knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(Guid.NewGuid(), "Test.Port1", "Test_123"),
                    CreatePort(knownOutputPortId, "Test.Output", "Test_123", direction: PortDirection.Output)
                }),
                CreateStep(knownStep5Id, "PortTestStep2", knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(knownPort2Id, knownPortName, "Test_123"),
                    CreatePort(knownInputPortId, "Test.Input", string.Empty)
                })
            },
            Links = new List<LinkAuditRecord>
            {

            }
        };

        var newChangeset = new ProjectAuditRecord
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ProjectId = projectId,
            Settings = new ProjectSettingsAuditRecord { IsForceResultCommunicationEnabled = true },
            Steps = new List<StepAuditRecord> {
                CreateStep(knownStepId, knownStepName, knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(knownPortId, knownPortName, knownPortValue)
                }),
                new StepAuditRecord {
                    Id = knownStep3Id,
                    Name = "Test3",
                    X = 5,
                    Y = 6,
                    AssemblyName = "Test.Assembly",
                    AssemblyVersion = "0.0.1",
                    TypeName = "Test.Type"
                },
                CreateStep(knownStep4Id, "PortTestStep1", knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(Guid.NewGuid(), "Test.Port2", "Test_456"),
                    CreatePort(knownOutputPortId, "Test.Output", "Test_123", direction: PortDirection.Output)
                }),
                CreateStep(knownStep5Id, "PortTestStep2", knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(knownPort2Id, knownPortName, "Test_abc"),
                    CreatePort(knownInputPortId, "Test.Input", string.Empty)
                })
            },
            Links = new List<LinkAuditRecord>
            {
                new LinkAuditRecord {
                    Id = Guid.NewGuid(),
                    SourceId = knownOutputPortId,
                    TargetId = knownInputPortId
                }
            }
        };

        projectAuditRecords.Add(orgChangeset);
        projectAuditRecords.Add(newChangeset);

        // Act
        IEnumerable<Data.Audit.Models.ChangeRecord> diffs = AgentCompareService.Compare(projectAuditRecords);

        // Assert
        Assert.NotNull(diffs);
        Assert.Single(diffs.GroupBy(d => d.ChangesetAId));
        Assert.Single(diffs.GroupBy(d => d.ChangesetBId));
        Assert.Equal(6, diffs.Count());
        Assert.Single(diffs.Where(d => d.ValueA.Equals("Test_123") && d.ValueB.Equals("Test_abc")));
        Assert.Single(diffs.Where(d => d.ValueA.Equals(string.Empty) && d.ValueB.Equals("Test_456")));
        Assert.Single(diffs.Where(d => d.ValueA.Equals("Test_123") && d.ValueB.Equals(string.Empty)));
    }

    [Fact]
    public void Test_CompareSingleChangeset()
    {
        // Arrange
        var projectAuditRecords = new List<ProjectAuditRecord>();

        var projectId = Guid.NewGuid();

        var knownStepId = Guid.NewGuid();
        var knownStep2Id = Guid.NewGuid();
        var knownStep3Id = Guid.NewGuid();
        var knownStep4Id = Guid.NewGuid();
        string knownStepName = "Known.Step";
        string knownStepAssemblyName = "Test.Assembly";
        string knownStepAssemblyVersion = "1.0.0";
        string knownStepTypeName = "Known.Step.Type";

        var knownPortId = Guid.NewGuid();
        var knownPort2Id = Guid.NewGuid();
        var knownInputPortId = Guid.NewGuid();
        var knownOutputPortId = Guid.NewGuid();
        string knownPortName = "Test.Port";
        string knownPortValue = "Test value";

        var newChangeset = new ProjectAuditRecord
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ProjectId = projectId,
            Settings = new ProjectSettingsAuditRecord { IsForceResultCommunicationEnabled = true },
            Steps = new List<StepAuditRecord> {
                CreateStep(knownStepId, knownStepName, knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(knownPortId, knownPortName, knownPortValue)
                }),
                new StepAuditRecord {
                    Id = knownStep2Id,
                    Name = "Test3",
                    X = 5,
                    Y = 6,
                    AssemblyName = "Test.Assembly",
                    AssemblyVersion = "0.0.1",
                    TypeName = "Test.Type"
                },
                CreateStep(knownStep3Id, "PortTestStep1", knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(Guid.NewGuid(), "Test.Port2", "Test_456"),
                    CreatePort(knownOutputPortId, "Test.Output", "Test_123", direction: PortDirection.Output)
                }),
                CreateStep(knownStep4Id, "PortTestStep2", knownStepAssemblyName, knownStepAssemblyVersion, knownStepTypeName,
                new List<PortAuditRecord> {
                    CreatePort(knownPort2Id, knownPortName, "Test_abc"),
                    CreatePort(knownInputPortId, "Test.Input", string.Empty)
                })
            },
            Links = new List<LinkAuditRecord>
            {
                new LinkAuditRecord {
                    Id = Guid.NewGuid(),
                    SourceId = knownOutputPortId,
                    TargetId = knownInputPortId
                }
            }
        };

        projectAuditRecords.Add(newChangeset);

        // Act
        IEnumerable<Data.Audit.Models.ChangeRecord> diffs = AgentCompareService.Compare(projectAuditRecords);

        // Assert
        Assert.NotNull(diffs);
        Assert.Single(diffs.GroupBy(d => d.ChangesetAId));
        Assert.Single(diffs.GroupBy(d => d.ChangesetBId));
        Assert.Equal(11, diffs.Count());
        Assert.Single(diffs.Where(d => d.ValueA.Equals(string.Empty) && d.ValueB.Equals("Ready")));
        Assert.Single(diffs.Where(d => d.ValueA.Equals(string.Empty) && d.ValueB.Equals("Test value")));
        Assert.Single(diffs.Where(d => d.ValueA.Equals(string.Empty) && d.ValueB.Equals("Test_456")));
        Assert.Single(diffs.Where(d => d.ValueA.Equals(string.Empty) && d.ValueB.Equals("Test_abc")));
        Assert.Empty(diffs.Where(d => !d.ValueA.Equals(string.Empty)));
    }

    [Fact]
    public void Test_CompareEmptyCollection()
    {
        // Arrange / Act
        IEnumerable<Data.Audit.Models.ChangeRecord> result = AgentCompareService.Compare(new List<ProjectAuditRecord>());

        // Assert
        Assert.Empty(result);
    }
}
