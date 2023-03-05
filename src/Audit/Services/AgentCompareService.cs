using System.Text.Json;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;

namespace AyBorg.Audit.Services;

public sealed class AgentCompareService : IAgentCompareService
{
    public IEnumerable<ChangeRecord> Compare(IEnumerable<ProjectAuditRecord> projectAuditRecords)
    {
        var changes = new List<ChangeRecord>();
        IEnumerable<IGrouping<Guid, ProjectAuditRecord>> groupedProjectAuditRecords = projectAuditRecords.GroupBy(r => r.ProjectId);

        foreach (IGrouping<Guid, ProjectAuditRecord> projectAuditGroup in groupedProjectAuditRecords)
        {
            if (projectAuditGroup.Count() == 1)
            {
                // No diff, just the whole project as changed
                changes.AddRange(AddNewProject(projectAuditGroup.First()));
            }
            else
            {
                ProjectAuditRecord? lastRecord = null;
                foreach (ProjectAuditRecord record in projectAuditGroup.OrderByDescending(c => c.Timestamp))
                {
                    if (lastRecord == null)
                    {
                        lastRecord = record;
                        continue;
                    }

                    changes.AddRange(Compare(record, lastRecord));
                    lastRecord = record;
                }
            }
        }

        return changes;
    }

    private static IEnumerable<ChangeRecord> Compare(ProjectAuditRecord projectAuditRecordA, ProjectAuditRecord projectAuditRecordB)
    {
        var result = new List<ChangeRecord>();
        if (projectAuditRecordA.ProjectState != projectAuditRecordB.ProjectState)
        {
            result.Add(new ChangeRecord
            {
                ChangesetAId = projectAuditRecordA.Id,
                ChangesetBId = projectAuditRecordB.Id,
                Label = "Project/State",
                SubLabel = "Project",
                ValueA = projectAuditRecordA.ProjectState.ToString(),
                ValueB = projectAuditRecordB.ProjectState.ToString()
            });
        }

        if (projectAuditRecordA.Settings != projectAuditRecordB.Settings)
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            result.Add(new ChangeRecord
            {
                ChangesetAId = projectAuditRecordA.Id,
                ChangesetBId = projectAuditRecordB.Id,
                Label = "Project/Settings",
                SubLabel = "Project",
                ValueA = JsonSerializer.Serialize(projectAuditRecordA.Settings, jsonOptions),
                ValueB = JsonSerializer.Serialize(projectAuditRecordB.Settings, jsonOptions)
            });
        }

        result.AddRange(CompareSteps(projectAuditRecordA, projectAuditRecordB));

        foreach (LinkAuditRecord linkRecordB in projectAuditRecordB.Links)
        {
            LinkAuditRecord? linkRecordA = projectAuditRecordA.Links.FirstOrDefault(l => l.Id.Equals(linkRecordB.Id));

            if (linkRecordA != null)
            {
                // Link must be equal, else it would not exists
                continue;
            }

            // New link
            result.Add(CreateAddedLink(linkRecordB, projectAuditRecordA, projectAuditRecordB));
        }

        foreach (LinkAuditRecord linkRecordA in projectAuditRecordA.Links)
        {
            LinkAuditRecord? linkRecordB = projectAuditRecordB.Links.FirstOrDefault(l => l.Id.Equals(linkRecordA.Id));

            if (linkRecordB != null)
            {
                // Link must be equal, else it would not exists
                continue;
            }

            // Removed link
            result.Add(CreateRemovedLink(linkRecordA, projectAuditRecordA, projectAuditRecordB));
        }

        return result;
    }

    private static IEnumerable<ChangeRecord> CompareSteps(ProjectAuditRecord projectAuditRecordA, ProjectAuditRecord projectAuditRecordB)
    {
        var result = new List<ChangeRecord>();
        var comparedSteps = new HashSet<StepAuditRecord>();

        foreach (StepAuditRecord stepB in projectAuditRecordB.Steps)
        {
            StepAuditRecord? stepA = projectAuditRecordA.Steps.FirstOrDefault(s => s.Id.Equals(stepB.Id));
            if (stepA == null)
            {
                // Step is new
                result.AddRange(AddNewStep(stepB, projectAuditRecordA.Id, projectAuditRecordB.Id));
                comparedSteps.Add(stepB);
                continue;
            }

            result.AddRange(CompareSteps(stepA, stepB, projectAuditRecordA.Id, projectAuditRecordB.Id));
            comparedSteps.Add(stepA);
            comparedSteps.Add(stepB);
        }

        foreach (StepAuditRecord stepA in projectAuditRecordA.Steps.Where(s => !comparedSteps.Any(c => c.Id.Equals(s))))
        {
            StepAuditRecord? stepB = projectAuditRecordB.Steps.FirstOrDefault(s => s.Id.Equals(stepA.Id));
            if (stepB == null)
            {
                // Step removed
                result.AddRange(RemoveOldStep(stepA, projectAuditRecordA.Id, projectAuditRecordB.Id));
                continue;
            }
        }

        return result;
    }

    private static IEnumerable<ChangeRecord> CompareSteps(StepAuditRecord stepA, StepAuditRecord stepB, Guid projectAuditRecordA, Guid projectAuditRecordB)
    {
        var result = new List<ChangeRecord>();
        var comparedPorts = new HashSet<PortAuditRecord>();
        foreach (PortAuditRecord portB in stepB.Ports)
        {
            PortAuditRecord? portA = stepA.Ports.FirstOrDefault(p => p.Id.Equals(portB.Id));
            if (portA == null)
            {
                // New port
                result.Add(CreatePortChange(portA!, portB, stepB.Name, projectAuditRecordA, projectAuditRecordB));
                comparedPorts.Add(portB);
                continue;
            }

            if (portA.Value != portB.Value)
            {
                result.Add(CreatePortChange(portA, portB, stepB.Name, projectAuditRecordA, projectAuditRecordB));
                comparedPorts.Add(portA);
                comparedPorts.Add(portB);
            }
        }

        foreach (PortAuditRecord? portA in stepA.Ports.Where(p => !comparedPorts.Any(c => c.Id.Equals(p.Id))))
        {
            PortAuditRecord? portB = stepB.Ports.FirstOrDefault(p => p.Id.Equals(portA.Id));
            if (portB == null)
            {
                // Port removed
                result.Add(CreatePortChange(portA, portB!, stepB.Name, projectAuditRecordA, projectAuditRecordB));
            }
        }

        return result;
    }

    private static IEnumerable<ChangeRecord> AddNewProject(ProjectAuditRecord projectAuditRecord)
    {
        var result = new List<ChangeRecord>
        {
            new ChangeRecord
            {
                ChangesetAId = Guid.Empty,
                ChangesetBId = projectAuditRecord.Id,
                Label = "Project/State",
                SubLabel = "Project",
                ValueA = string.Empty,
                ValueB = projectAuditRecord.ProjectState.ToString()
            },

            new ChangeRecord
            {
                ChangesetAId = Guid.Empty,
                ChangesetBId = projectAuditRecord.Id,
                Label = "Project/Settings",
                SubLabel = "Project",
                ValueA = string.Empty,
                ValueB = JsonSerializer.Serialize(projectAuditRecord.Settings, new JsonSerializerOptions { WriteIndented = true})
            }
        };

        foreach (StepAuditRecord stepRecord in projectAuditRecord.Steps)
        {
            result.AddRange(AddNewStep(stepRecord, Guid.Empty, projectAuditRecord.Id));
        }

        foreach (LinkAuditRecord linkRecord in projectAuditRecord.Links)
        {
            result.Add(CreateAddedLink(linkRecord, null!, projectAuditRecord));
        }

        return result;
    }

    private static ChangeRecord CreateAddedLink(LinkAuditRecord linkRecord, ProjectAuditRecord projectAuditRecordA, ProjectAuditRecord projectAuditRecordB)
    {
        StepAuditRecord sourceStep = projectAuditRecordB.Steps.First(s => s.Ports.Any(p => p.Id.Equals(linkRecord.SourceId)));
        StepAuditRecord targetStep = projectAuditRecordB.Steps.First(s => s.Ports.Any(p => p.Id.Equals(linkRecord.TargetId)));
        string sourceStepName = sourceStep.Name;
        string targetStepName = targetStep.Name;
        PortAuditRecord sourcePort = sourceStep.Ports.First(p => p.Id.Equals(linkRecord.SourceId));
        PortAuditRecord targetPort = targetStep.Ports.First(p => p.Id.Equals(linkRecord.TargetId));
        string sourcePortName = sourcePort.Name;
        string targetPortName = targetPort.Name;

        return new ChangeRecord
        {
            ChangesetAId = projectAuditRecordA?.Id ?? Guid.Empty,
            ChangesetBId = projectAuditRecordB.Id,
            Label = $"Project/Links/{sourceStepName}/{sourcePortName}",
            SubLabel = "Link",
            ValueA = string.Empty,
            ValueB = $"{sourceStepName}.{sourcePortName} -> {targetStepName}.{targetPortName}"
        };
    }

    private static ChangeRecord CreateRemovedLink(LinkAuditRecord linkRecord, ProjectAuditRecord projectAuditRecordA, ProjectAuditRecord projectAuditRecordB)
    {
        StepAuditRecord sourceStep = projectAuditRecordA.Steps.First(s => s.Ports.Any(p => p.Id.Equals(linkRecord.SourceId)));
        StepAuditRecord targetStep = projectAuditRecordA.Steps.First(s => s.Ports.Any(p => p.Id.Equals(linkRecord.TargetId)));
        string sourceStepName = sourceStep.Name;
        string targetStepName = targetStep.Name;
        PortAuditRecord sourcePort = sourceStep.Ports.First(p => p.Id.Equals(linkRecord.SourceId));
        PortAuditRecord targetPort = targetStep.Ports.First(p => p.Id.Equals(linkRecord.TargetId));
        string sourcePortName = sourcePort.Name;
        string targetPortName = targetPort.Name;

        return new ChangeRecord
        {
            ChangesetAId = projectAuditRecordA.Id,
            ChangesetBId = projectAuditRecordB.Id,
            Label = $"Project/Links/{sourceStepName}/{sourcePortName}",
            SubLabel = "Link",
            ValueA = $"{sourceStepName}.{sourcePortName} -> {targetStepName}.{targetPortName}",
            ValueB = string.Empty
        };
    }

    private static IEnumerable<ChangeRecord> AddNewStep(StepAuditRecord stepAuditRecord, Guid projectAuditRecordA, Guid projectAuditRecordB)
    {
        var result = new List<ChangeRecord>
        {
            new ChangeRecord {
                ChangesetAId = projectAuditRecordA,
                ChangesetBId = projectAuditRecordB,
                Label = $"Project/Steps/{stepAuditRecord.Name}",
                SubLabel = $"Step: {stepAuditRecord.Name} ({stepAuditRecord.AssemblyName}.{stepAuditRecord.TypeName})",
                ValueA = string.Empty,
                ValueB = JsonSerializer.Serialize(new StepInfo(stepAuditRecord), new JsonSerializerOptions { WriteIndented = true})
            }
        };

        foreach (PortAuditRecord portRecord in stepAuditRecord.Ports.Where(p => p.Direction == SDK.Common.Ports.PortDirection.Input))
        {
            result.Add(new ChangeRecord
            {
                ChangesetAId = projectAuditRecordA,
                ChangesetBId = projectAuditRecordB,
                Label = $"Project/Steps/{stepAuditRecord.Name}/{portRecord.Name}/Value",
                SubLabel = $"Port: {portRecord.Name}, Brand: {portRecord.Brand}",
                ValueA = string.Empty,
                ValueB = portRecord.Value
            });
        }

        return result;
    }

    private static IEnumerable<ChangeRecord> RemoveOldStep(StepAuditRecord stepAuditRecord, Guid projectAuditRecordA, Guid projectAuditRecordB)
    {
        var result = new List<ChangeRecord>
        {
            new ChangeRecord {
                ChangesetAId = projectAuditRecordA,
                ChangesetBId = projectAuditRecordB,
                Label = $"Project/Steps/{stepAuditRecord.Name}",
                SubLabel = $"Step: {stepAuditRecord.Name} ({stepAuditRecord.AssemblyName}.{stepAuditRecord.TypeName})",
                ValueA = JsonSerializer.Serialize(new StepInfo(stepAuditRecord), new JsonSerializerOptions { WriteIndented = true}),
                ValueB = string.Empty
            }
        };

        foreach (PortAuditRecord portRecord in stepAuditRecord.Ports.Where(p => p.Direction == SDK.Common.Ports.PortDirection.Input))
        {
            result.Add(CreatePortChange(portRecord, null!, stepAuditRecord.Name, projectAuditRecordA, projectAuditRecordB));
        }

        return result;
    }

    private static ChangeRecord CreatePortChange(PortAuditRecord portRecordA, PortAuditRecord portRecordB,
                                                string stepName, Guid projectAuditRecordA, Guid projectAuditRecordB)
    {
        string portName = portRecordA?.Name ?? portRecordB.Name;
        SDK.Common.Ports.PortBrand portBrand = portRecordA?.Brand ?? portRecordB.Brand;
        return new ChangeRecord
        {
            ChangesetAId = projectAuditRecordA,
            ChangesetBId = projectAuditRecordB,
            Label = $"Project/Steps/{stepName}/{portName}/Value",
            SubLabel = $"Port: {portName}, Brand: {portBrand}",
            ValueA = portRecordA?.Value ?? string.Empty,
            ValueB = portRecordB?.Value ?? string.Empty
        };
    }

    private sealed record StepInfo
    {
        public string Name { get; }
        public string TypeName { get; }
        public string AssemblyName { get; }
        public string AssemblyVersion { get; }

        public StepInfo(StepAuditRecord record)
        {
            Name = record.Name;
            TypeName = record.TypeName;
            AssemblyName = record.AssemblyName;
            AssemblyVersion = record.AssemblyVersion;
        }
    }
}
